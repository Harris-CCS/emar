using Emar.Core.Devices.Model;
using Emar.Data.Entities;
using System;
using System.Collections.Generic;

namespace Emar.Core.Devices.Repository
{
    public interface IDeviceRepository
    {
        IEnumerable<DeviceDto> GetDevices(int siteId, int userId);
        string GetPrinterFileName(int deviceId, int userId, string fourRandomChars, string root, int numOfCopies);
        string CopyFileToDestination(string sourcePath, string targetPath);
        string SendChartAdminEntry(string extPatientId, byte extSiteId, int extUserId, string data, bool activePatient);
        string ConvertBackToImage(string outputPath, string imageText, string imageType);
        string SendInternalMail(int extUserID, byte extSiteId, string sourcePath, string root, string eightRandomChars, DateTimeOffset printDateTime);
        string GenerateRandomChars(int numChars);
        void updatePrintHistory(int userId, DateTimeOffset printDateTime, string newName);
        Device GetDeviceById(int deviceId);
        string GetExtPatientId(long patId);
        string GetExtDeviceId(int deviceId);
        int GetExtUserId(int userId);
        byte GetExtSiteId(int siteId);
        string GetIbexRoot(byte siteId);
        string GetInternalDeviceId(int siteId, string externalDeviceId);
        bool GetPatientStatus(long patId);
        string SavePrintFile(Dictionary<string, string> printFileResponses = null);
    }
}
