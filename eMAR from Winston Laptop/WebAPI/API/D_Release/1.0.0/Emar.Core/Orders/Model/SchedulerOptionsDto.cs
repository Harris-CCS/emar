using System.Collections.Generic;
using Emar.Core.Medications.Model;

namespace Emar.Core.Orders.Model
{
    public class SchedulerOptionsDto
    {
        public string BrandName { get; set; }
        public List<FormStrengthDto> AvailableFormStrength { get; set; }
        public List<FrequencyScheduleAdministrationDto> Administrations { get; set; }
        public List<OrderInstructionDto> AdministrationInstructions { get; set; }
    }

    public class FormStrengthDto
    {
        public bool Combo { get; set; }
        public IEnumerable<MedicationDetailDto> MedicationDetails { get; set; }
        public int MedicationId { get; set; }
        public bool AntimicrobialRequiredIndicator { get; set; }
        public string FormStrengthName { get; set; }
        public IEnumerable<PreferredDoseDto> PreferredDoses { get; set; }
        public IEnumerable<MedicationRouteDto> PreferredRoutes { get; set; }
        public IEnumerable<FrequencyScheduleDto> PreferredFrequencies { get; set; }
    }

    public class PreferredDoseDto
    {
        public decimal Dose { get; set; }
        public MedicationUnitDto DoseUnit { get; set; }
        //Add a property that is the dose and unit conatenated into one field.
        //This will let us group by it and then select the first to
        //simulate a select distinct.
        //https://stackoverflow.com/a/14321048
        //Winston Murdock, 03/03/2021.  EMAR-824
        public string DosePlusUnit { get; set; }
    }
}