using System.Configuration;
using Autofac;
using PulseCheck.Data;
using PulseCheck.Data.Repositories;
using PulseCheck.ILogic;
using PulseCheck.IRepository;
using PulseCheck.Logic;

namespace PulseCheck.IOC.Mappings
{
    public class RegisterApplicationIocAutoFac : Module
    {
        private readonly IComponentContext _kernel; // A service for resolving dependencies required by this module

        public RegisterApplicationIocAutoFac(IComponentContext componentContext)
        {
            _kernel = componentContext;
        }

        public RegisterApplicationIocAutoFac()
        {
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
            builder.RegisterType<SiteManager>()
                .As<ISiteManager>()
                .InstancePerRequest();

            builder.RegisterType<UserManager>()
                .As<IUserManager>()
                .InstancePerRequest();

            builder.RegisterType<UserAccountManager>()
                .As<IUserAccountManager>()
                .InstancePerRequest();

            builder.RegisterType<PatientService>()
                .As<IPatientManager>()
                .InstancePerRequest();

            builder.RegisterType<MedicationManager>()
                .As<IMedicationManager>()
                .InstancePerRequest();

            builder.RegisterType<DeviceManager>()
                .As<IDeviceManager>()
                .InstancePerRequest();

            builder.RegisterType<AuthenticationManager>()
                .As<IAuthenticationManager>()
                .InstancePerRequest();

            builder.RegisterType<EmailManager>()
                .As<IEmailManager>()
                .InstancePerRequest();

            //Memebership Reboot
            builder.RegisterType<UserAccountManager>()
                .InstancePerRequest();

            builder.RegisterType<UserAccountRepository>()
                .InstancePerRequest();

            builder.RegisterType<MembershipDatabase>()
                .WithParameter("name", ConfigurationManager.ConnectionStrings["PulseCheck.Membership"].ConnectionString)
                .InstancePerRequest();  
        }
    }
}
