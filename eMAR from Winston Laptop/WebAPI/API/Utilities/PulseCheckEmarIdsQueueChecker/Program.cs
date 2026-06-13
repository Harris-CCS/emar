using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PulseCheckEmarIdsQueueChecker
{
    static class Program
    {
        /// <summary>
        /// Check on the status of the IDS.
        /// If we have unpicked up rows and are not processing
        /// a row, then restart the app pool and website.
        /// If all rows or processed or we are currently
        /// processing a row, then let it keep running.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //*********************************************************************************
            //Program:  PulseCheckEmarIdsQueueChecker
            //Author:   Winston Murdock
            //Date:     06/25/2021
            //Purpose:  Restart the API website and application pool when there are entries
            //              in the IDS queue table that have not been picked up.
            //
            //Updated:  Winston Murdock
            //Date:     02/03/2020
            //          Added a check for in-process rows that have not been completed yet.
            //              When we encounter one of those, we set the inprocess_datetime
            //              to null.  This triggers the IDS to pick it up and process it
            //              as if it were a new entry.
            //
            //Updated:  Winston Murdock
            //Date      08/22/2022.
            //          Only restart the website and application pool if we have unpicked up
            //              entries and there are no inprocess rows.  This no longer resets
            //              long-running entries.  A SQL job handles stamping long-running
            //              entries as "complete" and logging that we had to end that one
            //              prematurely.  If we have unpicked up entries and an inprocess
            //              entry, then we know that the IDS is proessing that entry, and we
            //              should let the IDS continue to process it.  We do not do
            //              anything in that situation.
            //*********************************************************************************


            //Wrap the entire thing in a try/catch.
            //if we hit any exceptions, log them to the event viewer.
            try
            {
                //Local variables to hold the results of the two SQL queries.
                int numRowsNotPickedUp = 0;
                int numRowsInProcessButNotCompleted = 0;

                //Get the connection string.
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["IbexSQlConnection"].ConnectionString;

                ////Get the wait times.
                ////We aren't using the settings values right now.
                ////But it doesn't hurt anything to leave them in the settings file and just comment out this code block.
                //var waitTime = System.Configuration.ConfigurationManager.AppSettings["WaitTimeInSeconds"];
                //var waitTimeInProcess = System.Configuration.ConfigurationManager.AppSettings["WaitTimeInProcessNotCompleteInSeconds"];

                ////Attempt to cast the wait time values to ints.
                ////If they are not valid numbers, then use logical defaults.
                ////This prevents issues from bad entries in the configuration file.
                ////These variables are only used for the TryParse call and not used anywhere else.
                //int waitTimeInt;
                //int waitTimeInProcessInt;

                //if (!int.TryParse(waitTime, out waitTimeInt))
                //{
                //    waitTime = "30";
                //} //end if

                //if (!int.TryParse(waitTimeInProcess, out waitTimeInProcessInt))
                //{
                //    waitTimeInProcess = "30";
                //} //end if

                //This query gets the number of rows in the queue tabel that have not been picked up yet.
                string sSQL = "SELECT COUNT(*) as 'count' ";
                sSQL += "FROM emar_update_queue (NOLOCK) ";
                sSQL += "WHERE complete_datetime IS NULL ";
                sSQL += "AND inprocess_datetime IS NULL ";

                //This query gets the number of rows that are in process and not completed.
                string sSQL2 = "SELECT COUNT(*) as 'count' ";
                sSQL2 += "FROM emar_update_queue (NOLOCK) ";
                sSQL2 += "WHERE complete_datetime IS NULL ";
                sSQL2 += "AND inprocess_datetime IS NOT NULL ";

                //Get the count of rows that have not been picked up yet.
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(sSQL, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    try
                    {
                        //If we have anything in the reader...
                        if (reader.HasRows)
                        {
                            //Actually read from the reader.
                            if (reader.Read())
                            {
                                //Get the first column (which will be the count since
                                //we only return one column in our select list).
                                numRowsNotPickedUp = reader.GetInt32(0);
                            } //end if
                        } //end if
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();

                        connection.Close();
                    } //end try/finally
                } //end using

                //Get the count of in process and not completed rows.
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(sSQL2, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    try
                    {
                        //If we have anything in the reader...
                        if (reader.HasRows)
                        {
                            //Actually read from the reader.
                            if (reader.Read())
                            {
                                //Get the first column (which will be the count since
                                //we only return one column in our select list).
                                numRowsInProcessButNotCompleted = reader.GetInt32(0);
                            } //end if
                        } //end if
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();

                        connection.Close();
                    } //end try/finally
                } //end using

                //If we have any rows that have not been picked up yet...
                //Then if we have no "in process" rows...
                //Then restart the API application pool/website.
                //If all rows are processed or if we have some rows that have not been
                //picked up but are currently processing a row, then we're processing
                //the "in process" row and don't need to restart anything.
                //If that "in process" row takes too long to process, my SQL job will
                //mark it as complete (and log that we did so) after it has been
                //running for five minutes.
                if (numRowsNotPickedUp > 0 && numRowsInProcessButNotCompleted < 1)
                {
                    //Get a reference to IIS.
                    ServerManager iisManager = new ServerManager();

                    //Get a reference to the eMARAPI application pool.
                    //This is not case sensitive.
                    ApplicationPool emarApiApplicationPool = iisManager.ApplicationPools["emarapi"];

                    //Get a reference to the eMARAPI website.
                    //This is not case sensitive.
                    Site emarWebSite = iisManager.Sites["emarapi"];

                    //Stop the website.
                    //We will restart it after the application pool is restarted.
                    if (emarWebSite.State == ObjectState.Started)
                    {
                        emarWebSite.Stop();
                    } //end if

                    //If the application pool is currently started, then recycle it.
                    //Else, it's not currently started and we will start it.
                    if (emarApiApplicationPool.State == ObjectState.Started)
                    {
                        //The API application pool is currently started.
                        //Recycle it.
                        emarApiApplicationPool.Recycle();
                    }
                    else
                    {
                        //The API application pool is not started.
                        //Start it.
                        emarApiApplicationPool.Start();
                    } //end if

                    //Restart the website.
                    emarWebSite.Start();

                    //Insert the "heartbeat" row into the queue table so that it starts processing
                    //rows in the queue table immediately rather than waiting for a user to do
                    //something in PulseCheck that inserts a row into the queue table.
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string sHeartBeat = "INSERT INTO emar_update_queue (entity, external_id) VALUES ('heartbeat', -1)";
                        SqlCommand command = new SqlCommand(sHeartBeat, connection);
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    } //end using

                    //Log to the event viewer that we recycled the application pool.
                    //Since "PulseCheck EMAR API" is already setup as a source in the
                    //Event Viewer, we'll use that as our source.
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        string sMessage = "PulseCheckEmarIdsQueueChecker recycled the emarapi application pool at " + DateTime.Now + " \n";
                        sMessage += "due to records not being picked up from the IDS queue table.";

                        eventLog.Source = "PulseCheck EMAR API";
                        eventLog.WriteEntry(sMessage, EventLogEntryType.Warning, 101, 1);
                    } //end using.
                }
                else
                {
                    //Either there are no rows in the queue table that have not been processed,
                    //or there is one row currently being processed and some rows waiting behind it.
                    //Either way, we didn't need to restart anything.
                    //Log that we did not have to recycle the application pool.
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        string sMessage = "The PulseCheck eMAR's IDS is running fine.\n";
                        sMessage += "There was no need to recycle the emar api application pool.";

                        eventLog.Source = "PulseCheck EMAR API";
                        eventLog.WriteEntry(sMessage, EventLogEntryType.Information, 101, 1);
                    } //end using.
                } //end if (do we have any rows that haven't been picked up yet?)
            }
            catch (Exception e)
            {
                //Log whatever happened to the event viewer as an "error" entry.
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = e.Message + "\n";
                    sException += "inner exception = " + e.InnerException + "\n";
                    sException += "source = " + e.Source + "\n";
                    sException += e.StackTrace + "\n";

                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                } //end using.
            } // end try/catch

            //Don't attempt to load the GUI/windows form.
            //I couldn't get the configuration stuff working in a console app, so a Windows app it is.
            //With the below commented out, this application closes as soon as its finished with no trace left on the desktop.
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
