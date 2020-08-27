using Emar.Core.Orders.Model;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Model.Mappings
{
    public static class MedicationMapper
    {
        public static MedicationRouteDto MapMedicationRoute(MedicationRoute medicationRoute)
        {
            if (medicationRoute == null)
            {
                return null;
            }

            var ret = new MedicationRouteDto
            {
                Id = medicationRoute.Id,
                RouteName = medicationRoute.Name,
                SiteId = medicationRoute.SiteId
            };

            return ret;
        }

        public static MedicationUnitDto MapMedicationUnit(MedicationUnit medicationUnit)
        {
            if (medicationUnit == null)
            {
                return null;
            }

            var ret = new MedicationUnitDto
            {
                Id = medicationUnit.Id,
                UnitName = medicationUnit.Name,
                SiteId = medicationUnit.SiteId,
                Code = medicationUnit.Code,
                PrintName = medicationUnit.PrintName,
                Active = medicationUnit.IsActive
            };

            return ret;
        }

        public static FrequencyScheduleDto MapMedicationFrequency(FrequencySchedule dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new FrequencyScheduleDto
            {
                Id = dbObj.Id,
                ScheduleName = dbObj.Name,
                SiteId = dbObj.SiteId,
                PointInTime = dbObj.PointInTime,
                Notes = dbObj.Notes
                //int FrequencyTypeId { get; set; }
                //int FrequencyTypeRecurring { get; set; }
                //int FrequencyInterval { get; set; }
                //int FrequencyIntervalUnitId { get; set; }
                //TimeSpan IntervalStartTime { get; set; }
                //short IntervalEndMinutes { get; set; }
            };
            
            return ret;
        }
    }
}
