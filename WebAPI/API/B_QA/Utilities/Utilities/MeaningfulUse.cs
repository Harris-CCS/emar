using Interfaces.DomainModel;
using System.Threading.Tasks;

namespace PulseCheck.Utilities
{
    public static class MeaningfulUse
    {
        public static async Task<bool> LogAccess(IUser user, string patientId, string areaAccessed)
        {
            var result = await Log(user, patientId, areaAccessed, "ACCESSED");
            return result;
        }

        public static async Task<bool> LogCreation(IUser user, string patientId, string areaCreated)
        {
            var result = await Log(user, patientId, areaCreated, "CREATED");
            return result;
        }

        public static async Task<bool> LogModification(IUser user, string patientId, string areaModified)
        {
            var result = await Log(user, patientId, areaModified, "MODIFIED");
            return result;
        }

        private static async Task<bool> Log(IUser user, string patientId, string area, string action)
        {
            var chart = new EMR(user.SiteId, patientId, true);
            var chartLine = new EMR.Line
            {
                LineHeader = new EMR.Line.Header
                {
                    sys_time = (new Time()).Timestamp(),
                    user = user.Id,
                },
                LinePart = new EMR.Line.Part
                {
                    part = area + " " + action,
                    section = EMR.Constants.SECT_ADMIN,
                    nct = EMR.Constants.NCT_MEANINGFUL_USE
                },
            };
            return chart.WriteLine(chartLine);
        }
    }
}
