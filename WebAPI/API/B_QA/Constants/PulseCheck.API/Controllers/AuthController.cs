using IdentityModel.Client;
using System.Threading.Tasks;
using System.Web.Http;
using DomainModel;
using System.Net.Http;
using System.Collections.Generic;
using System.Web;
using PulseCheck.Constants;
using System;
using System.Security.Claims;
using IdentityServer3.Core.Extensions;
using Interfaces.Services;
using Interfaces.Repository;
using Services;
using System.Net;
using PulseCheck.Utilities;
using System.Data;
using DomainModel.Membership;
using System.Linq;
using Host.Configuration;
using PulseCheck.API.Actions;

namespace PulseCheck.API.Controllers
{
    /// <summary>
    /// Authentication controller for PulseCheck API
    /// </summary>
    public class AuthController : ApiController
    {
        private readonly IDeviceService _deviceService;
        private readonly ISiteService _siteService;
        private readonly IUserService _userService;
        private readonly IUserMappingRepository _userMapping;
        private readonly UserAccountService _userAccountService;
        private readonly Authentication _authUtil = new Authentication();

        /// <summary>
        /// AuthController constructor
        /// </summary>
        /// <param name="deviceService"></param>
        /// <param name="siteService"></param>
        /// <param name="userService"></param>
        /// <param name="userMapping"></param>
        /// <param name="userAccountService"></param>
        public AuthController(IDeviceService deviceService, ISiteService siteService, IUserService userService, IUserMappingRepository userMapping, UserAccountService userAccountService)
        {
            _deviceService = deviceService;
            _siteService = siteService;
            _userService = userService;
            _userMapping = userMapping;
            _userAccountService = userAccountService;
        }

        // POST: api/auth/login
        /// <summary>
        /// Post credentials for API
        /// </summary>
        /// <remarks>
        /// Get token to allow API access
        /// </remarks>
        /// <returns>
        /// Token information dictionary
        /// </returns>
        /// <response code="200"></response>
        [VersionedRoute("api/auth/login", 1)]
        [Route("api/v1/auth/login")]
        [HttpPost]
        [AllowAnonymous]
        [DisableRequestLogging]
        public async Task<IHttpActionResult> Post([FromBody]AuthCredentials credentials)
        {
            if (!string.IsNullOrEmpty(credentials.DeviceId))
            {
                var device = await _deviceService.GetDeviceByIdAsync(credentials.DeviceId);
                if (device == null || !device.IsAuthorized)
                {
                    Events.LogAPIEvent(Events.Constants.Events.INVALID_DEVICE, credentials.UserName, null, credentials.DeviceId);
                    return BadRequest("Login failed. Device is not authorized.");
                }
            }
            var domain = Settings.GetSetting("domain");
            if (!string.IsNullOrEmpty(domain) && string.IsNullOrEmpty(credentials.Domain))
                credentials.Domain = domain;          

            var userMappingInfo = await _userMapping.GetUserMappingInfo(credentials.UserName, credentials.Domain);

            var iuserMappingInfo = userMappingInfo.Select(x => (IUserMapping)x).ToList();
            var accountInfo = _authUtil.GetUserAccountInfo(iuserMappingInfo, credentials.UserName, credentials.Password, credentials.Domain);
            accountInfo.Account = _userAccountService.GetByUsername(accountInfo.APIUsername);

            var acct = (UserAccount)accountInfo.Account;
            if (acct == null)
            {
                Events.LogAPIEvent(Events.Constants.Events.NO_ACCOUNT, credentials.UserName);
                return BadRequest("Login failed. No valid account was found.");
            }

            if (userMappingInfo.Count < 1)
            {
                Events.LogAPIEvent(Events.Constants.Events.NO_MAPPED_USERS, credentials.UserName);
                return BadRequest("Login failed. No mapped PulseCheck users were found.");
            }

            bool pcLimitExceeded = true;
            bool apiLimitExceeded = true;
            var failureCount = 0;
            byte siteId = 0;
            var userId = 0;

            // If this user is locked out of the API across all their sites, don't let them in.
            // TODO: Maybe in the future this is a hardcoded value so we don't have to check every site the user could access.
            foreach (var userMapping in userMappingInfo)
            {
                if (acct.FailedLoginCount < userMapping.Retry)
                {
                    apiLimitExceeded = false;
                    break;
                } else if (acct.FailedLoginCount > failureCount)
                {
                    siteId = userMapping.SiteId;
                    failureCount = acct.FailedLoginCount;
                    userId = userMapping.Id;
                }
            }

            if (!apiLimitExceeded)
            {
                // If we cannot find a site where the user's failed login counter value is less than the site's retry limit, don't let them in.
                foreach (var userMapping in userMappingInfo)
                {
                    if (userMapping.Ctr < userMapping.Retry)
                    {
                        pcLimitExceeded = false;
                        break;
                    } else if (userMapping.Ctr > failureCount)
                    {
                        siteId = userMapping.SiteId;
                        failureCount = userMapping.Ctr;
                        userId = userMapping.Id;
                    }
                }
            }

            if (apiLimitExceeded || pcLimitExceeded)
            {
                Events.Log(siteId, userId, GetType().Name.ToString(), Events.Constants.Events.ACCOUNTLOCKED, failureCount + " failed login attempts");
                return BadRequest("Password retry limit exceeded. Please contact your system administrator/super user. Due to security restrictions, the PulseCheck support line cannot reset your password.");
            }

            var apiUsername = credentials.UserName;
            var apiPassword = credentials.Password;
            var validLogin = ValidateCredentials(accountInfo);
            if (validLogin)
            {
                Events.LogAPIEvent(Events.Constants.Events.LOGIN, credentials.UserName);
                // Need to be sure any PulseCheck constant claims are removed before generating token,
                // or else they will be stored in the new token, overriding versions stored in the DB.
                _userAccountService.RemovePulseCheckClaims(acct.ID);
                
                if (accountInfo.DomainLogin)
                {
                    apiUsername = acct.Username;
                    var user = _userAccountService.GetByUsername(apiUsername);
                    try
                    {
                        _userAccountService.SetPassword(new Guid(user.ID.ToString()), apiPassword);
                    }
                    catch
                    {
                        // Don't do anything.  There's no real way to see if the password has been changed to use
                        // the domain one, and it fails if it thinks the password is getting set to the current one
                    }
                }

                var token = await GetUserToken(apiUsername, apiPassword);
                _userAccountService.AddClaim(acct.ID, PulseCheckClaims.AccessToken, token.AccessToken);

                if (!string.IsNullOrWhiteSpace(credentials.DeviceId))  
                    _userAccountService.AddClaim(acct.ID, PulseCheckClaims.DeviceId, credentials.DeviceId);

                return Ok(new
                {
                    access_token = token.AccessToken,
                    error = token.Error,
                    expires_in = token.ExpiresIn,
                    id_token = token.IdentityToken,
                    refresh_token = token.RefreshToken,
                    http_error_reason = token.HttpErrorReason,
                    is_error = token.IsError,
                    http_error_status_code = token.HttpErrorStatusCode,
                    is_http_error = token.IsHttpError,
                    token_type = token.TokenType
                });
            }
            else
            {
                // An error occurred - try to send back more useful messages.
                return BadRequest("Login failed");
            }
        }

        // GET: api/auth/domains
        /// <summary>
        /// Get windows domains specified for each active site in the system
        /// </summary>
        /// <returns>Dictionary linking one or more domains to each site identifier</returns>
        [VersionedRoute("api/auth/domains", 1)]
        [Route("api/v1/auth/domains")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetDomainsV1()
        {
            var domainInfo = new List<Dictionary<string, object>>();
            var res = new DB.Select { Sql = "SELECT site, windowsdomains FROM org WHERE [status] = 'A' ORDER BY site" }.RunForDataSet();
            if (res != null)
            {
                foreach (DataRow dr in res.Tables[0].Rows)
                {
                    var domains = dr["windowsdomains"]?.ToString();
                    var site = Convert.ToByte(dr["site"]);
                    if (!string.IsNullOrWhiteSpace(domains))
                    {
                        var list = new List<string>(domains.Split(new char[] { ' ', ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries));
                        list.Sort();
                        domainInfo.Add(new Dictionary<string, object>
                        {
                            { "SiteId", site },
                            { "Domains", list }
                        });
                    }
                }
            }

            return Ok(domainInfo);
        }

        // POST: api/auth/stayinalive
        /// <summary>
        /// Get a new access token to extend API access lifetime or re-authenticate from a refresh token
        /// </summary>
        /// <remarks>
        /// Supply refresh token from previous authentication to receive a new access token
        /// </remarks>
        /// <returns>
        /// Token information dictionary
        /// </returns>
        /// <response code="200"></response>
        [VersionedRoute("api/auth/stayinalive", 1)]
        [Route("api/v1/auth/stayinalive")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostAuth([FromBody]Token token)
        {
            if (token.refresh_token != null && token.refresh_token.Length > 0)
            {
                var newToken = await RefreshToken(token.refresh_token);
                return Ok(new
                {
                    access_token = newToken.AccessToken,
                    error = newToken.Error,
                    expires_in = newToken.ExpiresIn,
                    id_token = newToken.IdentityToken,
                    refresh_token = newToken.RefreshToken,
                    http_error_reason = newToken.HttpErrorReason,
                    is_error = newToken.IsError,
                    http_error_status_code = newToken.HttpErrorStatusCode,
                    is_http_error = newToken.IsHttpError,
                    token_type = newToken.TokenType
                });
            } else
            {
                return BadRequest("Refresh token not found");
            }
        }

        // POST: api/auth/site
        /// <summary>
        /// Post additional credentials to log an API user in to a particular site
        /// </summary>
        /// <remarks>
        /// Calling this route will set the site information for an API user
        /// </remarks>
        /// <returns>
        /// Response status code defines success/failure
        /// </returns>
        /// <response code="200"></response>
        /// <response code="400"></response>
        [VersionedRoute("api/auth/site", 1)]
        [Route("api/v1/auth/site")]
        [HttpPost]
        public async Task<IHttpActionResult> PostSite([FromBody]SiteInfo siteInfo)
        {
            if (siteInfo == null || siteInfo.SiteId == 0)
            {
                return BadRequest("Invalid site selection");
            }

            var caller = User as ClaimsPrincipal;
            var subjectId = caller.GetSubjectId();
            var accessibleSites = await _siteService.GetSitesBySubjectIdAsync(subjectId);
            var requestedSite = siteInfo.SiteId;

            Guid id;
            if (!Guid.TryParse(subjectId, out id))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var userAccount = _userAccountService.GetByID(id);
            foreach (var s in accessibleSites) {
                if (s.Id == requestedSite)
                {
                    int userId = await _userMapping.GetSiteLoginUserNum(userAccount.Username, s.Id);
                    _userAccountService.RemovePulseCheckClaims(id);
                    _userAccountService.AddClaim(id, PulseCheckClaims.PulseCheckSiteId, s.Id.ToString());
                    _userAccountService.AddClaim(id, PulseCheckClaims.PulseCheckUserId, userId.ToString());

                    var user = await _userService.GetUserByIdAsync(userId);
                    var site = await _siteService.GetSiteByIdAsync(s.Id);
                    site.UserInfo = user;

                    Events.Log(site.Id, userId, GetType().Name.ToString(), Events.Constants.Events.LOGIN, null);

                    return Ok(new Dictionary<string, object>
                    {
                        { "Id", site.Id },
                        { "Name", site.Name },
                        { "Timeout", site.Timeout },
                        { "Refresh", site.Refresh },
                        { "UserInfo", site.UserInfo }
                    });
                }
            }

            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        // POST: api/auth/logout
        /// <summary>
        /// Revoke provided tokens to log a user out of the API.
        /// </summary>
        /// <remarks>
        /// Access token, refresh token, or both tokens can be supplied for revocation.
        /// </remarks>
        /// <returns>
        /// Response status code defines success/failure
        /// </returns>
        /// <response code="200"></response>
        [VersionedRoute("api/auth/logout", 1)]
        [Route("api/v1/auth/logout")]
        [HttpPost]
        public IHttpActionResult PostLogout([FromBody]Token token)
        {
            var revokeTokens = new List<Dictionary<string, string>>();
            if (token.access_token != null && token.access_token.Length > 0)
            {
                var tokenInfo = new Dictionary<string, string>();
                tokenInfo.Add("token_type_hint", "access_token");
                tokenInfo.Add("token", token.access_token);
                revokeTokens.Add(tokenInfo);
            }

            if (token.refresh_token != null && token.refresh_token.Length > 0)
            {
                var tokenInfo = new Dictionary<string, string>();
                tokenInfo.Add("token_type_hint", "refresh_token");
                tokenInfo.Add("token", token.refresh_token);
                revokeTokens.Add(tokenInfo);
            }

            var caller = User as ClaimsPrincipal;
            var subjectId = caller.GetSubjectId();

            bool revokedTokens = false;
            foreach (Dictionary<string, string> t in revokeTokens)
            {
                var client = GetClient(string.Format(Endpoints.TokenRevocationEndpoint, Addresses.GetIDServerBaseAddress()));
                var result = client.RequestAsync(t);
                revokedTokens = true;
            }

            if (revokedTokens)
            {
                Guid id;
                if (Guid.TryParse(subjectId, out id))
                {
                    _userAccountService.RemovePulseCheckClaims(id);
                }
                Request.GetOwinContext().Authentication.SignOut();
                return Ok();
            } else
            {
                return BadRequest("Token not found");
            }
        }

        private async Task<TokenResponse> GetUserToken(string username, string password)
        {
            var client = GetClient(string.Format(Endpoints.TokenEndpoint, Addresses.GetIDServerBaseAddress()));
            var scopes =
                Identifiers.APIScopeName + " " +
                IdentityServer3.Core.Constants.StandardScopes.OpenId + " " +
                IdentityServer3.Core.Constants.StandardScopes.Profile + " " +
                IdentityServer3.Core.Constants.StandardScopes.Roles + " " +
                IdentityServer3.Core.Constants.StandardScopes.OfflineAccess
            ;
            var result = await client.RequestResourceOwnerPasswordAsync(username, password, scopes);
            return result;
        }

        private async Task<TokenResponse> RefreshToken(string refreshToken)
        {
            var client = GetClient(string.Format(Endpoints.TokenEndpoint, Addresses.GetIDServerBaseAddress()));
            var result = await client.RequestRefreshTokenAsync(refreshToken);
            return result;
        }

        private TokenClient GetClient(string endpoint)
        {
            return new TokenClient(
                endpoint,
                Identifiers.APIClientId,
                Identifiers.APIClientSecret
            );
        }

        private void RemovePulseCheckClaims(Guid id)
        {
            _userAccountService.RemoveClaim(id, PulseCheckClaims.PulseCheckSiteId);
            _userAccountService.RemoveClaim(id, PulseCheckClaims.PulseCheckUserId);
            _userAccountService.RemoveClaim(id, PulseCheckClaims.DeviceId);
        }

        private bool ValidateCredentials(Utilities.AccountInfo accountInfo)
        {
            var validLogin = false;
            if (accountInfo.DomainLogin)
            {
                validLogin = _authUtil.ValidateDomainCredentials(accountInfo.Username, accountInfo.Password, accountInfo.Domain);
            }
            else
            {
                validLogin = _userAccountService.Authenticate(accountInfo.Username, accountInfo.Password);
            }

            return validLogin;
        }
    }

    /// <summary>
    /// Object containing information to post for an API user's site authentication
    /// </summary>
    public class SiteInfo
    {
        /// <summary>
        /// API user's site id/site number
        /// </summary>
        public int SiteId { get; set; }
    }

    /// <summary>
    /// Object containing information to post for an API user's token authentication
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Refresh token
        /// </summary>
        public string refresh_token { get; set; }

        /// <summary>
        /// Access token
        /// </summary>
        public string access_token { get; set; }
    }
}
