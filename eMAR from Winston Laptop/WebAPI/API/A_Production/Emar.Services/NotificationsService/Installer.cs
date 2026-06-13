using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;

namespace Emar.Services
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {

        private ServiceInstaller serviceAdmin;
        private EventLogInstaller eventLogInstaller;

        // Name that appears under "Applications and Services Logs" in Event Viewer
        public const string serviceLog = "PulseCheck eMAR";

        // Source that appears in Event Viewer entries
        public const string serviceSource = "Notifications Service";

        // Underlying name of service to install
        private const string serviceName = "eMARNotifications";

        // Displayed name of installed service
        private const string serviceDisplayName = "PulseCheck eMAR Notifications Service";

        // Displayed description of installed service
        private const string serviceDescription = "Generates and sends user notifications for the PulseCheck eMAR application";

        public Installer()
        {
            InitializeComponent();

            ServiceProcessInstaller process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;

            serviceAdmin = new ServiceInstaller();
            serviceAdmin.StartType = ServiceStartMode.Automatic;
            serviceAdmin.DelayedAutoStart = true;

            // Does not depend on SQL Server Service in case of different DB use in future
            serviceAdmin.ServicesDependedOn = new string[] {
                "EventLog"
            };

            serviceAdmin.ServiceName = serviceName;
            serviceAdmin.DisplayName = serviceDisplayName;
            serviceAdmin.Description = serviceDescription;

            eventLogInstaller = new EventLogInstaller();
            eventLogInstaller.Source = serviceSource;
            eventLogInstaller.Log = serviceLog;

            Installers.Add(serviceAdmin);
            Installers.Add(process);
            Installers.Add(eventLogInstaller);

            this.Committed += new InstallEventHandler(ServiceInstaller_Committed);
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            using (var serviceController = new ServiceController(serviceAdmin.ServiceName, Environment.MachineName))
            {
                serviceController.Start();
            }
        }

        void ServiceInstaller_Committed(object sender, InstallEventArgs e)
        {
            // Auto-start after installation
            var controller = new ServiceController(serviceName);
            controller.Start();
        }
    }
}
