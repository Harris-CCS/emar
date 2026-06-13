using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Orders.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.HomeMedications.Model.Mappings
{
    public static class HomeMedicationMapper
    {
        public static HomeMedicationDto MapHomeMedication(PatientHomeMedication homeMedication)
        {
            if (homeMedication == null)
                return null;

            var homeMedicationDto = new HomeMedicationDto
            {
                Id = homeMedication.Id,
                PatientId = homeMedication.PatientId,
                Class = homeMedication.Class,
                Category = homeMedication.Category,
                InternalDrugId = homeMedication.InternalDrugId,
                MedicationId = homeMedication.MedicationId,
                Medication = MedicationMapper.MapMedication(homeMedication.Medication, null),
                Name = homeMedication.Name,
                AlternateName = homeMedication.AlternateName,
                Dose = homeMedication.Dose,
                MedicationUnit = OrderMapper.MapMedicationUnit(homeMedication.MedicationUnit),
                MedicationRoute = OrderMapper.MapMedicationRoute(homeMedication.MedicationRoute),
                MedicationDrugId = homeMedication.MedicationDrugId,
                IsActive = homeMedication.IsActive,
                Comment = homeMedication.Comment,
                Schedule = homeMedication.Schedule,
                Reaction = homeMedication.Reaction,
                Severity = homeMedication.Severity,
                ParentDrugName = homeMedication.ParentDrugName,
                ActionStatus = homeMedication.ActionStatus,
                LastTakenNote = homeMedication.LastTakenNote
            };

            return homeMedicationDto;
        }
    }
}