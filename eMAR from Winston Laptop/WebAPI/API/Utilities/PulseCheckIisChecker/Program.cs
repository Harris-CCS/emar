using Microsoft.Web.Administration;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PulseCheckIisChecker
{
    static class Program
    {
        /// <summary>
        /// Program:        PulseCheckIisChecker
        /// Author:         Winston Murdock
        /// Date:           06/28/2022
        /// Purpose:        To check the specified application pools and websites and restart them if they are stopped.
        /// Note:           This must be run as an administrator.
        /// Note 2:         This will likely be run as a scheduled task in Windows.
        /// 
        /// Params:         applicationPools = comma-delimited list of application pools that we want to check.
        ///                 websites = comma-delimited list of websites that we want to check.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //Get the lists of websites and application pools that we want to check from the settings file.
            string[] applicationPoolsNames = ConfigurationManager.AppSettings["applicationPools"].Split(',');
            string[] websiteNames = ConfigurationManager.AppSettings["websites"].Split(',');

            foreach (string applicationPoolName in applicationPoolsNames)
            {
                CheckApplicationPool(applicationPoolName);
            } //end foreach

            foreach (string websiteName in websiteNames)
            {
                CheckWebSite(websiteName);
            } //end foreach

            //Don't attempt to load the GUI/windows form.
            //I couldn't get the configuration stuff working in a console app, so a Windows app it is.
            //With the below commented out, this application closes as soon as its finished with no trace left on the desktop.
            //We don't have a configuration file for this project, yet.  But we might neeed one, some day.
            //So I've used a Windows application as I did for PulseCheckEmarIdsQueueChecker.

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        } //end method Main

        public static void CheckWebSite(string websiteName)
        {
            try
            {
                //Get a reference to IIS.
                ServerManager iisManager = new ServerManager();

                //Get the specified website.
                Site webSite = iisManager.Sites[websiteName];

                //If the website is not currently started, then start it.
                if (webSite.State != ObjectState.Started)
                {
                    webSite.Start();

                    //Log to the event viewer that we had to restart the UI website.
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        string sMessage = "PulseCheckEmarIisChecker started the " + websiteName + " website at " + DateTime.Now + ".";

                        eventLog.Source = "PulseCheck EMAR API";
                        eventLog.WriteEntry(sMessage, EventLogEntryType.Warning, 101, 1);
                    } //end using.
                } //end if
            }
            catch (Exception ex)
            {
                //Log the issue we got.
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = ex.Message + "\n";
                    sException += "inner exception = " + ex.InnerException + "\n";
                    sException += "source = " + ex.Source + "\n";
                    sException += ex.StackTrace + "\n";

                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Warning, 101, 1);
                } //end using.
            } //end try/catch
        } //end CheckWebsite

        public static void CheckApplicationPool(string applicationPoolName)
        {
            try
            {
                //Get a reference to IIS.
                ServerManager iisManager = new ServerManager();

                //Get the specified application pool.
                ApplicationPool applicationPool = iisManager.ApplicationPools[applicationPoolName];

                //If the application pool is not currently started, then start it.
                if (applicationPool.State != ObjectState.Started)
                {
                    //Start it.
                    applicationPool.Start();

                    //Log to the event viewer that we restarted the application pool.
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        string sMessage = "PulseCheckEmarIisChecker started the " + applicationPoolName + " application pool at " + DateTime.Now + ".";

                        eventLog.Source = "PulseCheck EMAR API";
                        eventLog.WriteEntry(sMessage, EventLogEntryType.Warning, 101, 1);
                    } //end using.
                } //end if
            }
            catch (Exception ex)
            {
                //Log the issue we got.
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = ex.Message + "\n";
                    sException += "inner exception = " + ex.InnerException + "\n";
                    sException += "source = " + ex.Source + "\n";
                    sException += ex.StackTrace + "\n";

                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Warning, 101, 1);
                } //end using.
            } //end try/catch

        } //end CheckApplicationPool
    }
}
