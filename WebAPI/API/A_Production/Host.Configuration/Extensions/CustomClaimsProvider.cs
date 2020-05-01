using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Validation;

namespace Host.Configuration.Extensions
{
    public class CustomClaimsProvider : DefaultClaimsProvider
    {
        public CustomClaimsProvider(IUserService users) : base(users)
        { }

        public override Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes,
            bool includeAllIdentityClaims, ValidatedRequest request)
        {
            return base.GetIdentityTokenClaimsAsync(subject, client, scopes, includeAllIdentityClaims, request);
        }

        public override async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, ValidatedRequest request)
        {
            var claims =  await base.GetAccessTokenClaimsAsync(subject, client, scopes, request);

            var newClaims = claims.ToList();

            var pulseCheckUserId = subject.FindFirst(PulseCheck.Constants.PulseCheckClaims.PulseCheckUserId);
            if (pulseCheckUserId != null)
            {
                newClaims.Add(pulseCheckUserId);
            }

            var pulseCheckSiteId = subject.FindFirst(PulseCheck.Constants.PulseCheckClaims.PulseCheckSiteId);
            if (pulseCheckSiteId != null)
            {
                newClaims.Add(pulseCheckSiteId);
            }

            return newClaims;

        }
    }
}
