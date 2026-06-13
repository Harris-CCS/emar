using System.Collections.Generic;
using IdentityServer3.Core.Models;

namespace Host.Configuration.Config
{
    public class Clients
    {
        public static List<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = PulseCheck.Constants.Identifiers.APIClientName,
                    ClientId = PulseCheck.Constants.Identifiers.APIClientId,
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Reference,
                    IdentityTokenLifetime = 900,              // 15 minutes initially. Once the user picks a site, the site's timeout is used
                    AccessTokenLifetime = 900,
                    AbsoluteRefreshTokenLifetime = 28800,   // 8 hours
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    Flow = Flows.ResourceOwner,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(PulseCheck.Constants.Identifiers.APIClientSecret.Sha256())
                    },
                    RedirectUris = new List<string>
                    {
                        Addresses.GetAPIBaseAddress()
                    },
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        PulseCheck.Constants.Identifiers.APIScopeName,
                        "roles",
                        "AccountService",
                        "offline_access"
                    }
                },
                new Client
                {
                    ClientName = PulseCheck.Constants.Identifiers.PasswordChangeClientName,
                    ClientId = PulseCheck.Constants.Identifiers.PasswordChangeClientId,
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Reference,
                    IdentityTokenLifetime = 86400, // 24 hours
                    AccessTokenLifetime = 86400,
                    AbsoluteRefreshTokenLifetime = 86400,   
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    Flow = Flows.ResourceOwner,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(PulseCheck.Constants.Identifiers.PasswordChangeSecret.Sha256())
                    },
                    RedirectUris = new List<string>
                    {
                        Addresses.GetAPIBaseAddress()
                    },
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        PulseCheck.Constants.Identifiers.PasswordChangeScopeName,
                        "roles",
                        "AccountService",
                        "offline_access"
                    }
                },
                new Client
                {
                    ClientName = PulseCheck.Constants.Identifiers.WebClientName,
                    ClientId = PulseCheck.Constants.Identifiers.WebClientId,
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Reference,
                    IdentityTokenLifetime = 900,              // 15 minutes initially. Once the user picks a site, the site's timeout is used
                    AccessTokenLifetime = 900,
                    AbsoluteRefreshTokenLifetime = 28800,   // 8 hours
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    Flow = Flows.ResourceOwner,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(PulseCheck.Constants.Identifiers.WebClientSecret.Sha256())
                    },
                    RedirectUris = new List<string>
                    {
                        Addresses.GetAPIBaseAddress()
                    },
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        PulseCheck.Constants.Identifiers.WebScopeName,
                        "roles",
                        "AccountService",
                        "offline_access"
                    }
                }
            };
        }
    }
}