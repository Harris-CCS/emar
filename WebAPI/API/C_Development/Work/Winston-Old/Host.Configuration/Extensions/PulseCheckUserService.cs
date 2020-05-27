using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.MembershipReboot;
using Microsoft.Owin;
using PulseCheck.Data.Repositories;
using PulseCheck.Domain.Membership;
using PulseCheck.Logic;

namespace Host.Configuration.Extensions
{
    public static class CustomUserServiceExtensions
    {
        public static void ConfigureCustomUserService(this IdentityServerServiceFactory factory, string connectionString)
        {
            factory.Register(new Registration<MembershipDatabase>(resolver => new MembershipDatabase(connectionString)));
            factory.Register(new Registration<UserAccountRepository>());
            factory.Register(new Registration<UserAccountManager>());

            factory.UserService = new Registration<IUserService, CustomUserService>()
            {
                Mode = RegistrationMode.InstancePerHttpRequest
            };
        }
    }

    public class CustomUserService : MembershipRebootUserService<UserAccount>
    {
        private readonly UserAccountManager _userAccountSvc;
        OwinContext ctx;

        public CustomUserService(OwinEnvironmentService owinEnv, UserAccountManager userSvc) : base(userSvc)
        {
            ctx = new OwinContext(owinEnv.Environment);
            _userAccountSvc = userSvc;
        }

        public override Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            var id = ctx.Request.Query.Get("signin");
            context.AuthenticateResult = new AuthenticateResult("~/custom/login?id=" + id, (IEnumerable<Claim>)null);
            return Task.FromResult(0);
        }

        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            if (_userAccountSvc.Authenticate(context.UserName, context.Password))
            {
                var user = _userAccountSvc.GetByUsername(context.UserName);
                context.AuthenticateResult = new AuthenticateResult(user.ID.ToString(), user.Username);
            }
            return Task.FromResult(0);
        }

        public override Task PostAuthenticateAsync(PostAuthenticationContext context)
        {            
            return base.PostAuthenticateAsync(context);
        }

        protected override IEnumerable<Claim> GetClaimsFromAccount(UserAccount account)
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, GetSubjectForAccount(account)),
                new Claim(Constants.ClaimTypes.UpdatedAt,
                    IdentityModel.EpochTimeExtensions.ToEpochTime(account.LastUpdated).ToString(),
                    ClaimValueTypes.Integer),
                new Claim("tenant", account.Tenant),
                new Claim(Constants.ClaimTypes.PreferredUserName, account.Username),
            };

            if (!String.IsNullOrWhiteSpace(account.Email))
            {
                claims.Add(new Claim(Constants.ClaimTypes.Email, account.Email));
                claims.Add(new Claim(Constants.ClaimTypes.EmailVerified, account.IsAccountVerified ? "true" : "false"));
            }

            if (!String.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumber, account.MobilePhoneNumber));
                claims.Add(new Claim(Constants.ClaimTypes.PhoneNumberVerified, !String.IsNullOrWhiteSpace(account.MobilePhoneNumber) ? "true" : "false"));
            }

            claims.AddRange(account.Claims.Select(x => new Claim(x.Type, x.Value)));
            claims.AddRange(userAccountService.MapClaims(account));

            return claims;
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext ctx)
        {
            var subject = ctx.Subject;
            var requestedClaimTypesList = ctx.RequestedClaimTypes.ToList();
            requestedClaimTypesList.Add(PulseCheck.Constants.PulseCheckClaims.PulseCheckSiteId);
            requestedClaimTypesList.Add(PulseCheck.Constants.PulseCheckClaims.PulseCheckUserId);
            var requestedClaimTypes = requestedClaimTypesList.AsEnumerable();

            var acct = userAccountService.GetByID(subject.GetSubjectId().ToGuid());
            if (acct == null)
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            var claims = GetClaimsFromAccount(acct);
            claims = claims.Where(x => requestedClaimTypes.Contains(x.Type));
            ctx.IssuedClaims = claims;

            return Task.FromResult(0);
        }
    }
}