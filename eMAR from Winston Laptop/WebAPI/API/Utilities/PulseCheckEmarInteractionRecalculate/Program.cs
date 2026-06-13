using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PulseCheckEmarInteractionRecalculate
{
    static class Program
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //*********************************************************************************
            //Program:  PulseCheckEmarInteractionRecalculate
            //Author:   Winston Murdock
            //Date:     06/13/2021
            //
            //1) Call get_list_of_patients_to_clean_up_interactions_for to get the list of
            //      patients that we need to recalculate the interactions for.
            //
            //2) Loop through the list of patients.
            //
            //3) For each one, Make an HTTP request to recalculate the interactions and reactions
            //      {{URL}}/api/medications/RecalculateAllInteractionsReactions/Patient/{patientId}
            //*********************************************************************************


            //Wrap the entire thing in a try/catch.
            //if we hit any exceptions, log them to the event viewer.
            try
            {

                //Get the connection string and wait times.
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["emarSQlConnection"].ConnectionString;

                //Grab the URL to the recalculate endpoint from the settings file.
                string url = System.Configuration.ConfigurationManager.AppSettings["RecalculateURL"];

                //1) Call get_list_of_patients_to_clean_up_interactions_for to get the list of
                //      patients that we need to recalculate the interactions for.
                var conn = new SqlConnection(connectionString);
                using (conn)
                {
                    conn.Open();

                    try
                    {
                        //Testing for one patient.
                        string thisURL = url.Replace("{patientId}", Convert.ToString(699));

                        MakeApiCall(thisURL);

                        //SqlCommand cmd = new SqlCommand("get_list_of_patients_to_clean_up_interactions_for", conn);
                        //cmd.CommandType = CommandType.StoredProcedure;

                        //SqlDataReader reader = cmd.ExecuteReader();
                        //long patientId;

                        ////2) Loop through the list of patients.
                        //while (reader.Read())
                        //{
                        //    patientId = (long)reader["patient_id"];

                        //    //3) For each one, Make an HTTP request to recalculate the interactions and reactions
                        //    //      {{URL}}/api/medications/RecalculateAllInteractionsReactions/Patient/{patientId}
                        //    //Get the exact URL for this patient.
                        //    string thisURL = url.Replace("{patientId}", Convert.ToString(patientId));

                        //    MakeApiCall(thisURL);

                        //} //end while

                        conn.Close();
                    }
                    catch (Exception e)
                    {
                        using (EventLog eventLog = new EventLog("Application"))
                        {
                            string sException = e.Message + "\n";
                            sException += "inner exception = " + e.InnerException + "\n";
                            sException += "source = " + e.Source + "\n";
                            sException += e.StackTrace + "\n";

                            eventLog.Source = "PulseCheck EMAR API";
                            eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                        } //end using.

                        conn.Close();
                    } // end try/catch

                } //end using.
            }
            catch (Exception e)
            {
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
        } //end Main

        static public async void MakeApiCall(string url)
        {
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(url);
                request.Method = "GET";

                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Timeout = 10000;
                var response = request.GetResponse();

                var xyz = "temp";
            }
            catch (Exception ex)
            {
                var abc = 1;
            }

            //throw new NotImplementedException();
        } //end MakeApiCall
        }
    }
