using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;

namespace Emar.Services.NotificationsService
{
    public partial class Service : ServiceBase
    {
        private EventLog Logger;
        private Timer timer;

        public Service()
        {
            InitializeComponent();

            Logger = new EventLog();
            Logger.Source = Installer.serviceSource;
            Logger.Log = Installer.serviceLog;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            Logger.WriteEntry("Service starting", EventLogEntryType.Information);

            // Set up a timer that triggers based on configuration
            timer = new Timer();

            // Default to trigger every 60 seconds.
            int timerInterval = 60000;

            try
            {
                string serviceIntervalSeconds = ConfigurationManager.AppSettings.Get("serviceIntervalSeconds");
                if (serviceIntervalSeconds != null && serviceIntervalSeconds.Length > 0)
                {
                    int seconds;
                    int.TryParse(serviceIntervalSeconds, out seconds);
                    if (seconds >= 0)
                    {
                        timerInterval = seconds * 1000;
                    }
                }
            } catch (Exception)
            {
                Logger.WriteEntry("Configured serviceIntervalSeconds invalid. Switching to default", EventLogEntryType.Warning);
            }

            timer.Interval = timerInterval; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Enabled = true;
            timer.Start();

            Logger.WriteEntry("Service started with " + (timerInterval / 1000) + " second interval", EventLogEntryType.Information);
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            Logger.WriteEntry("Notifications check starting", EventLogEntryType.Information);

            int notificationCount = 0;

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlConnection"].ConnectionString))
                {
                    using (var comm = new SqlCommand("generate_notifications", conn))
                    {
                        conn.Open();
                        comm.CommandType = CommandType.StoredProcedure;
                        notificationCount = Convert.ToInt32(comm.ExecuteScalar());
                    }
                }
            } catch (Exception ex)
            {
                Logger.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }

            Logger.WriteEntry("Notifications check completed. " + notificationCount + " new notifications generated.", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            base.OnStop();

            Logger.WriteEntry("Service stopped", EventLogEntryType.Information);
        }

        protected override void OnContinue()
        {
            base.OnContinue();

            Logger.WriteEntry("Service continuing", EventLogEntryType.Information);
        }
    }
}
