using Emar.Core.Devices.Model;
using Emar.Data;
using System.Collections.Generic;
using System.Linq;

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
