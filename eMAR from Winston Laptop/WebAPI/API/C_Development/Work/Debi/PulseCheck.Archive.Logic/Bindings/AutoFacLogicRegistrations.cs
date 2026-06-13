using System;
using System.Configuration;
using Autofac;
using PulseCheck.Archive.Data;
using PulseCheck.Archive.Domain;
using PulseCheck.Data.Common.Database;

namespace PulseCheck.Archive.Logic.Bindings
{
    public class AutoFacLogicRegistrations : Module
    {
        private readonly IComponentContext _kernel;
        public static IContainer Container { get; set; }

        public AutoFacLogicRegistrations()
        {
        }

        public AutoFacLogicRegistrations(IComponentContext componentContext)
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
            string ibexArchiveConncetionString = ConfigurationManager.ConnectionStrings["IbexArchiveConnection"].ConnectionString;
            string ibexArchiveProvidername = ConfigurationManager.ConnectionStrings["IbexArchiveConnection"].ProviderName;
            string ibexConncetionString = ConfigurationManager.ConnectionStrings["IbexConnection"].ConnectionString;
            string ibexProvidername = ConfigurationManager.ConnectionStrings["IbexConnection"].ProviderName;

            IIbexArchiveConnectionSettings ibexArchiveConnectionSettings = new IbexArchiveConnectionSettings(ibexArchiveConncetionString, ibexArchiveProvidername);
            builder.RegisterInstance(ibexArchiveConnectionSettings).As<IIbexArchiveConnectionSettings>();

            IIbexConnectionSettings ibexConnectionSettings = new IbexConnectionSettings(ibexConncetionString, ibexProvidername);
            builder.RegisterInstance(ibexConnectionSettings).As<IIbexConnectionSettings>();



            builder.RegisterType<IbexArchiveRepository>().As<IIbexArchiveRepository>();
            builder.RegisterType<IbexRepository>().As<IIbexRepository>();
            builder.RegisterType<IbexArchiveManager>().As<IArchiveManager>();
            
        }
    }
}
