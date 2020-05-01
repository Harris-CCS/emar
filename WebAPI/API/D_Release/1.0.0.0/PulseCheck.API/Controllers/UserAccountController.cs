using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using DomainModel;
using Interfaces.Repository;
using Interfaces.Services;
using System.Net;
using PulseCheck.API.Models;
using System.Security.Claims;
using IdentityServer3.Core.Extensions;
using IdentityModel.Client;
using PulseCheck.Constants;
using Host.Configuration;
using PulseCheck.API.Actions;

namespace PulseCheck.API.Controllers
{
    /// <summary>
    /// Contoller for User Accounts
    /// </summary>
    public class UserAccountController : ApiController
    {
        private readonly ISiteService _siteService;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepo;
        private readonly IUserMappingRepository _userMappingRepo;
        private readonly Services.UserAccountService _userAccountService;
        private readonly IAuthenticationService _authService;
        private readonly IDeviceService _deviceService;
        private readonly IEmailService _emailService;

        /// <summary>
        /// UserAccountController constructor
        /// </summary>
        /// <param name="siteService"></param>
        /// <param name="userService"></param>
        /// <param name="userAccountService"></param>
        /// <param name="mappingRepo"></param>
        /// <param name="userRepo"></param>
        /// <param name="authService"></param>
        /// <param name="deviceService"></param>
        /// <param name="emailService"></param>
        public UserAccountController(ISiteService siteService, IUserService userService, IUserRepository userRepo, IUserMappingRepository mappingRepo, Services.UserAccountService userAccountService, IAuthenticationService authService, IDeviceService deviceService, IEmailService emailService)
        {
            _siteService = siteService;
            _userService = userService;
            _userRepo = userRepo;
            _userMappingRepo = mappingRepo;
            _userAccountService = userAccountService;
            _authService = authService;
            _deviceService = deviceService;
            _emailService = emailService;
        }

        /// <summary>
        /// Create a new master user account
        /// </summary>
        /// <returns>The newly created account</returns>
        [VersionedRoute("api/account/create", 1)]
        [Route("api/v1/account/create")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> CreateNewAccount([FromBody]DomainModel.Membership.UserAccountConfiguration newAccount)
        {
            var user = await GetValidatedWebUser();
            var password = Utilities.Security.GeneratePassword();
            var account = _userAccountService.CreateUserAccount(newAccount, password);

            var devicePasscode = _deviceService.CreateAuthorizationCode();
            var token = await GeneratePasswordToken(account, password);
            // TODO: something if the e-mail wasn't sent
            var sentMail = await _emailService.SendNewAccountEmail(account, token.AccessToken, devicePasscode);

            return Ok(account);
        }

        /// <summary>
        /// Get a master user account's PulseCheck users
        /// </summary>
        /// <returns>The PulseCheck users for the master user</returns>
        [VersionedRoute("api/account/{accountId}/users", 1)]
        [Route("api/v1/account/{accountId}/users")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetAccountUsers(Guid accountId)
        {
            var user = await GetValidatedWebUser();
            var account = _userAccountService.GetByID(accountId);
            if (account == null)
                return new ErrorResponse("Account not found", 404, Request);

            var userList = await _userMappingRepo.GetUserMappingInfo(account.Username, "");
            return Ok(userList);
        }

        /// <summary>
        /// Add a PulseCheck user to the account
        /// </summary>
        /// <param name="accountId">Account the user is being added to</param>
        /// <param name="user">User getting added</param>
        /// <returns></returns>
        [VersionedRoute("api/account/{accountId}/users", 1)]
        [Route("api/v1/account/{accountId}/users")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> AddPulseCheckUser(Guid accountId, [FromBody]UserMapping user)
        {
            var validUser = await GetValidatedWebUser();
            var account = _userAccountService.GetByID(accountId);
            if (account == null)
                return new ErrorResponse("Account not found", 404, Request);

            _userMappingRepo.AddAccountUser(account.Username, user.Id);
            return Ok();
        }

        /// <summary>
        /// Remove PulseCheck user mapping from the account
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [VersionedRoute("api/account/{accountId}", 1)]
        [Route("api/v1/account/{accountId}")]
        [HttpDelete]
        [AllowAnonymous]
        public async Task<IHttpActionResult> RemoveAccount(Guid accountId)
        {
            var user = await GetValidatedWebUser();
            var account = _userAccountService.GetByID(accountId);
            if (account == null)
                return new ErrorResponse("Account not found", 404, Request);

            if (_userMappingRepo.RemoveAllAccountUsers(account.Username))
            {
                _userAccountService.DeleteAccount(accountId);
                return Ok();
            }

            return BadRequest("Failed to remove account");
        }

        /// <summary>
        /// Remove PulseCheck user mapping from the account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [VersionedRoute("api/account/{accountId}/users/{userId}", 1)]
        [Route("api/v1/account/{accountId}/users/{userId}")]
        [HttpDelete]
        [AllowAnonymous]
        public async Task<IHttpActionResult> RemoveAccountUser(Guid accountId, int userId)
        {
            var user = await GetValidatedWebUser();
            var account = _userAccountService.GetByID(accountId);
            if (account == null)
                return new ErrorResponse("Account not found", 404, Request);

            if (_userMappingRepo.RemoveAccountUser(account.Email, userId))
                return Ok();


            return BadRequest("Failed to remove account user");
        }

        /// <summary>
        /// Get a master user account's PulseCheck users
        /// </summary>
        /// <returns>The PulseCheck users for the master user</returns>
        [VersionedRoute("api/account/{accountId}", 1)]
        [Route("api/v1/account/{accountId}")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<IHttpActionResult> UpdateAccount(Guid accountId, [FromBody]DomainModel.Membership.UserAccount updatedAccount)
        {
            var user = await GetValidatedWebUser();
            var existingAccount = _userAccountService.GetByID(accountId);
            if (existingAccount == null)
                return new ErrorResponse("Account not found", 404, Request);

            _userAccountService.ChangeUserAccount(existingAccount, updatedAccount);
            return Ok();
        }

        /// <summary>
        /// Get the master user accounts
        /// </summary>
        /// <returns>The list of master user accounts</returns>
        [VersionedRoute("api/account/{accountId}/users/search", 1)]
        [Route("api/v1/account/{accountId}/users/search")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SearchUsersForAccount(Guid accountId, string name)
        {
            var user = await GetValidatedWebUser();
            var account = _userAccountService.GetByID(accountId);
            if (account == null)
                return new ErrorResponse("Account not found", 404, Request);

            var userList = await _userRepo.SearchUsersForAccount(account.Username, name);
            return Ok(userList);
        }


        /// <summary>
        /// Update an account's password
        /// </summary>
        /// <returns>The list of master user accounts</returns>
        [VersionedRoute("api/account/password", 1)]
        [Route("api/v1/account/password")]
        [HttpPut]
        [DisableRequestLogging]
        public IHttpActionResult UpdatePassword([FromBody]string password)
        {
            var caller = User as ClaimsPrincipal;
            var subjectId = caller.GetSubjectId();

            Guid accountId;
            if (!Guid.TryParse(subjectId, out accountId))
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            var account = _userAccountService.GetByID(accountId);
            if (account == null)
                return new ErrorResponse("Account not found", 404, Request);

            var updateError = _userAccountService.UpdatePassword(account, password);
            if (updateError != null)
                return new ErrorResponse(updateError, 500, Request);

            return Ok();
        }

        /// <summary>
        /// Send a device authorization e-mail to an account
        /// </summary>
        /// <returns>The list of master user accounts</returns>
        [VersionedRoute("api/account/{accountId}/send-authorization", 1)]
        [Route("api/v1/account/{accountId}/send-authorization")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SendDeviceAuthorization(Guid accountId)
        {
            var account = _userAccountService.GetByID(accountId);
            if (account == null)
                return new ErrorResponse("Account not found", 404, Request);

            var devicePasscode = _deviceService.CreateAuthorizationCode();
            var sentMail = await _emailService.SendDeviceAuthorizationEmail(account, devicePasscode);
            if (sentMail)
                return Ok();

            return BadRequest("Failed to send device authorization e-mail");
        }

        /// <summary>
        /// Send a device authorization e-mail to an account
        /// </summary>
        /// <returns>The list of master user accounts</returns>
        [VersionedRoute("api/account/{accountId}/password-reset", 1)]
        [Route("api/v1/account/{accountId}/password-reset")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ResetPassword(Guid accountId)
        {
            var account = _userAccountService.GetByID(accountId);
            if (account == null)
                return new ErrorResponse("Account not found", 404, Request);

            var password = Utilities.Security.GeneratePassword();
            _userAccountService.SetPassword(account.ID, password);

            var token = await GeneratePasswordToken(account, password);
            var sentMail = await _emailService.SendAccountPasswordResetEmail(account, token.AccessToken);
            if (sentMail)
                return Ok();

            return BadRequest("Failed to send password reset e-mail");
        }

        /// <summary>
        /// Get the master user accounts
        /// </summary>
        /// <returns>The list of master user accounts</returns>
        [VersionedRoute("api/accounts", 1)]
        [Route("api/v1/accounts")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetAccounts()
        {
            var user = await GetValidatedWebUser();
            var accounts = _userAccountService.GetUserAccounts();
            return Ok(accounts);
        }

        private async Task<User> GetValidatedWebUser()
        {            
            var user = await _authService.GetValidatedWebUser();
            if (user == null || !user.CanNavigateTo(Navigation.Constants.ACCOUNT_ADMIN))
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            return user;
        }

        private async Task<TokenResponse> GeneratePasswordToken(DomainModel.Membership.UserAccount account, string password)
        {
            var client = new TokenClient(
                string.Format(Endpoints.TokenEndpoint, Addresses.GetIDServerBaseAddress()),
                Identifiers.PasswordChangeClientId,
                Identifiers.PasswordChangeSecret
            );
            var scopes =
                Identifiers.PasswordChangeScopeName + " " +
                IdentityServer3.Core.Constants.StandardScopes.OpenId + " " +
                IdentityServer3.Core.Constants.StandardScopes.Profile + " " +
                IdentityServer3.Core.Constants.StandardScopes.Roles + " " +
                IdentityServer3.Core.Constants.StandardScopes.OfflineAccess
            ;
            var result = await client.RequestResourceOwnerPasswordAsync(account.Username, password, scopes);
            return result;
        }
    }
}
