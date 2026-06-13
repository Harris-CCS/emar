using Emar.Core.Devices.Model;
using System.Collections.Generic;
using Emar.Data.Entities;
using System;


namespace Emar.Core.Devices.Service
{
    public interface IDeviceService
    {
        IEnumerable<DeviceDto> GetDevices(int siteId, int userId);
        string PrintPdfFile(int extUserId, byte extSiteId, string extPatId, int extDevId, string root, string sourcepath, DateTimeOffset printDateTime, int userId);
        string PrintIPBasedFile(int extUserId, byte extSiteId, string extPatId, int extDevId, string root, string sourcepath, DateTimeOffset printDateTime, int userId);
        string MoveImageFile(int extUserId, byte extSiteId, string extPatId, int extDevId, string sourcepath, string targetPath, string imageType);
        Device GetDeviceById(int deviceId);
        string GetExtPatientId(long patId);
        int GetExtDeviceId(int deviceId);
        int GetExtUserId(int userId);
        byte GetExtSiteId(int siteId);
        string GetIbexRoot(byte siteId);
        string SavePrintFile(Dictionary<string, string> printFileResponses = null);
        string MakePrintFile(Dictionary<string, string> printFileResponses, string sourcePath);
    }
}
