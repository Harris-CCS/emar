using DomainModel;
using IdentityServer3.Core.Extensions;
using Interfaces.Utilities;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to help with authentication logic
    /// </summary>
    public class Authentication : IAuthUtility
    {
        /// <summary>
        /// Get the PulseCheck User ID associated with an authenticated user principal
        /// </summary>
        /// <param name="User">Authenticated user principal</param>
        /// <returns></returns>
        public int GetAuthenticatedUserId(IPrincipal User)
        {
            var caller = User as ClaimsPrincipal;
            if (caller == null)
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            var currentUserIdClaim = caller.Claims.FirstOrDefault(
                claim =>
                    string.Equals(claim.Type, Constants.PulseCheckClaims.PulseCheckUserId,
                        StringComparison.CurrentCultureIgnoreCase));

            int userId;
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out userId))
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            return userId;
        }

        /// <summary>
        /// Get the PulseCheck Site ID associated with an authenticated user principal
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public byte GetAuthenticatedUserSite(IPrincipal User)
        {
            var caller = User as ClaimsPrincipal;
            if (caller == null)
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            var subjectId = caller.GetSubjectId();
            var currentSiteClaim = caller.Claims.FirstOrDefault(
                claim =>
                    string.Equals(claim.Type, Constants.PulseCheckClaims.PulseCheckSiteId,
                        StringComparison.CurrentCultureIgnoreCase));

            return Convert.ToByte(currentSiteClaim.Value);
        }

        /// <summary>
        /// Get the user's account information
        /// </summary>
        /// <param name="userMappingInfo">A list of usermapping objects that apply to this user</param>
        /// <param name="username">Provided username</param>
        /// <param name="password">Provided password</param>
        /// <param name="domain">Provided windows domain name</param>
        /// <returns>An AccountInfo object</returns>
        public AccountInfo GetUserAccountInfo(List<IUserMapping> userMappingInfo, string username, string password, string domain)
        {
            // If we received a domain and userMappingInfo contains entries, we should be able to find an entry
            // in there where the username and domain match. If we do, then we can use that information to find
            // the API login username for this domain user.
            var domainLogin = false;
            var apiUsername = username;
            if (!string.IsNullOrWhiteSpace(domain))
            {
                foreach (var mapping in userMappingInfo)
                {
                    if (mapping.WindowsDomains.ToUpperInvariant().IndexOf(domain.ToUpperInvariant()) >= 0 &&
                        (mapping.DomainLogin.ToUpperInvariant().Equals(username.ToUpperInvariant()) || mapping.DomainLogin.ToUpperInvariant().Equals((domain + @"\" + username).ToUpperInvariant())))
                    {
                        apiUsername = mapping.Login;
                        domainLogin = true;
                        break;
                    }
                }
            }

            return new AccountInfo()
            {
                DomainLogin = domainLogin,
                APIUsername = apiUsername,
                Username = username,
                Password = password,
                Domain = domain,
            };
        }

        /// <summary>
        /// Check a given username and password against a domain
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="domain">Domain name</param>
        /// <returns>Boolean for whether the provided username and password are valid in the domain</returns>
        public bool ValidateDomainCredentials(string username, string password, string domain)
        {
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domain))
                {
                    var result = pc.ValidateCredentials(username, password);
                    return result;
                }
            } catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Object continaing information about a user's API account
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// Account object
        /// </summary>
        public object Account { get; set; }

        /// <summary>
        /// Flag for whether this is a domain login
        /// </summary>
        public bool DomainLogin { get; set; }

        /// <summary>
        /// Determined domain username
        /// </summary>
        public string DomainUsername { get; set; }

        /// <summary>
        /// Determined API username
        /// </summary>
        public string APIUsername { get; set; }

        /// <summary>
        /// Provided username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Provided password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Provided windows domain name
        /// </summary>
        public string Domain { get; set; }
    }
}