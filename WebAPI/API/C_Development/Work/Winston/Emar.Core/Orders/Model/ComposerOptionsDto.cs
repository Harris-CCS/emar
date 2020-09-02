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
        public IEnumerable<MockFrequencyDto> PreferredFrequencies { get; set; }
    }

    public class PreferredDoseDto
    {
        public string DoseName { get; set; }
        public decimal Dose { get; set; }
        public MockUnitDto DoseMockUnit { get; set; }
    }

    public class MockUnitDto
    {
        public int Id { get; set; }
        public string UnitName { get; set; }
    }

    public class MockFrequencyDto
    {
        public int Id { get; set; }
        public string FrequencyName { get; set; }
    }
}