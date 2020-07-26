using Emar.Data.Entities;

namespace Emar.Core.Medications.Model.Mappings
{
    public static class MedicationRouteMapper
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
                Name = medicationRoute.Name,
                SiteId = medicationRoute.SiteId
            };

            return ret;
        }
    }
}
