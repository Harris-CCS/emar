using System.Threading.Tasks;
using Emar.Core.OutboundChart.Model;

namespace Emar.Core.Helpers
{
    public static class MeaningfulUse
    {
        public static async Task<bool> LogCreation(IUser user, string patientId, string areaCreated, bool activePatient, string? siteNow = null)
        {
            var result = await Log(user, patientId, areaCreated, "CREATED", activePatient, siteNow);
            return result;
        }

        private static async Task<bool> Log(IUser user, string patientId, string area, string action, bool activePatient, string? siteNow = null)
        {
            var chart = new EMR(user.SiteId, patientId, true, false, !activePatient);
            var chartLine = new EMR.Line
            {
                LineHeader = new EMR.Line.Header
                {
                    //This is using now in the web server's time zone.
                    //We need to use "now" in the site's time zone.
                    //We could accept siteNow as an optional parameter here.
                    sys_time = siteNow ?? (new Time()).Timestamp(),
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
