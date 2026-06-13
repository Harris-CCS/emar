using Emar.Core.Devices.Model;
using Emar.Core.Helpers;
using Emar.Data;
using Emar.Data.Entities;
using System;
using System.Data;
using System.Data.SqlClient;
//using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using OcsSite = Emar.Core.OutboundChart.Model.Site;

namespace Emar.Core.Devices.Repository
{
    public partial class DeviceRepository : IDeviceRepository
    {
        private readonly EmarContext _context;

        public DeviceRepository(EmarContext emarContext)
        {
            _context = emarContext;
        }

        public IEnumerable<DeviceDto> GetDevices(int siteId, int userId)
        {
            //***********************************************************************************************
            //Winston Murdock, 01/31/2021.  EMAR-???
            //1. Get the last used device for this user(if any).
            //  a. Go grab any rows from the devices table.
            //  b. Inner Join to user_settings on devices.id
            //  c. Inner Join to settings on userr_settings.settings_id
            //  d. where settings.name = ‘LAST_USED_PRINTER’
            //  e. and user_settings.user_id matches
            //  f. and device.site_id matches
            //  g. and the device is active.
            //2. Store the results in a class that only exists in this repository rather than a device entity
            //   (so that I can also have a “IsLastUsed” flag, which will be set to true).
            //3. The return list will have either zero rows or one row at this point.
            //4. If the return list has one row
            //  a. Grab the ID into a local variable.
            //  b. Add all devices except that one to the return list.
            //    I. Grab any rows from the devices table.
            //    II. Where the site_id matches
            //    III. and where the id is not the ID of the last used device
            //    IV. and the device is active.
            //    V. Store the results into the same class as before but seting IsLastUsed to false.
            //5. Else the return list has zero rows
            //  a. Add all devices to the return list.
            //    I. Grab any rows from the devices table.
            //    II. Where the site_id matches
            //    III. and the device is active.
            //    IV. Store the results into the same class as before but seting IsLastUsed to false.
            //6. Sort the return list by IsLastUsed and then by Description.
            //7. Return the return list.
            //***********************************************************************************************

            int lastUsedDeviceId = -1;

            //Get the last used device for this user, if any exists.
            //Grab all devices
            //  join to user settings on the device id
            //  join to settings on the setting id
            //where the setting name is "LAST_USED_PRINTER"
            //  and the user setting's user id matches the user's id
            //  and the device's site id matches the site's id.
            //  and is active is true.
            var devices =
            (
                from d in _context.Devices
                join us in _context.UserSettings on d.Id.ToString() equals us.SettingValue
                join s in _context.Settings on us.SettingId equals s.Id
                where s.Name == "LAST_USED_PRINTER"
                    && us.UserId == userId
                    && d.SiteId == siteId
                    && d.IsActive == true
                select
                (
                    //Select into our class rather than into a device entity, so that we have the IsLastUsed flag.
                    new DeviceWithLastUsed
                    {
                        Id = d.Id,
                        SiteId = d.SiteId,
                        Address = d.Address,
                        Description = d.Description,
                        IsActive = d.IsActive,
                        PrintQueueName = d.PrintQueueName,
                        Tray = d.Tray,
                        DeviceType = d.DeviceType,
                        PclType = d.PclType,
                        IsLastUsed = true
                    }
                )
            ).ToList();

            //Now grab the ID of the last used device, if there is one.
            if (devices.Count > 0)
            {
                //We know there is a device in here. Grab its ID.
                lastUsedDeviceId = devices[0].Id;
            } //end if (count > 0)?

            //If we did set lastUsedDeviceId above, then filter to not include it here.
            //Else, don't filter on Device ID.
            if (lastUsedDeviceId == -1)
            {
                //Grab all devices except the one we've already added to the list.
                //Once we have that list, add it to the return list.
                //Filter by site id and where is active is true.
                //Again, we're using our custom class to set IsLastUsed to false.
                devices.AddRange
                (
                    from d in _context.Devices
                    where d.SiteId == siteId
                        && d.IsActive == true
                    select
                    (
                        new DeviceWithLastUsed
                        {
                            Id = d.Id,
                            SiteId = d.SiteId,
                            Address = d.Address,
                            Description = d.Description,
                            IsActive = d.IsActive,
                            PrintQueueName = d.PrintQueueName,
                            Tray = d.Tray,
                            DeviceType = d.DeviceType,
                            PclType = d.PclType,
                            IsLastUsed = false
                        }
                    )
                );
            }
            else
            {
                //There is not last used device for this user.
                //Grab the list of all devices from the table
                //and add it to the return list.
                //Filter by site id and where is active is true.
                //Again, we're using our custom class to set IsLastUsed to false.
                devices.AddRange
                (
                    from d in _context.Devices
                    where d.SiteId == siteId
                        && d.Id != lastUsedDeviceId
                        && d.IsActive == true
                    select
                    (
                        new DeviceWithLastUsed
                        {
                            Id = d.Id,
                            SiteId = d.SiteId,
                            Address = d.Address,
                            Description = d.Description,
                            IsActive = d.IsActive,
                            PrintQueueName = d.PrintQueueName,
                            Tray = d.Tray,
                            DeviceType = d.DeviceType,
                            PclType = d.PclType,
                            IsLastUsed = false
                        }
                    )
                );
            } //end if

            //var devices2 = _context.Devices.Where(s => s.SiteId == siteId).OrderBy(t => t.Description).ToList();
            //return devices2;

            //The "devices" variable is a list of DeviceWithLastUsed objects.
            //Convert it to a list of DeviceDto objects before returning it to the service.
            return devices.Select
            (
                d => new DeviceDto
                {
                    Id = d.Id,
                    SiteId = d.SiteId,
                    Address = d.Address,
                    Description = d.Description,
                    IsActive = d.IsActive,
                    PrintQueueName = d.PrintQueueName,
                    Tray = d.Tray,
                    DeviceType = d.DeviceType,
                    PclType = d.PclType,
                    IsLastUsed = d.IsLastUsed
                }
            //Order by IsLastUsed desending (since false is 0 and true is 1).
            //Then order by Description.
            //This way, we'll ahve the last used device listed first, and then
            //the rest of the devices ordered by their name.
            ).OrderByDescending(i => i.IsLastUsed).ThenBy(i => i.Description);
        } //end GetDevices

        public string GetPrinterFileName(int extDeviceId, int extUserId, string fourRandomChars, string root, int numOfCopies)
        {
            // get epoch time component of file name
            // needs to be in sync with the print queue calculated epoch time
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time = Convert.ToInt64((DateTime.Now.ToUniversalTime() - epoch).TotalSeconds).ToString();

            // concatenate all the above strings into the full file name path for use by the ibex print queue
            var printerFileName = root + "link\\prt\\" + fourRandomChars + '.' + extUserId.ToString() + '_' + extDeviceId + "_N_" + numOfCopies.ToString() + '_' + time + "_emarmedsvc.pdf";

            return printerFileName;
        }

        public string CopyFileToDestination(string sourcePath, string targetPath)
        {
            // for examples on running from command prompt, see
            // https://stackoverflow.com/questions/1469764/run-command-prompt-commands

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C copy /Y " + sourcePath + " " + targetPath;
            process.StartInfo = startInfo;
            process.Start();

            return "";
        }

        public string SendChartAdminEntry(string extPatientId, byte extSiteId, int extUserId, string data)
        {
            var EMR = new EMR(extSiteId, extPatientId, true);
            var line = new EMR.Line();
            line.LineHeader.sys_time = DateTime.Now.ToString("yyyyMMddHHmmss");
            line.LineHeader.user = extUserId;
            line.LinePart.nct = EMR.Constants.NCT_MEANINGFUL_USE;
            line.LinePart.section = EMR.Constants.SECT_ADMIN;
    //        line.LinePart.part = "MEDICATION SERVICE PRINTED";
            // Audit log report will need to be revisited to be sure it can identify this new part
            line.LinePart.part = "MEDICATION ADMINISTRATION RECORD PRINTED";
            line.DataSegments = new List<EMR.Line.DataSegment>
            {
                new EMR.Line.DataSegment(EMR.Line.DataSegment.Constants.TYPE_DROPDOWN, data)
            };

            if (EMR.WriteLine(line, extUserId))
                return "";
            else
                return "Error: WriteLine() fail in SendChartAdminEntry";
        }

        public string ConvertBackToImage(string inputPath, string outputPath, string imageType)
        {
            if (!File.Exists(inputPath))
            {
                return "Error: bad input path";
            }

            string imageText = File.ReadAllText(inputPath);
            if (string.IsNullOrEmpty(imageText))
                return "Error: empty input file";

            if (imageText.IndexOf(",") >= 0)
                imageText.Substring(imageText.IndexOf(",") + 1);
            // check for invalid base 64 chars beforehand?
            byte[] bytes = Convert.FromBase64String(imageText);

            // Will leave the commented out block here for now since may want to revisit this
            // if more control is needed over the decoding process. Was also having an issue with
            // using System.Drawing and including a proper assembly reference in the project.

            //            Image image;
            //            using (MemoryStream ms = new MemoryStream(bytes))
            //            {
            //                image = Image.FromStream(ms);
            //                System.Drawing.Imaging.ImageFormat format;
            //                switch (imageType)
            //                {
            //                    case "Jpeg":
            //                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
            //                        break;
            //                    case "Tiff":
            //                        format = System.Drawing.Imaging.ImageFormat.Tiff;
            //                        break;
            //                    case "Png":
            //                        format = System.Drawing.Imaging.ImageFormat.Png;
            //                        break;
            //                    default:
            //                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
            //                        break;
            //                }
            //                image.Save(outputPath, format);
            //            }

            // need to validate outputPath that folder exists and filename is valid?
            using (var imageFile = new FileStream(outputPath, FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }

            return "";
        }

        public string SendInternalMail(int extUserID, byte extSiteId, string sourcePath, string root, string eightRandomChars, DateTimeOffset printDateTime)
        {
            var result = CopyFileToDestination(sourcePath, root + "temp\\" + extUserID.ToString() + eightRandomChars + ".pdf");
            var message = "Your PDF document is ready and can be viewed by clicking the link below.\n"
                        + "<A HREF=\"ibex05.mpex?pdf=1&c=" + eightRandomChars + "\" TARGET=\"_new\">"
                        + "EMAR Medication Services printed at " + printDateTime.ToString("F") +
                        "</A>" + "\nNote: Acrobat Reader is required and PDF files are removed after seven days.\n";
            var site = new OcsSite(extSiteId);
            var internalMail = new PulseMail(site);
            if (internalMail != null)
            {
                if (!internalMail.SendMessage(extUserID, "PDF Print EMAR Medication Services", message, 0, eightRandomChars))
                    // need error message return or let the SendMessage() error handling deal with it?
                    return "";
            }

            return "";
        }

        public string GenerateRandomChars(int numChars)
        {
            // borrowed code from Emar.Core.Helpers.PulseMail RandomCharacters()
            // chose to separate from PulseMail class to eliminate object construction costs
            char[] chars = new char[36];
            chars = "abcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[numChars];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(numChars);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public void updatePrintHistory(int userId, DateTimeOffset printDateTime, string newFileName)
        {
            // find print history entry - use PrintUserId and PrintDateTime (may need to change)
            var history = _context.PrintHistory.First(h => h.PrintUserId == userId && h.PrintDateTime == printDateTime);
            history.FileName = newFileName;
            _context.Entry(history).Property(p => p.FileName).IsModified = true;
            _context.SaveChanges();
        }

        public Device GetDeviceById(int deviceId)
        {
            var device = from d in _context.Devices
                         where d.Id == deviceId
                         select
                         (
                             new Device
                             {
                                 Id = d.Id,
                                 SiteId = d.SiteId,
                                 Address = d.Address,
                                 Description = d.Description,
                                 IsActive = d.IsActive,
                                 PrintQueueName = d.PrintQueueName,
                                 Tray = d.Tray,
                                 DeviceType = d.DeviceType,
                                 PclType = d.PclType
                             }
                         );
            return device.FirstOrDefault();
        }

        public string GetExtPatientId(long patId)
        {
            var patient = from e in _context.ExternalIds
                          where e.InternalId == patId
                                && e.Entity == "patients"
                                && e.Vendor == "pulsecheck"
                          select e.ExternalId;
            return patient.FirstOrDefault().Split("|")[1];
        }

        public string GetExtDeviceId(int deviceId)
        {
            var device = from e in _context.ExternalIds
                         where e.InternalId == deviceId
                               && e.Entity == "devices"
                               && e.Vendor == "pulsecheck"
                         select e.ExternalId;
            return device.FirstOrDefault().Split("|")[1];
        }

        public int GetExtUserId(int userId)
        {
            var user = from e in _context.ExternalIds
                       where e.InternalId == userId
                             && e.Entity == "users"
                             && e.Vendor == "pulsecheck"
                       select e.ExternalId;
            return int.TryParse(user.FirstOrDefault(), out int userNum) ? userNum : 0;
        }

        public byte GetExtSiteId(int siteId)
        {
            var site = from e in _context.ExternalIds
                       where e.InternalId == siteId
                             && e.Entity == "sites"
                             && e.Vendor == "pulsecheck"
                       select e.ExternalId;
            return byte.TryParse(site.FirstOrDefault(), out byte siteNum) ? siteNum : (byte)0;
        }

        public string GetIbexRoot(byte siteId)
        {
            // get the data from the ibex..org table based upon the site
            // don't currently have this data stored in emar
            var info = new DB.Select
            {
                Sql = "SELECT root,mailroot FROM org WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId}
                }
            }.RunForDataRow();
            if (info == null)
                return "";

            return info["root"]?.ToString().Trim();
        }
        
        public string SavePrintFile(Dictionary<string, string> printFileResponses = null)
        {
            try
            {
                //Grab the fields from the HTML form and save into the PrintHistory object.
                PrintHistory printHistory = new PrintHistory();
                printHistory.PrintUserId = Convert.ToInt32(printFileResponses["user_id_printing"]);
                printHistory.DeviceId = Convert.ToInt32(printFileResponses["device_id"]);
                printHistory.PatientId = Convert.ToInt32(printFileResponses["patient_id"]);
                printHistory.Description = printFileResponses["description"];
                printHistory.DocumentType = printFileResponses["document_type"];
                printHistory.FileName = printFileResponses["file_name"];
                printHistory.PageCount = Convert.ToInt32(printFileResponses["page_count"]);
                printHistory.PrintDateTime = Convert.ToDateTime(printFileResponses["date_time"]);
                printHistory.ExpirationDateTime = Convert.ToDateTime(printFileResponses["expiration_documentation"]);
                printHistory.PrintBody = printFileResponses["content"];

                //Per email thread from 05/05/2021, the UI is not sending us this field.
                //Instead I'll grab the extension off the end of the file name (everything after the last period)
                //and put it in the FileFormat field (since it doesn't allow nulls in the DB).
                //printHistory.FileFormat = printFileResponses["file_format"];
                //Winston Murdock, 05/05/2021.  EMAR-444
                printHistory.FileFormat = printHistory.FileName.Substring(printHistory.FileName.LastIndexOf('.') + 1);

                //In case there's no file extension specified, default to pdf.
                if (printHistory.FileFormat.Length < 1)
                {
                    printHistory.FileFormat = "pdf";
                } //end if

                //Get rid of "data:application/pdf;base64," from the beginning of the string.
                //If there's a comman in the string at all (comma is not valid in a base 64 string.
                if (printHistory.PrintBody.IndexOf(",") >= 0)
                {
                    //Only keep everything after the first comma.
                    printHistory.PrintBody = printHistory.PrintBody.Substring(printHistory.PrintBody.IndexOf(",") + 1);
                } //end if

                //Convert the string of data to a byte array.
                byte[] bytes = Convert.FromBase64String(printHistory.PrintBody);
                
                //Get the external site id (device id -> emar site id -> external site id).
                var ibexSiteId =
                (
                    from s in _context.Sites
                    join d in _context.Devices on s.Id equals d.SiteId
                    join e in _context.ExternalIds
                        on s.Id equals e.InternalId
                    //    on new {SiteId = s.Id, Entity = "s" }
                    //    equals new {e.InternalId, e.Entity}
                    where d.Id == printHistory.DeviceId
                        && e.Entity == "sites"
                    select
                    (
                        e.ExternalId
                    )
                ).FirstOrDefault();

                //Call Jim's helper to get the ibex root.
                //The External ID column in external_ids is a string.  But we need a byte here.
                //So convert it before we call GetIbexRoot
                string sPath = GetIbexRoot(Convert.ToByte(ibexSiteId));

                //The above is returning the path with four slashes as folder separators instead of two.
                //Handle that here.
                sPath = sPath.Replace("\\\\", "\\");

                //Append the temp folder and then append the file name.
                //Jim's function returns the path WITH the ending slash.
                sPath += "temp\\" + printHistory.FileName;

                //Write the byte array to disk.
                bool bWriteWorked = WriteBytesToDisk(bytes, sPath);

                //If the save to disk was successful, then log this in the print history table and return a success message.
                //Else, bubble up an error.
                if (bWriteWorked)
                {
                    //Now that we've got the file info and have saved the file to disk,
                    //Insert the info for this print into the print_history table.
                    _context.PrintHistory.Add(printHistory);
                    _context.SaveChanges();

                    //Return the path to the PDF file so that Jim's stuff in the controller has it.
                    return sPath;
                }
                else
                {
                    throw new Exception("There was an error printing the file.  Please try again.");
                } //end if (bWriteWorked)
            }
            catch (Exception ex)
            {
                //Log any errors to the Event Viewer.
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = ex.Message + "\n";
                    sException += "source = " + ex.Source + "\n";
                    sException += "inner exception = " + ex.InnerException + "\n";
                    sException += ex.StackTrace + "\n";

                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                } //end using.

                //Pass the error up the chain.
                throw new Exception(ex.Message, ex.InnerException);
            } //end try/catch
        }

        public bool WriteBytesToDisk(byte[] data, string filePath)
        {
            BinaryWriter Writer = null;

            try
            {
                // Create a new stream to write to the file
                Writer = new BinaryWriter(File.OpenWrite(filePath));

                // Writer raw data                
                Writer.Write(data);
                Writer.Flush();
                Writer.Close();

                //When we do a file print (where we just take the file that
                //I have created) and copy it to a networked rive,
                //we sometimes run into an issue where we still have the file
                //open and cannot delete it form this directory after copying it.
                //Google led Jim and I to the Dispose call.
                //Theoeretically, it's already being called by internal C# code.
                //But we think we can force it to happen earlier by directly calling it here.
                //Winston Murdock, 05/07/2021.  EMAR-496.
                Writer.Dispose();
            }
            catch (Exception e)
            {
                //Log the issue to the event viewer.
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = e.Message + "\n";
                    sException += "inner exception = " + e.InnerException + "\n";
                    sException += "source = " + e.Source + "\n";
                    sException += e.StackTrace + "\n";

                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                } //end using.
                
                //Return false.
                return false;
            } //end try/catch.

            //No issues writing the file.
            //Return true.
            return true;
        } //end WriteBytesToDisk

        //Class only for use in this repository.
        //This is the same as the Device entity from Emar.Data, except I've 
        //added a boolean flag "IsLastUsedDevice."
        private class DeviceWithLastUsed
        {
            public int Id { get; set; }

            public int SiteId { get; set; }

            public string Address { get; set; }

            public string Description { get; set; }

            public bool IsActive { get; set; }

            public string PrintQueueName { get; set; }

            public string Tray { get; set; }

            public string DeviceType { get; set; }

            public string PclType { get; set; }

            public bool IsLastUsed { get; set; }
        } //end class MedicationLookup
    } //end class DeviceRepository
}
