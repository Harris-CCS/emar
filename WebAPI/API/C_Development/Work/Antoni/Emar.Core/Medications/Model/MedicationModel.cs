using System;
using System.Collections.Generic;

namespace Emar.Core.Medications.Model
{
    public class MedicationInteractionReaction
    {
        public int SiteId { get; set; }
        public long? PatientId { get; set; }
        public int UserId { get; set; }
        public string SourceTable { get; set; }
        public long? SourceTableId { get; set; }
        public EmarOrderType Type { get; set; }
        public string BrandName { get; set; }
        public string ActiveName { get; set; }
        public string ActiveId { get; set; }

        public List<Dictionary<string, string>> Interactions { get; set; } = new List<Dictionary<string, string>>();
        public List<Dictionary<string, string>> Reactions { get; set; } = new List<Dictionary<string, string>>();
    }

    public class MedicationModel
    {
        public long Id { get; set; }
        public int SiteId { get; set; }
        public long PatientId { get; set; }
        public int UserId { get; set; }
        public string SourceTable { get; set; }
        public long? SourceTableId { get; set; }
        public EmarOrderType Type { get; set; }
        public DateTimeOffset? AddDatetime { get; set; }
        public int? AddUserId { get; set; }
        public string AlternateName { get; set; }
        public DateTimeOffset? BeginDatetime { get; set; }
        public string Category { get; set; }
        public DateTimeOffset? ChangeDatetime { get; set; }
        public int? ChangeUserId { get; set; }
        public string Class { get; set; }
        public string Comment { get; set; }
        public decimal? Dose { get; set; }
        public DateTimeOffset? EndDatetime { get; set; }
        public int? FrequencyScheduleId { get; set; }
        public string InternalDrugId { get; set; }
        public bool? IsActive { get; set; }
        public string MedicationDrugId { get; set; }
        internal int MedicationId { get; set; }
        public MedicationDto Medication { get; set; }
        public int? MedicationRouteId { get; set; }
        public int? MedicationUnitId { get; set; }
        public int? OrderPhysicianUserId { get; set; }
        public string OrderStatus { get; set; }
        public string ParentDrugId { get; set; }
        public string ParentDrugName { get; set; }
        public bool? PointInTime { get; set; }
        public byte? Priority { get; set; }
        public bool? Prn { get; set; }
        public string Reaction { get; set; }
        public string Schedule { get; set; }
        public string Severity { get; set; }

        ///  these stuff exist in allergies but not in home meds
        public string Name { get; set; }
        public string AllergyDrugId { get; set; }
        public string ActionStatus { get; set; }
        public string InformationSource { get; set; }
        public string PersonNumber { get; set; }
        public string AccountNumber { get; set; }

        public List<Dictionary<string, string>> Interactions { get; set; } = new List<Dictionary<string, string>>();
        public List<Dictionary<string, string>> Reactions { get; set; } = new List<Dictionary<string, string>>();
    }
}