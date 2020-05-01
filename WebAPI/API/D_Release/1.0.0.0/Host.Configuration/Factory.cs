using IdentityServer3.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Repositories;
using Host.Configuration.Config;
using Host.Configuration.Extensions;
using IdentityServer3.Core.Resources;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;
using IdentityServer3.EntityFramework;
using IdentityServer3.Core.Models;

namespace Host.Configuration
{
    public class Factory
    {
        public static IdentityServerServiceFactory Configure(string connectionString)
        {            
            var efConfig = new EntityFrameworkServiceOptions
            {
                ConnectionString = connectionString,
                //SynchronousReads = true
            };

            var factory = new IdentityServerServiceFactory();

            factory.ClaimsProvider =
                new Registration<IClaimsProvider>(typeof(CustomClaimsProvider));

            var scopeStore = new InMemoryScopeStore(Config.Scopes.Get());
            factory.ScopeStore = new Registration<IScopeStore>(resolver => scopeStore);

            var clientStore = new InMemoryClientStore(Clients.Get());
            factory.ClientStore = new Registration<IClientStore>(resolver => clientStore);

            OperationalDbContext context = new OperationalDbContext(connectionString);
            var tokenStore = new TokenHandleStore(context, scopeStore, clientStore);
            factory.TokenHandleStore = new Registration<ITokenHandleStore>(resolver => tokenStore);

            var refreshTokenStore = new RefreshTokenStore(context, scopeStore, clientStore);
            factory.RefreshTokenStore = new Registration<IRefreshTokenStore>(resolver => refreshTokenStore);

            factory.ConfigureCustomUserService(connectionString);

            // these two calls just pre-populate the test DB from the in-memory config
            ConfigureClients(Config.Clients.Get(), efConfig);
            ConfigureScopes(Config.Scopes.Get(), efConfig);

            factory.RegisterConfigurationServices(efConfig);
            factory.RegisterOperationalServices(efConfig);
                       
            factory.ConfigureClientStoreCache();
            factory.ConfigureScopeStoreCache();

            // This is where the CORS policy service is configured.
            var corsPolicyService = new DefaultCorsPolicyService();
            corsPolicyService.AllowAll = true;
            factory.CorsPolicyService = new Registration<ICorsPolicyService>(corsPolicyService);

            return factory;
        }

        public static void ConfigureClients(IEnumerable<Client> clients, EntityFrameworkServiceOptions options)
        {
            using (var db = new ClientConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Clients.Any())
                {
                    foreach (var c in clients)
                    {
                        var e = c.ToEntity();
                        db.Clients.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }

        public static void ConfigureScopes(IEnumerable<Scope> scopes, EntityFrameworkServiceOptions options)
        {
            using (var db = new ScopeConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Scopes.Any())
                {
                    foreach (var s in scopes)
                    {
                        var e = s.ToEntity();
                        db.Scopes.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }
    }
}
