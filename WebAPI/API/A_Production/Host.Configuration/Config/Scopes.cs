using System.Collections.Generic;
using DomainModel.Membership;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;

namespace Host.Configuration.Config
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new[]
            {
                ////////////////////////
                // identity scopes
                ////////////////////////

                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.Address,
                StandardScopes.OfflineAccess,
                StandardScopes.RolesAlwaysInclude,
                StandardScopes.AllClaims,

                ////////////////////////
                // resource scopes
                ////////////////////////
                
                new Scope
                {
                    Enabled = true,
                    Name = PulseCheck.Constants.Identifiers.APIScopeName,
                    DisplayName = "PulseCheck API",
                    Description = "Access to the PulseCheck API",
                    Type = ScopeType.Resource,
                    Claims = new List<ScopeClaim>()
                    {
                        new ScopeClaim(PulseCheck.Constants.PulseCheckClaims.PulseCheckUserId),
                        new ScopeClaim(PulseCheck.Constants.PulseCheckClaims.PulseCheckSiteId),
                        new ScopeClaim(PulseCheck.Constants.PulseCheckClaims.DeviceId)
                    }
                },
                new Scope
                {
                    Enabled = true,
                    Name = PulseCheck.Constants.Identifiers.PasswordChangeScopeName,
                    DisplayName = "Account Password change",
                    Description = "Ability to change account password",
                    Type = ScopeType.Resource,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(PulseCheck.Constants.PulseCheckClaims.PulseCheckUserId),
                        new ScopeClaim(PulseCheck.Constants.PulseCheckClaims.PulseCheckSiteId)
                    }
                }
            };
        }
    }
}