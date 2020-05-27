using System;
using System.Configuration;
using Autofac;
using PulseCheck.Data.Common.Caching;
using PulseCheck.Data.Common.Configuration;
using PulseCheck.Data.Common.Database;
using PulseCheck.Data.Common.Rest;
using PulseCheck.QCPR.Data.Repository;
using PulseCheck.QCPR.Domain.Constants;
using PulseCheck.QCPR.Domain.Contract;
using PulseCheck.QCPR.Logic.Managers;

namespace PulseCheck.QCPR.Logic.Bindings
{
    public class AutoFacQcprLogicRegistrations : Module
    {
        private readonly IComponentContext _kernel;
        public static IContainer Container { get; set; }

        public AutoFacQcprLogicRegistrations()
        {
        }

        public AutoFacQcprLogicRegistrations(IComponentContext componentContext)
        {
            _kernel = componentContext;
        }

        public static T GetType<T>(IContainer container)
        where T: class
        {
            if(container == null)
                throw new NullReferenceException($"{nameof(container)} has not been loaded");

            using (var scope = container.BeginLifetimeScope())
            {
                return scope.Resolve<T>();
            }
        }

        public T GetType<T>()
            where T : class
        {
            if (Container == null)
                throw new NullReferenceException($"{nameof(Container)} has not been loaded");

            using (var scope = Container.BeginLifetimeScope())
            {
                return scope.Resolve<T>();
            }
        }


        public void LoadContainer()
        {
            var builder = new ContainerBuilder();
            Load(builder);
            Container = builder.Build();
        }

        protected override void Load(ContainerBuilder builder)
        {
            string conncetionString = ConfigurationManager.ConnectionStrings["IbexArchiveConnection"].ConnectionString;
            string providername = ConfigurationManager.ConnectionStrings["IbexArchiveConnection"].ProviderName;
            IIbexArchiveConnectionSettings ibexArchiveConnectionSettings = new IbexArchiveConnectionSettings(conncetionString, providername);
            builder.RegisterInstance(ibexArchiveConnectionSettings).As<IIbexArchiveConnectionSettings>();

            if (Settings.GetBool(AppSettingConstants.UseRedisCache))
            {
                IRedisCache redisCache = new RedisCache();
            
                if (redisCache.IsConnected())
                {
                    builder.RegisterInstance<IRedisCache>(redisCache);
                }
            }

            string qcprBaseUrl = Settings.GetString(AppSettingConstants.QcprBaseUrl);

            if (!string.IsNullOrEmpty(qcprBaseUrl))
            {
                IRestConnection qcprRestConnection = new RestConnection() {BaseUrl = qcprBaseUrl};
                builder.RegisterInstance(qcprRestConnection);
                builder.RegisterType<RestSharpHandler>().As<IRestSharpHandler>();
                builder.RegisterType<QcprInterfaceRepository>().As<IQcprInterfaceRepository>();
            }

            builder.RegisterType<ImportRepository>().As<IImportRepository>();
            builder.RegisterType<QcprManager>().As<IQcprManager>();
            
        }
    }
}
