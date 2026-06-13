using eMARUpdater;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmarUpdater
{
    public partial class Form1 : Form
    {
        Utilities util;

        public Form1()
        {
            InitializeComponent();
            
            //Get a reference to the utilties class.
            //The actual work is done by methods inside it.
            util = new Utilities();
            
            //Load the notes as to what we'll do in the left textbox.
            txtLog.Text = util.PopulateNotes();

            //Load the value of the settings in the right text box.
            txtSettings.Text = util.PopulateSettings();

            //Allow the user to interact with this form.
            this.Enabled = true;

            //Change the title of this form.
            this.Text = "eMAR Updater";
        }


        private void cmdGo_Click(object sender, EventArgs e)
        {
            //This actually does the update steps.
            //I have listed a comment explaining what each one is.
            //It makes a call out to the utilties class to do the actual work.
            //Then it logs completion of that step in the left text box when finished.
            //Also, I disable the "Go" button so that this cannot be fired more than once.

            //Changed this so each method in the utilities class returns its status as a string.
            //Then we add that status to the log text box.

            try
            {
                //Before anything else, clear out the log textbox.
                txtLog.Text = "";

                //Also disable the Go button.
                cmdGo.Enabled = false;

                //Store the status of each operation.
                string sTemp;

                //Whether or not we're updating the API and UI.
                bool bUpdateApi = util.IsApiUpdate();
                bool bUpdateUi = util.IsUiUpdate();

                //If therer's no update to either the UI or API, then don't do anything.
                if (bUpdateApi || bUpdateUi)
                {

                    //1) Disable the scheduled task that checks the IDS.
                    sTemp = util.SetTaskState(false);
                    txtLog.Text += sTemp;

                    //2) Disable the scheduled task that checks IIS.
                    sTemp = util.SetIisTaskState(false);
                    txtLog.Text += sTemp;

                    //If we're doing a UI update...
                    if (bUpdateUi)
                    {
                        //3) Stop the UI website.
                        sTemp = util.StopUiWebsite();
                        txtLog.Text += sTemp;

                        //4) Stop the UI application pool.
                        sTemp = util.StopUiApplicationPool();
                        txtLog.Text += sTemp;

                        //5) Delete the UI Old folder if it exists.
                        //6) Rename the UI folder to UI Old.
                        //7) Rename the UI New folder to UI.
                        sTemp = util.RenameUiFolder();
                        txtLog.Text += sTemp;

                        //8) Start the UI application pool.
                        sTemp = util.StartUiApplicationPool();
                        txtLog.Text += sTemp;

                        //9) Start the UI website.
                        sTemp = util.StartUiWebsite();
                        txtLog.Text += sTemp;
                    }
                    else
                    {
                        txtLog.Text += "There is no UI update." + Environment.NewLine;
                    } //end if

                    //If we're doing an API update.
                    if (bUpdateApi)
                    {
                        //10) Stop the API website.
                        sTemp = util.StopApiWebsite();
                        txtLog.Text += sTemp;

                        //11) Stop the API application pool.
                        sTemp = util.StopApiApplicationPool();
                        txtLog.Text += sTemp;

                        //12) Delete the API Old folder if it exists.
                        //13) Rename the API folder to API Old.
                        //14) Rename the API New folder to API.
                        sTemp = util.RenameApiFolder();
                        txtLog.Text += sTemp;

                        //15) Start the API application pool.
                        sTemp = util.StartApiApplicationPool();
                        txtLog.Text += sTemp;

                        //16) Start the API website.
                        sTemp = util.StartApiWebsite();
                        txtLog.Text += sTemp;
                    }
                    else
                    {
                        txtLog.Text += "There is no API update." + Environment.NewLine;
                    } //end if

                    //17) Enable the scheduled task that checks IIS.
                    sTemp = util.SetIisTaskState(true);
                    txtLog.Text += sTemp;

                    //18) Enable the scheduled task that checks the IDS.
                    sTemp = util.SetTaskState(true);
                    txtLog.Text += sTemp;

                    //19) Log that we are finished.
                    txtLog.Text += Environment.NewLine + "Upgrade is completed." + Environment.NewLine;
                }
                else
                {
                    txtLog.Text = "There is no UI nor API update to be applied.";
                } //end if
            }
            catch (Exception ex)
            {
                //Show a messagebox with whatever message we have.
                //Could be one I wrote for specific situations or
                //an actual exception's message.
                //If we trip an exception in one step, then we won't continue on to the other steps.
                MessageBox.Show(ex.Message);

                //Also put the text of the exception in the left/notes textbox.
                //Add a couple of line breaks (so that we can easily delineate
                //the log above from the exception below).
                //Then put the exception below.
                txtLog.Text += Environment.NewLine + Environment.NewLine;
                txtLog.Text += ex.Message + Environment.NewLine;
                txtLog.Text += ex.Source + Environment.NewLine;
                txtLog.Text += ex.StackTrace + Environment.NewLine + Environment.NewLine;

                txtLog.Text += "Please manually finish the update and ensure that eMAR is running properly." + Environment.NewLine;
                txtLog.Text += "Instructions for doing this are in Confluence." + Environment.NewLine;

                //Because I'm putting the exception in the textbox of the updater,
                //I don't see any beenfit to writing it out to the Event Viewer.
                //The message can be copied from the textbox on the form and saved off.
            } //end try/catch.
        }
    }
}
