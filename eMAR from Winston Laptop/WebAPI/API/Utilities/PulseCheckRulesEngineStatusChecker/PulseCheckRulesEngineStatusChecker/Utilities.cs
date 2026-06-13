using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace PulseCheckRulesEngineStatusChecker
{
    public class Utilities
    {
        //Get the currernt date so that we can use that in any logging statements.
        static string sCurrentDate = DateTime.Now.ToString("yyyy-MM-dd");

        static string sLogFilePath = GetConfigurationKeyValue("LogFilePath");

        public static void LogMessageToFile(string message)
        {
            //Log to a .txt file, named with today's date,
            //that is in the path specified by the LogFilePath configuration key.
            try
            {
                //Name the file yyyy-MM-dd.txt
                string sFilePath = sLogFilePath + sCurrentDate + ".txt";

                string logEntry = Environment.NewLine;
                logEntry += $"{DateTime.Now}: {message}{Environment.NewLine}";
                File.AppendAllText(sFilePath, logEntry);
            }
            catch (Exception ex)
            {
                // Handle potential errors during file writing (e.g., file in use, permissions)
                //Don't do anything here.  But also don't hose the application if we couldn't write to the .txt file.
            }
        } //end LogMessageToFile

        public static void LogMessageToEventViewer(string sMessage, EventLogEntryType oType)
        {
            //Before logging to the Event Viewer for the first time on an environment,
            //the following command must be run from a command prompt.
            //It sets up PulseCheckRulesEngineStatusChecker as a source in the Event Viewer.
            //eventcreate / ID 1 / L APPLICATION / T INFORMATION / SO "PulseCheckRulesEngineStatusChecker" / D "Test Message“
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "PulseCheckRulesEngineStatusChecker";
                eventLog.WriteEntry(sMessage, oType, 101, 1);
            } //end using.
        } //end LogMessageToEventViewer

        public static string GetConfigurationKeyValue(string sKey)
        {
            //Use a helper function to get the value of the configuration key
            //They are returned as nullable strings from the configuration file,
            //but we want to have them as normal strings throughout this application.
            //If a value is null in the configuration file, this will return empty string.
            //Else, it will return the actual value from the configuration file.
            string? sNullable = ConfigurationManager.AppSettings[sKey];
            string sReturn = String.Empty;

            if (!string.IsNullOrEmpty(sNullable))
            {
                sReturn = sNullable;
            } //end if

            return sReturn;
        } //end GetConfigurationKeyValue

        public static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
        } //end IsBase64String

        public static string ExecuteCommand(string sCommand, string sArguments = "")
        {
            //Execute the command that was passed in.
            //If any arguments were passed in, pass those along.
            //This is used for the call out to check the status of Rules Engine
            //and also to call sc.exe to stop/start the service.

            //I should have used the following but ended up using a batch file (which requires zero parameters from the command line).
            //perl E:\ibex\link\RulesEngine\rulesclient.ibx "QUERY CONFIG" 20100 ros-57c-dx01.picis.com
            //Hindsight is 20/20.  I could use perl.exe as the filename and then
            //pass everything else as arguments.
            //Since the batch file works, we'll go with that.
            //But were I starting over from scratch, I would attempt to use the arguments.
            try
            {
                //Create process
                System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

                //strCommand is path and file name of command to run
                pProcess.StartInfo.FileName = sCommand;

                //If we have arguments to pass into the command, do so now.
                if (sArguments.Length > 0)
                {
                    pProcess.StartInfo.Arguments = sArguments;
                } //end if
                

                pProcess.StartInfo.UseShellExecute = false;

                //Set output of program to be written to process output stream
                pProcess.StartInfo.RedirectStandardOutput = true;

                //Optional
                string sApplicationPath = GetConfigurationKeyValue("WorkingDirectory");
                pProcess.StartInfo.WorkingDirectory = sApplicationPath;

                //Start the process
                pProcess.Start();

                //Get program output
                string sOutput = pProcess.StandardOutput.ReadToEnd();

                //Wait for process to finish
                pProcess.WaitForExit();

                //Return the output.
                return sOutput;
            }
            catch (Exception ex)
            {
                string sException = ex.Message + Environment.NewLine;
                sException += "source = " + ex.Source + Environment.NewLine;
                sException += "inner exception = " + ex.InnerException + Environment.NewLine;
                sException += ex.StackTrace + Environment.NewLine;

                //Log any errors to the Event Viewer.
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "PulseCheckRulesEngineStatusChecker";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                } //end using.

                //Also log to text file on disk.
                LogMessageToFile(sException);

                return "An error happened.  See the Event Viewer and/or text file logs for information.";
            } //end try/catch
        } //end ExecuteCommand

        public static RulesEngineConfigResults CheckPulseCheckRulesEngineStatus()
        {
            //Check the status of the Rules Engine.
            //Return true if it's up.
            //Return false if it's not up.
            RulesEngineConfigResults oResults = new RulesEngineConfigResults();
            bool bReturn = true;

            //We have a batch file setup to run the perl command.
            //Run that batch file (which will run the perl command).
            //perl E:\ibex\link\RulesEngine\rulesclient.ibx "QUERY CONFIG" 20100 ros-57c-dx01.picis.com;
            string sCommandToRun = GetConfigurationKeyValue("BatchPath");
            string sResults = ExecuteCommand(sCommandToRun);

        //Now investigate the results to see if it's running or not.
        //On 57c when it's not running, I get this error.
        //socket_create: IO::Socket::INET: connect: No connection could be made because the target machine actively refused it.

        //if (sResults.EndsWith("OK"))
        //Here is the output from the call to QUERY CONFIG on 57c.
        //Perhaps we could look for something other than having "OK" in it.
        //runtime: CPU usage          : 2.20 %
        //logfile:E:/ ibex / link / 20251209rul.txt
        //current cfg: none
        //current ruleset:
        //none
        //current rule: none
        //name: 
        //load:
        //OK


            //When it's up, I get an "OK" at the end of the call to config.
            //Check for the presence of OK in the response.
            //If we have it, then Rules Engine is running fine.
            //If we don't have it, then Rules Engine is down.
            if (sResults.IndexOf("OK") > -1)
            {
                bReturn = true;
            }
            else
            {
                bReturn = false;
            } //end if

            //Return.
            oResults.RestartNotNeeded = bReturn;
            oResults.ConfigResults = sResults;
            return oResults;

        } //end CheckPulseCheckRulesEngineStatus

        public static void SendEmail()
        {
            string sEmailSubject = GetConfigurationKeyValue("EmailSubject");
            string sEmailSender = GetConfigurationKeyValue("EmailSender");
            string sEmailReceipients = GetConfigurationKeyValue("EmailReceipients");
            var emailReceipientsList = GetConfigurationKeyValue("EmailReceipients").Split(";").ToList();
            string sEmailServerAddress = GetConfigurationKeyValue("EmailSMTPServerAddress");
            string sEmailSMTPServerPort = GetConfigurationKeyValue("EmailSMTPServerPort");
            int? port = int.TryParse(sEmailSMTPServerPort, out int number) ? number : (int?)null;
            string sEmailSMTPServerUsername = GetConfigurationKeyValue("EmailSMTPServerUsername");
            string sEmailSMTPServerPassword = GetConfigurationKeyValue("EmailSMTPServerPassword");
            bool bEmailSMTPServerUseSsl = GetConfigurationKeyValue("EmailSMTPServerUseSsl") == "Y" ? true : false;
            string sEnvironmentName = GetConfigurationKeyValue("EnvironmentName");
            string sServiceName = GetConfigurationKeyValue("ServiceName");
            string sEmailBody = "";
            MailMessage message = new MailMessage();

            SmtpClient smtpClient = null;
            try
            {
                //We should store the base64 encoded version of the password rather than the plaintext password.
                if (IsBase64String(sEmailSMTPServerPassword))
                {
                    sEmailSMTPServerPassword = Encoding.UTF8.GetString(Convert.FromBase64String(sEmailSMTPServerPassword));
                }

                smtpClient = new SmtpClient(sEmailServerAddress)
                {
                    Port = (int)port,
                    EnableSsl = bEmailSMTPServerUseSsl,
                };

                if (!string.IsNullOrWhiteSpace(sEmailSMTPServerUsername) && !string.IsNullOrWhiteSpace(sEmailSMTPServerPassword))
                {
                    smtpClient.Credentials = new NetworkCredential(sEmailSMTPServerUsername, sEmailSMTPServerPassword);
                }
                else
                {
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                }

                //Construct the body of the email message.
                sEmailBody = "The PulseCheck Rules Engine service was stuck in " + sEnvironmentName + ".  ";
                sEmailBody += "If the port remains blocked after stopping the " + sServiceName + ", please contact PulseCheck support.";

                message = new MailMessage
                {
                    From = new MailAddress(sEmailSender),
                    Sender = new MailAddress(sEmailSender),
                    Subject = sEmailSubject,
                    Body = sEmailBody,
                    IsBodyHtml = false
                };

                //Add the recipients.
                foreach (string sRecipient in emailReceipientsList)
                {
                    message.To.Add(sRecipient);
                } //end foreach.

                //Send the email
                try
                {
                    //smtpClient.SendMailAsync(message);
                    smtpClient.Send(message);
                }
                catch (Exception ex)
                {
                    //Some issue sending the email.
                    string sException = ex.Message + Environment.NewLine;
                    sException += "source = " + ex.Source + Environment.NewLine;
                    sException += "inner exception = " + ex.InnerException + Environment.NewLine;
                    sException += ex.StackTrace + Environment.NewLine;

                    //Log any errors to the Event Viewer.
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "PulseCheckRulesEngineStatusChecker";
                        eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                    } //end using.

                    //Also log to text file on disk.
                    LogMessageToFile(sException);

                    return;
                } //end try/catch
            }
            catch (Exception e)
            {
                //Configuration file was missing something we needed.
                return;
            } //end try/catch
        } //end SendEmail

        public static void RestartService()
        {
            //Get the name of the service from the configuration file.
            string sServiceName = GetConfigurationKeyValue("ServiceName");

            //sc.exe works with services.
            string sCommand = "sc.exe";

            //Setup the stop and start arguments
            //(including double quotes around the service name).
            string sArgumentsStop = "stop \"" + sServiceName + "\"";
            string sArgumentStart = "start \"" + sServiceName + "\"";

            //Stop the service.
            ExecuteCommand(sCommand, sArgumentsStop);

            //Wait for five seconds before attempting to start the service.
            //Want to give the service time to stop before attempting to restart it.
            Thread.Sleep(5000);

            //Start the service.
            ExecuteCommand(sCommand, sArgumentStart);
        } //end function RestartService
    }
}
