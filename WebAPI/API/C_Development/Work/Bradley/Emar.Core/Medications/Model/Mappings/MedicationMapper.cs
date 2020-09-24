using System.Linq;
using Emar.Core.Sites.Model.Mappings;
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
            };
            
            return ret;
        }

        public static MedicationDto MapMedication(Data.Entities.Medication dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new MedicationDto
            {
                Id = dbObj.Id,
                SiteId = dbObj.SiteId,
                Site = SiteMapper.MapSite(dbObj.Site),
                DrugId = dbObj.DrugId,
                DisplayName = dbObj.DisplayName,
                DrugVendor = dbObj.DrugVendor,
                MedicationDetails = dbObj.MedicationDetails?.Select(MapMedicationDetail).ToList()
            };

            return ret;
        }

        private static MedicationDetailDto MapMedicationDetail(MedicationDetail dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new MedicationDetailDto
            {
                Id = dbObj.Id,
                MedicationId = dbObj.MedicationId,
                DrugId = dbObj.DrugId,
                BrandName = dbObj.BrandName,
                ActiveList = dbObj.ActiveList,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                IsActive = dbObj.IsActive
            };

            return ret;
        }
    }
}
