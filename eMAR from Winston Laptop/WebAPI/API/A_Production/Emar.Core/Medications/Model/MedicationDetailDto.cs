using System;
using Emar.Core.FdbObjects.Model;
using Emar.Core.Orders.Model;

namespace Emar.Core.Medications.Model
{
    public class MedicationDetailDto
    {
        public int Id { get; set; } 
        public int MedicationId { get; set; }
        public string DrugId { get; set; }
        public string BrandName { get; set; }
        public string ActiveList { get; set; }
        public decimal? Dose { get; set; }
        internal int? MedicationUnitId { get; set; }
        public MedicationUnitDto DoseUnit { get; set; }
        internal int? MedicationRouteId { get; set; }
        public bool IsActive { get; set; }

        internal FdbBrandNameDto FdbBrandName { get; set; }

        public string GetName()
        {
            if (BrandName.Equals(ActiveList) || String.IsNullOrWhiteSpace(ActiveList))
            {
                return BrandName;
            }
            else
            {
                return BrandName + " (" + ActiveList + ")";
            }
        }
    }
}