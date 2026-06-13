using Emar.Core.Devices.Model;
using Emar.Core.Devices.Repository;
using Emar.Data.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Emar.Core.Devices.Service
{
    public partial class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepository;

        public DeviceService(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        //TODO: Accept user_id as a parameter here (and in the interface).
        public IEnumerable<DeviceDto> GetDevices(int siteId, int userId)
        {
            //Call to the repository to get the data from the DB.
            var devices = _deviceRepository.GetDevices(siteId, userId);

            //The abvove should return us a list of DeviceDTO objects, so there's no need to call out to the mapper here.
            //We do the mapping in the repository, becasue it has a class with an extra field that we need.

            ////List of DTO objects.
            //List<DeviceDto> devicesDtos = new List<DeviceDto>();

            ////For each device in the list, map it to a DTO object and add to the DTO list.
            //foreach (Device device in devices)
            //{
            //    devicesDtos.Add(DeviceMapper.MapDevice(device));
            //}

            ////Return.
            return devices;
        }

        public string PrintPdfFile(int extUserId, byte extSiteId, string extPatId, int extDevId,
                                   string root, string sourcepath, DateTimeOffset printDateTime, int userId)
        {
            var eightRandomChars = _deviceRepository.GenerateRandomChars(8);
            // TODO: need to convert pdf here?
            var targetPath = sourcepath;
            //Not needed because Winston's stuff already took the base64 string and converted it to a byte array.
            //var image = _deviceRepository.ConvertBackToImage(sourcepath, targetPath, "");
            // send out pulsemail entry for PDF file printing
            var mail = _deviceRepository.SendInternalMail(extUserId, extSiteId, targetPath, root, eightRandomChars, printDateTime);
            var data = "EMAR Print";
            // chart entry needed for audit log (Meaningful Use)
            var chart = _deviceRepository.SendChartAdminEntry(extPatId, extSiteId, extUserId, data);
            // update the print_history table with the proper file name
            _deviceRepository.updatePrintHistory(userId, printDateTime, extUserId.ToString() + eightRandomChars + ".pdf");

            return "";
        }

        public string PrintIPBasedFile(int extUserId, byte extSiteId, string extPatId, int extDevId,
                                       string root, string sourcepath, DateTimeOffset printDateTime, int userId)
        {
            var fourRandomChars = _deviceRepository.GenerateRandomChars(4);
            // create the file name in the form the print queue expects
            var fileName = _deviceRepository.GetPrinterFileName(extDevId, extUserId, fourRandomChars, root, 1);
            // TODO: need to convert pdf here?
            var targetPath = sourcepath;
            //Not needed because Winston's stuff already took the base64 string and converted it to a byte array.
            //var image = _deviceRepository.ConvertBackToImage(sourcepath, targetPath, "");
            // copy the file to the print queue location - convert to File.Copy like in MoveImageFile()?
            var queueCopy = _deviceRepository.CopyFileToDestination(targetPath, fileName);
            var data = "EMAR Print";
            // chart entry needed for audit log (Meaningful Use)
            var chart = _deviceRepository.SendChartAdminEntry(extPatId, extSiteId, extUserId, data);
            // need to remove path from fileName
            _deviceRepository.updatePrintHistory(userId, printDateTime, fileName.Substring(fileName.LastIndexOf("\\") + 1));

            return "";
        }

        public string MoveImageFile(int extUserId, byte extSiteId, string extPatId, int extDevId,
                                    string sourcepath, string targetPath, string imageType)
        {
            //Not needed because Winston's stuff already took the base64 string and converted it to a byte array.
            // var image = _deviceRepository.ConvertBackToImage(sourcepath, targetPath, imageType);
            try
            {
                File.Copy(sourcepath, targetPath, true);
            }
            catch (Exception e)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = e.Message + "\n" + "inner exception = " + e.InnerException + "\n" + "source = " + e.Source + "\n" + e.StackTrace + "\n";
                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                    return "Error: exception in MoveImageFile";
                }
            }
            
            var data = "EMAR Print";
            var chart = _deviceRepository.SendChartAdminEntry(extPatId, extSiteId, extUserId, data);

            return "";
        }

        public Device GetDeviceById(int deviceId)
        {
            var device = _deviceRepository.GetDeviceById(deviceId);

            return device;
        }

        public string GetExtPatientId(long patId)
        {
            var patient = _deviceRepository.GetExtPatientId(patId);

            return patient;
        }

        public int GetExtDeviceId(int deviceId)
        {
            var device = _deviceRepository.GetExtDeviceId(deviceId);

            return int.TryParse(device, out int number) ? number : 0;
        }

        public int GetExtUserId(int userId)
        {
            var user = _deviceRepository.GetExtUserId(userId);

            return user;
        }

        public byte GetExtSiteId(int siteId)
        {
            var site = _deviceRepository.GetExtSiteId(siteId);

            return site;
        }

        public string GetIbexRoot(byte siteId)
        {
            var root = _deviceRepository.GetIbexRoot(siteId);

            return root;
        }
        
        public string SavePrintFile(Dictionary<string, string> printFileResponses = null)
        {
            //Call to the repository to write the base 64 string to disk as a PDF file,
            //save an entry into the print_history table,
            //and return the path to the PDF file.
            return _deviceRepository.SavePrintFile(printFileResponses);
        }

        public string MakePrintFile(Dictionary<string, string> printFileResponses, string sourcepath)
        {
            var deviceId = Convert.ToInt32(printFileResponses["device_id"]);
            var device = GetDeviceById(deviceId);
            if (device == null)
                return "Error: Could not get device by Id";

            // get all the params needed for the device type method call below
            var printUserId = Convert.ToInt32(printFileResponses["user_id_printing"]);
            var extUserId = GetExtUserId(printUserId);
            var extSiteId = GetExtSiteId(device.SiteId);
            var extPatId = GetExtPatientId(Convert.ToInt32(printFileResponses["patient_id"]));
            var extDevId = GetExtDeviceId(deviceId);
            var root = GetIbexRoot(extSiteId);
            var printDateTime = Convert.ToDateTime(printFileResponses["date_time"]);

            switch (device.DeviceType)
            {
                case "I":
                    // Export report aka IP Based printer
                    var ipReturn = PrintIPBasedFile(extUserId, extSiteId, extPatId, extDevId, root, sourcepath, printDateTime, printUserId);
                    break;
                case "D":
                    // PDF Printer
                    var pdfReturn = PrintPdfFile(extUserId, extSiteId, extPatId, extDevId, root, sourcepath, printDateTime, printUserId);
                    break;
                case "W":
                    // File directory aka Windows shared
                    if (string.IsNullOrWhiteSpace(device.Address) || string.IsNullOrWhiteSpace(printFileResponses["file_name"]))
                        break;
                    var targetPath = device.Address;
                    if (!device.Address.EndsWith("\\"))
                        targetPath += "\\";
                    targetPath += printFileResponses["file_name"];
                    var imgReturn = MoveImageFile(extUserId, extSiteId, extPatId, extDevId, sourcepath, targetPath, "");
                    break;
                default:
                    break;
            }

            try
            {
                // delete original temp file if still exists
                if (File.Exists(sourcepath))
                {
                    File.Delete(sourcepath);
                }
            }
            catch (Exception e)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    string sException = e.Message + "\n" + "inner exception = " + e.InnerException + "\n" + "source = " + e.Source + "\n" + e.StackTrace + "\n";
                    eventLog.Source = "PulseCheck EMAR API";
                    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                    return "Error: exception in MakePrintFile";
                }
            }

            return "";
        }
    }
}
