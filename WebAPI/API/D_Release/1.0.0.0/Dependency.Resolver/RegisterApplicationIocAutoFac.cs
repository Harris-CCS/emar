using Autofac;
using Data;
using Data.Repositories;
using Interfaces.Repository;
using Interfaces.Services;
using Services;
using System.Configuration;

namespace Dependency.Resolver
{
    public class RegisterApplicationIocAutoFac : Module
    {
        private readonly IComponentContext _kernel; // A service for resolving dependencies required by this module

        public RegisterApplicationIocAutoFac(IComponentContext componentContext)
        {
            _kernel = componentContext;
        }

        protected override void Load(ContainerBuilder builder)
        {
            //DbContext 
            builder.RegisterType<IbexContext>()
                .InstancePerRequest();

            //Repositorires
            builder.RegisterType<SiteRepository>()
                .As<ISiteRepository>()
                .InstancePerRequest();

            builder.RegisterType<DepartmentRepository>()
                .As<IDepartmentRepository>()
                .InstancePerRequest();

            builder.RegisterType<UserRepository>()
                .As<IUserRepository>()
                .InstancePerRequest();

            builder.RegisterType<UserMappingRepository>()
                .As<IUserMappingRepository>()
                .InstancePerRequest();

            builder.RegisterType<AreaRepository>()
                .As<IAreaRepository>()
                .InstancePerRequest();

            builder.RegisterType<PatientRepository>()
                .As<IPatientRepository>()
                .InstancePerRequest();

            builder.RegisterType<MedicationRepository>()
                .As<IMedicationRepository>()
                .InstancePerRequest();

            builder.RegisterType<DeviceRepository>()
                .As<IDeviceRepository>()
                .InstancePerRequest();

            //Services
            builder.RegisterType<SiteService>()
                .As<ISiteService>()
                .InstancePerRequest();

            builder.RegisterType<UserService>()
                .As<IUserService>()
                .InstancePerRequest();

            builder.RegisterType<UserAccountService>()
                .As<IUserAccountService>()
                .InstancePerRequest();

            builder.RegisterType<PatientService>()
                .As<IPatientService>()
                .InstancePerRequest();

            builder.RegisterType<MedicationService>()
                .As<IMedicationService>()
                .InstancePerRequest();

            builder.RegisterType<DeviceService>()
                .As<IDeviceService>()
                .InstancePerRequest();

            builder.RegisterType<AuthenticationService>()
                .As<IAuthenticationService>()
                .InstancePerRequest();

            builder.RegisterType<EmailService>()
                .As<IEmailService>()
                .InstancePerRequest();

            //Memebership Reboot
            builder.RegisterType<UserAccountService>()
                .InstancePerRequest();

            builder.RegisterType<UserAccountRepository>()
                .InstancePerRequest();

            builder.RegisterType<MembershipDatabase>()
                .WithParameter("name", ConfigurationManager.ConnectionStrings["PulseCheck.Membership"].ConnectionString)
                .InstancePerRequest();            
        }
    }
}
