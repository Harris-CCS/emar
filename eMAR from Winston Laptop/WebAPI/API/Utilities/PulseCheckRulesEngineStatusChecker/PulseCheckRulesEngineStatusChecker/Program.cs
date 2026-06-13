using Microsoft.VisualBasic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace PulseCheckRulesEngineStatusChecker
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            //****************************************
            //Authors: Thomas Antolick and Winston Murdock
            //Dates: 12/02/2025 - 12/15/2025
            //1) Call out to the rules client ibx file to see if the Rules Engine is hung or not.
            //2) If it's hung...
            //  A) Send an email to relevant parties (pulling from a configuration file).
            //  B) Log to the Windows Event Viewer.
            //3) Stop/restart Rules Engine.
            //****************************************

            //In other instances where I needed to get to a configuration file,
            //I had to use a Windows forms app rather than a console app.
            //Do the same here.
            //Do not load the form.
            //Application.Run(new Form1());

            //1) Make a call to the rules client to get the current status of the Rules Engine on this server.
            RulesEngineConfigResults oRulesEngineConfigResults = Utilities.CheckPulseCheckRulesEngineStatus();

            //2) If Rules Engine is hung
            //  A) Send an email.
            //  B) Log to the Windows Event Viewer.
            if (oRulesEngineConfigResults.RestartNotNeeded)
            {
                //Rules Engine is up.
                Utilities.LogMessageToEventViewer("Rules Engine is running.", EventLogEntryType.Information);
            }
            else
            {
                //Rules Engine is down.

                //Send email.
                Utilities.SendEmail();

                //Log to the event viewer that Rules Engine is down.
                Utilities.LogMessageToEventViewer("Rules Engine needs to be restarted.", EventLogEntryType.Error);

                //Also log to a .txt file.
                Utilities.LogMessageToFile("Rules Engine needs to be restarted." + oRulesEngineConfigResults.ConfigResults);

                //3) Stop/Restart the Rules Engine.
                Utilities.RestartService();
            }//end if
        } //end Main
    } //end class Program
} //end Namespace PulseCheckRulesEngineStatusChecker