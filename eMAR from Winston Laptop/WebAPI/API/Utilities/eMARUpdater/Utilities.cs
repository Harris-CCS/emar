using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using TaskScheduler;

namespace eMARUpdater
{
    public class Utilities
    {
        //varaibles to hold the values from applconfig.
        string _webFolder;
        string _scheduledTaskName;
        bool _scheduledTaskExists;
        string _iisScheduledTaskName;
        bool _iisScheduledTaskExists;
        string _uiFolderName;
        string _apiFolderName;
        string _uiApplicationPoolName;
        string _apiApplicationPoolName;
        string _uiWebsiteName;
        string _apiWebsiteName;

        //Get a reference to IIS.
        ServerManager _iisManager = new ServerManager();

        public string PopulateNotes()
        {
            //Build up a string describing what we're giong to do.
            //then return that string.
            //It will be set as the text of the left text box on the form.
            string sNotes = "This tool will..." + Environment.NewLine + Environment.NewLine;
            sNotes += "1) Disable the scheduled task that checks the IDS." + Environment.NewLine;
            sNotes += "2) Disable the scheduled task that checks IIS." + Environment.NewLine;
            sNotes += "3) If we have a UI update..." + Environment.NewLine;
            sNotes += "  A) Stop the UI website." + Environment.NewLine;
            sNotes += "  B) Stop the UI application pool." + Environment.NewLine;
            sNotes += "  C) Delete the UI Old folder if it exists." + Environment.NewLine;
            sNotes += "  D) Rename the UI folder to UI Old." + Environment.NewLine;
            sNotes += "  E) Rename the UI New folder to UI." + Environment.NewLine;
            sNotes += "  F) Start the UI application pool." + Environment.NewLine;
            sNotes += "  G) Start the UI website." + Environment.NewLine;
            sNotes += "4) If we have an API update..." + Environment.NewLine;
            sNotes += "   A) Stop the API website." + Environment.NewLine;
            sNotes += "   B) Stop the API application pool." + Environment.NewLine;
            sNotes += "   C) Delete the API Old folder if it exists." + Environment.NewLine;
            sNotes += "   D) Rename the API folder to API Old." + Environment.NewLine;
            sNotes += "   E) Rename the API New folder to API." + Environment.NewLine;
            sNotes += "   F) Start the API application pool." + Environment.NewLine;
            sNotes += "   G) Start the API website." + Environment.NewLine;
            sNotes += "5) Enable the scheduled task that checks IIS." + Environment.NewLine;
            sNotes += "6) Enable the scheduled task that checks the IDS." + Environment.NewLine;
            sNotes += "7) Log completion status in this textbox." + Environment.NewLine;

            //Return the built-up string.
            return sNotes;
        } // end PopulateNotes

        public string PopulateSettings()
        {
            //Build up a string with each of ther values from app.config
            //then return that string.
            //It will be set as the text of the right text box on the form.
            string sSettings = "Please confirm that these settings are correct before clicking the Go button.  If they are not correct, exit this application, change values as needed in " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".exe.config, save the config file, and run this again." + Environment.NewLine + Environment.NewLine;

            //root web folder (C:\inetpub\wwwroot\)
            _webFolder = ConfigurationManager.AppSettings.Get("WebFolder");

            //Append an ending slash to web folder if it doesn't end in one.
            //Farther down, we assume that it has an end slash when appending
            //a sub directory name to this directory path.
            if (!_webFolder.EndsWith("\\"))
            {
                //it doesn't end in a backslash.  Add one.
                _webFolder += "\\";
            } //end if

            sSettings += "Web Folder = " + _webFolder + Environment.NewLine + Environment.NewLine;

            //Scheduled task exists
            _scheduledTaskExists = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("IdsScheduledTaskExists"));
            sSettings += "IDS Scheduled Task Exists = " + _scheduledTaskExists + Environment.NewLine + Environment.NewLine;

            //Scheduled task name.
            _scheduledTaskName = ConfigurationManager.AppSettings.Get("IdsScheduledTaskName");
            sSettings += "IDS Scheduled Task Name = " + _scheduledTaskName + Environment.NewLine + Environment.NewLine;

            //IIS scheduled task exists
            _iisScheduledTaskExists = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("IisScheduledTaskExists"));
            sSettings += "IIS Scheduled Task Exists = " + _iisScheduledTaskExists + Environment.NewLine + Environment.NewLine;

            //IIS Scheduled task name.
            _iisScheduledTaskName = ConfigurationManager.AppSettings.Get("IISScheduledTaskName");
            sSettings += "IIS Scheduled Task Name = " + _iisScheduledTaskName + Environment.NewLine + Environment.NewLine;

            //UI folder name (eMARUI)
            _uiFolderName = ConfigurationManager.AppSettings.Get("UiFolderName");
            sSettings += "UI Folder Name = " + _uiFolderName + Environment.NewLine + Environment.NewLine;

            //API folder name. (eMARAPI)
            _apiFolderName = ConfigurationManager.AppSettings.Get("ApiFoldername");
            sSettings += "API Folder Name = " + _apiFolderName + Environment.NewLine + Environment.NewLine;

            //UI application pool name
            _uiApplicationPoolName = ConfigurationManager.AppSettings.Get("UiApplicationPoolName");
            sSettings += "UI App Pool Name = " + _uiApplicationPoolName + Environment.NewLine + Environment.NewLine;

            //API application pool name
            _apiApplicationPoolName = ConfigurationManager.AppSettings.Get("ApiApplicationPoolName");
            sSettings += "API App Pool Name = " + _apiApplicationPoolName + Environment.NewLine + Environment.NewLine;

            //UI website name
            _uiWebsiteName = ConfigurationManager.AppSettings.Get("UiWebsiteName");
            sSettings += "UI Website Name = " + _uiWebsiteName + Environment.NewLine + Environment.NewLine;

            //API website name
            _apiWebsiteName = ConfigurationManager.AppSettings.Get("ApiWebsiteName");
            sSettings += "API Website Name = " + _apiWebsiteName;

            //Return the settings values.
            return sSettings;
        } // end PopulateSettings

        public string SetTaskState(bool bEnabled)
        {
            //Set the enabled property of the specified Scheduled Tasks to true or false based on bEnabled.
            string sRet = "";

            //Add a try/catch just in case we hit an issue accessing the task scheduler
            //(or if there isn't a scheduled task with the specified name).
            try
            {
                //Only try to disable/enable the scheduled task if this server has the scheduled task installed.
                //Rather than checking to see if the task exists, we're just using a setting from app.config for this value.
                if (_scheduledTaskExists)
                {
                    //Get a reference to the Task Scheduler service.
                    //This is an Interop.TaskScheduler COM object.
                    ITaskService taskService = new TaskScheduler.TaskScheduler();

                    //Connect to the Task Scheduler service.
                    taskService.Connect();

                    //Get the scheduled task for the IDS checker.
                    IRegisteredTask task = taskService.GetFolder("\\").GetTask(_scheduledTaskName);

                    //If bEnabled is true, enable the task.
                    //If bEnabled is false, disable the task.
                    task.Enabled = bEnabled;

                    //Tell the user that we either stopped or started the scheduled task.
                    if (bEnabled)
                    {
                        sRet += "Enabled the scheduled task that checks the IDS." + Environment.NewLine;
                    }
                    else
                    {
                        sRet += "Disabled the scheduled task that checks the IDS." + Environment.NewLine;
                    } //end if
                }
                else
                {
                    //Scheduled task doesn't exist.  No need to start/stop it.
                    sRet += "IDS scheduled task does not exist on this server." + Environment.NewLine;
                } //end if (scheduled task exists?)

                return sRet;
            }
            catch (Exception ex)
            {
                //Silently eat the error.
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch
        } //end SetTaskState

        public string SetIisTaskState(bool bEnabled)
        {
            //Set the enabled property of the specified Scheduled Tasks to true or false based on bEnabled.
            string sRet = "";

            //Add a try/catch just in case we hit an issue accessing the task scheduler
            //(or if there isn't a scheduled task with the specified name).
            try
            {
                //Only try to disable/enable the scheduled task if this server has the scheduled task installed.
                //Rather than checking to see if the task exists, we're just using a setting from app.config for this value.
                if (_iisScheduledTaskExists)
                {
                    //Get a reference to the Task Scheduler service.
                    //This is an Interop.TaskScheduler COM object.
                    ITaskService taskService = new TaskScheduler.TaskScheduler();

                    //Connect to the Task Scheduler service.
                    taskService.Connect();

                    //Get the scheduled task for the IDS checker.
                    IRegisteredTask task = taskService.GetFolder("\\").GetTask(_iisScheduledTaskName);

                    //If bEnabled is true, enable the task.
                    //If bEnabled is false, disable the task.
                    task.Enabled = bEnabled;

                    //Tell the user that we either stopped or started the scheduled task.
                    if (bEnabled)
                    {
                        sRet += "Enabled the scheduled task that checks IIS." + Environment.NewLine;
                    }
                    else
                    {
                        sRet += "Disabled the scheduled task that checks IIS." + Environment.NewLine;
                    } //end if
                }
                else
                {
                    //Scheduled task doesn't exist.  No need to start/stop it.
                    sRet += "IIS scheduled task does not exist on this server." + Environment.NewLine;
                } //end if (scheduled task exists?)

                return sRet;
            }
            catch (Exception ex)
            {
                //Silently eat the error.
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch

            //Add a try/catch just in case we hit an issue accessing the task scheduler
            //(or if there isn't a scheduled task with the specified name).
        } //end SetIisTaskState

        public string StopUiWebsite()
        {
            //Stop the Ui website.

            string sRet = "";

            try
            {
                //Get a reference to both the UI website.
                Site uiWebSite = _iisManager.Sites[_uiWebsiteName];

                //Try to start the website up to 20 times before we give up.
                bool tryAgain = true;
                int count = 0;
                while (tryAgain && count <= 20)
                {
                    try
                    {
                        //If the UI website is not stopped, then stop it.
                        if (uiWebSite.State != ObjectState.Stopped)
                        {
                            uiWebSite.Stop();
                        } //end if

                        tryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        //Increment.  If we get to 20 then we'll exit the loop.
                        count++;

                        //Log that we failed to stop the API website
                        sRet += "Attempt " + count + " to stop the API website failed." + Environment.NewLine;

                        //Delay three seconds before the next attempt.
                        Thread.Sleep(3000);
                    } //end try/catch.
                } //end while loop

                if (count >= 20)
                {
                    sRet += "Could not stop the UI website.  You might need to manually handle this update." + Environment.NewLine;
                }
                else
                {
                    sRet += "Stopped the UI website." + Environment.NewLine;
                } //end if

                return sRet;
            }
            catch (Exception ex)
            {
                //Could not stop the UI website.
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch
        } //end StopUiWebsite

        public string StopApiWebsite()
        {
            //Stop the APi website.

            string sRet = "";

            try
            {
                //Get a reference to both the UI website.
                Site apiWebSite = _iisManager.Sites[_apiWebsiteName];

                //Try to start the website up to 20 times before we give up.
                bool tryAgain = true;
                int count = 0;
                while (tryAgain && count <= 20)
                {
                    try
                    {
                        //If the API website is not stopped, then stop it.
                        if (apiWebSite.State != ObjectState.Stopped)
                        {
                            apiWebSite.Stop();
                        } //end if

                        tryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        //Increment.  If we get to 20 then we'll exit the loop.
                        count++;

                        //Log that we failed to stop the API website
                        sRet += "Attempt " + count + " to stop the API website failed." + Environment.NewLine;

                        //Delay three seconds before the next attempt.
                        Thread.Sleep(3000);
                    } //end try/catch.
                } //end while loop

                if (count >= 20)
                {
                    sRet += "Could not stop the API application pool.  You might need to manually handle this update." + Environment.NewLine;
                }
                else
                {
                    sRet += "Stopped the API website." + Environment.NewLine;
                } //end if

                return sRet;
            }
            catch (Exception ex)
            {
                //Could not stop the API website.
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch
        } //end StopUiWebsite

        public string StopUiApplicationPool()
        {
            //Stop the UI application pool.
            string sRet = "";

            try
            {
                //Get a reference to the UI application pool.
                ApplicationPool uiApplicationPool = _iisManager.ApplicationPools[_uiApplicationPoolName];

                //Try to start the website up to 20 times before we give up.
                bool tryAgain = true;
                int count = 0;
                while (tryAgain && count <= 20)
                {
                    try
                    {
                        //If the UI application pool is not stopped, then stop it.
                        if (uiApplicationPool.State != ObjectState.Stopped)
                        {
                            uiApplicationPool.Stop();
                        } //end if

                        tryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        //Increment.  If we get to 20 then we'll exit the loop.
                        count++;

                        //Log that we failed to stop the API website
                        sRet += "Attempt " + count + " to stop the API website failed." + Environment.NewLine;

                        //Delay three seconds before the next attempt.
                        Thread.Sleep(3000);
                    } //end try/catch.
                } //end while loop

                if (count >= 20)
                {
                    sRet += "Could not stop the UI application pool.  You might need to manually handle this update." + Environment.NewLine;
                }
                else
                {
                    sRet += "Stopped the UI application pool." + Environment.NewLine;
                } //end if

                return sRet;
            }
            catch (Exception ex)
            {
                //Could not stop the UI application pools.
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch
        } //end StopUiApplicationPool

        public string StopApiApplicationPool()
        {
            //Stop the API application pool.
            string sRet = "";

            try
            {
                //Get a reference to the API application pool.
                ApplicationPool apiApplicationPool = _iisManager.ApplicationPools[_apiApplicationPoolName];

                //Try to start the website up to 20 times before we give up.
                bool tryAgain = true;
                int count = 0;
                while (tryAgain && count <= 20)
                {
                    try
                    {
                        //If the API application pool is not stopped, then stop it.
                        if (apiApplicationPool.State != ObjectState.Stopped)
                        {
                            apiApplicationPool.Stop();
                        } //end if

                        tryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        //Increment.  If we get to 20 then we'll exit the loop.
                        count++;

                        //Log that we failed to stop the API website
                        sRet += "Attempt " + count + " to stop the API website failed." + Environment.NewLine;

                        //Delay three seconds before the next attempt.
                        Thread.Sleep(3000);
                    } //end try/catch.
                } //end while loop

                if (count >= 20)
                {
                    sRet += "Could not stop the API application pool.  You might need to manually handle this update." + Environment.NewLine;
                }
                else
                {
                    sRet += "Stopped the API application pool." + Environment.NewLine;
                } //end if

                return sRet;
            }
            catch (Exception ex)
            {
                //Could not stop the application pools.
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch
        } //end StopApiApplicationPool


        public string StartUiApplicationPool()
        {
            //Start both the API and UI application pools.

            string sRet = "";

            try
            {
                //Get a reference to the UI application pool.
                ApplicationPool uiApplicationPool = _iisManager.ApplicationPools[_uiApplicationPoolName];

                //Try to start the UI application pool up to 20 times before we give up.
                bool tryAgain = true;
                int count = 0;

                while (tryAgain && count <= 20)
                {
                    try
                    {
                        //If the UI application pool is not started, then start it.
                        if (uiApplicationPool.State != ObjectState.Started)
                        {
                            uiApplicationPool.Start();
                        } //end if

                        tryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        //Increment.  If we get to 20 then we'll exit the loop.
                        count++;

                        //Log that we failed to restart the application pool.
                        sRet += "Attempt " + count + " to start the UI app pool failed." + Environment.NewLine;

                        //Delay three seconds before the next attempt.
                        Thread.Sleep(3000);
                    } //end try/catch.
                } //end while loop

                if (count >= 20)
                {
                    sRet += "Could not start the UI application pool.  Please do it manually." + Environment.NewLine;
                }
                else
                {
                    sRet += "Started the UI application pool." + Environment.NewLine;
                }

                return sRet;
            }
            catch (Exception ex)
            {
                //Could not stop the UI website.
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch
        } //end StartUiApplicationPool

        public string StartApiApplicationPool()
        {
            //Start both the API application pool.

            string sRet = "";

            try
            {
                //Get a reference to both application pools.
                ApplicationPool apiApplicationPool = _iisManager.ApplicationPools[_apiApplicationPoolName];

                //Try to start the API application pool up to 20 times before we give up.
                bool tryAgain = true;
                int count = 0;

                while (tryAgain && count <= 20)
                {
                    try
                    {
                        //If the API application pool is not started, then start it.
                        if (apiApplicationPool.State != ObjectState.Started)
                        {
                            apiApplicationPool.Start();
                        } //end if

                        tryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        //Increment.  If we get to 20 then we'll exit the loop.
                        count++;

                        //Log that we failed to restart the application pool.
                        sRet += "Attempt " + count + " to start the API app pool failed." + Environment.NewLine;

                        //Delay three seconds before the next attempt.
                        Thread.Sleep(3000);
                    } //end try/catch.
                } //end while loop

                if (count >= 20)
                {
                    sRet += "Could not start the API application pool.  Please do it manually." + Environment.NewLine;
                }
                else
                {
                    sRet += "Started the API application pool." + Environment.NewLine;
                }

                return sRet;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch
        } //end StartApiApplicationPool

        public string StartUiWebsite()
        {
            //Start the UI website.

            string sRet = "";

            try
            {
                //Get a reference to the UI website.
                Site uiWebSite = _iisManager.Sites[_uiWebsiteName];

                //Try to start the UI website up to 20 times before we give up.
                bool tryAgain = true;
                int count = 0;
                while (tryAgain && count <= 20)
                {
                    try
                    {
                        //If the UI website is not started, then start it.
                        if (uiWebSite.State != ObjectState.Started)
                        {
                            uiWebSite.Start();
                        } //end if

                        tryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        //Increment.  If we get to 20 then we'll exit the loop.
                        count++;

                        //Log that we failed to restart the website.
                        sRet += "Attempt " + count + " to start the API website failed." + Environment.NewLine;

                        //Delay three seconds before the next attempt.
                        Thread.Sleep(3000);
                    } //end try/catch.
                } //end while loop

                if (count >= 20)
                {
                    sRet += "Could not start the UI website.  Please do it manually." + Environment.NewLine;
                }
                else
                {
                    sRet += "Started the UI website." + Environment.NewLine;
                }

                return sRet;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch
        } //end StartUiWebsite

        public string StartApiWebsite()
        {
            //Start the API website.

            string sRet = "";

            try
            {
                //Get a reference to both the API and UI websites.
                Site apiWebSite = _iisManager.Sites[_apiWebsiteName];

                //Try to start the API website up to 20 times before we give up.
                bool tryAgain = true;
                int count = 0;
                while (tryAgain && count <= 20)
                {
                    try
                    {
                        //If the API website is not started, then start it.
                        if (apiWebSite.State != ObjectState.Started)
                        {
                            apiWebSite.Start();
                        } //end if

                        tryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        //Increment.  If we get to 20 then we'll exit the loop.
                        count++;

                        //Log that we failed to restart the website.
                        sRet += "Attempt " + count + " to start the API website failed." + Environment.NewLine;

                        //Delay three seconds before the next attempt.
                        Thread.Sleep(3000);
                    } //end try/catch.
                } //end while loop

                if (count >= 20)
                {
                    sRet += "Could not start the API website.  Please do it manually." + Environment.NewLine;
                }
                else
                {
                    sRet += "Started the API website." + Environment.NewLine;
                }

                return sRet;
            }
            catch (Exception ex)
            {
                //Could not stop the websites.
                throw new Exception(ex.Message + Environment.NewLine, ex);
            } //end try/catch
        } //end StartApiWebsite

        public string RenameUiFolder()
        {
            //If the UI New folder exists.
            //Delete the UI Old folder if it exists.
            //Raneme the UI folder to UI old.
            //Rename the UI New folder to UI.
            //Else, don't do anything.
            try
            {
                string sRet = "";

                string activePath = Path.Combine(_webFolder, _uiFolderName);
                string newPath = Path.Combine(_webFolder, _uiFolderName + " New");
                string oldPath = Path.Combine(_webFolder, _uiFolderName + " Old");

                //If the "new" folder doesn't exist, then we aren't updateing the UI.
                if (Directory.Exists(newPath))
                {
                    //Delete the existing "Old" directory if there is one.
                    //We should've made a backup copy of it whenever we applied that update anyways.
                    sRet += DeleteFolderIfExists(oldPath);

                    //There is no built-in "rename" functionality.
                    //Instead you move the folder to its new name/location.

                    //Rename the current folder to "old" so that we have a backup
                    //of the current files.
                    Directory.Move(activePath, oldPath);
                    sRet += "Renamed the UI folder to UI Old." + Environment.NewLine;

                    //Rename the "new" folder to be the current folder so that the new
                    //files are in the folder that IIS expects them to reside in..
                    Directory.Move(newPath, activePath);
                    sRet += "Renamed the UI New folder to UI." + Environment.NewLine;
                }
                else
                {
                    sRet = "There is no UI update to apply." + Environment.NewLine;
                } //end if ("new" directory exists).

                return sRet;
            }
            catch (Exception ex)
            {
                //Bubble up any error.
                throw new Exception(ex.Message, ex);
            } //end try/catch.
        } //end RenameUiFolder

        public string RenameApiFolder()
        {
            //If the API New folder exists.
            //Delete the API Old folder if it exists.
            //Raneme the API folder to API old.
            //Rename the API New folder to API.
            //Else, don't do anything.

            try
            {
                string sRet = "";

                string activePath = Path.Combine(_webFolder, _apiFolderName);
                string newPath = Path.Combine(_webFolder, _apiFolderName + " New");
                string oldPath = Path.Combine(_webFolder, _apiFolderName + " Old");

                //If the "new" folder doesn't exist, then we aren't updateing the UI.
                if (Directory.Exists(newPath))
                {
                    //Delete the existing "Old" directory if there is one.
                    //We should've made a backup copy of it whenever we applied that update anyways.
                    sRet += DeleteFolderIfExists(oldPath);

                    //There is no built-in "rename" functionality.
                    //Instead you move the folder to its new name/location.

                    //Rename the current folder to "old" so that we have a backup
                    //of the current files.
                    Directory.Move(activePath, oldPath);
                    sRet += "Renamed the API folder to API Old." + Environment.NewLine;

                    //Rename the "new" folder to be the current folder so that the new
                    //files are in the folder that IIS expects them to reside in..
                    Directory.Move(newPath, activePath);
                    sRet += "Renamed the API New folder to API." + Environment.NewLine;
                }
                else
                {
                    sRet = "There is no API update to apply." + Environment.NewLine;
                } //end if ("new" directory exists).

                return sRet;
            }
            catch (Exception ex)
            {
                //Bubble up any error.
                throw new Exception(ex.Message, ex);
            } //end try/catch.
        } //end RenameApiFolder

        private static string DeleteFolderIfExists(string directoryPath)
        {
            //Get a directory info object for the passed in folder path.
            DirectoryInfo di = new DirectoryInfo(directoryPath);

            try
            {
                string sRet = "";

                //Ensure that the directory exists before trying to delete its contents.
                if (Directory.Exists(di.FullName))
                {
                    //Delete this directory and any files/directories inside of it.
                    di.Delete(true);

                    sRet += "Deleted directory " + directoryPath + Environment.NewLine;
                }
                else
                {
                    sRet += "Directory " + directoryPath + " does not exist and was not deleted." + Environment.NewLine;
                } //end if (does the directory exist?)

                return sRet;
            }
            catch (Exception ex)
            {
                return "Error trying to delete " + directoryPath + Environment.NewLine;
                //Log that there was an error deleting stuff in this folder.
                //throw new Exception(ex.Message, ex);
            } //end try/catch
        } //end DeleteAllFiles

        public bool IsApiUpdate()
        {
            //If the "new" folder does exist, then we are updating the API.
            //Else, we aren't updating it.
            bool bRet = false;

            try
            {
                string newPath = Path.Combine(_webFolder, _apiFolderName + " New");
                bRet = Directory.Exists(newPath);

                return bRet;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        } //end IsApiUpdate

        public bool IsUiUpdate()
        {
            //If the "new" folder does exist, then we are updating the UI.
            //Else, we aren't updating it.
            bool bRet = false;

            try
            {
                string newPath = Path.Combine(_webFolder, _uiFolderName + " New");
                bRet = Directory.Exists(newPath);

                return bRet;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        } //end IsUiUpdate
    } //end class Utilities
}
