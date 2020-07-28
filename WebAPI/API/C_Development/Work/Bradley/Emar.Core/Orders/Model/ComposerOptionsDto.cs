using System.Collections.Generic;
using Emar.Core.Medications.Model;

namespace Emar.Core.Orders.Model
{
    public class ComposerOptionsDto
    {
        public string BrandName { get; set; }
        public IEnumerable<FormStrengthDto> AvailableFormStrength { get; set; }
    }

    public class FormStrengthDto
    {
        public int Id { get; set; }
        public string FormStrengthName { get; set; }
        public IEnumerable<MedicationRouteDto> AvailableRoutes { get; set; }
        public IEnumerable<PreferredDoseDto> PreferredDoses { get; set; }
        public IEnumerable<MedicationRouteDto> PreferredRoutes { get; set; }
        public IEnumerable<FrequencyDto> PreferredFrequencies { get; set; }
    }

    public class PreferredDoseDto
    {
        public string DoseName { get; set; }
        public decimal Dose { get; set; }
        public UnitDto DoseUnit { get; set; }
    }

    public class UnitDto
    {
        public int Id { get; set; }
        public string UnitName { get; set; }
    }

    public class FrequencyDto
    {
        public int Id { get; set; }
        public string FrequencyName { get; set; }
    }
}