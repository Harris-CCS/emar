using System;
using Emar.Core.Helpers;
using Emar.Core.Users.Model;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Model
{
    public class DrugInteractionViewDto
    {
        public long Id { get; set; }
        public string InteractionDrug1 { get; set; }
        public string InteractionDrug2 { get; set; }
        public string Severity { get; set; }
        internal int? OverrideReasonId { get; set; }
        public OverrideReasonDto OverrideReason { get; set; }
        internal int? OverrideReasonUserId { get; set; }
        public UserDto OverrideReasonUser { get; set; }
        public DateTimeOffset? OverrideReasonDatetime { get; set; }
        public long? InteractionOrderId { get; set; }
        public string InteractionOrderTable { get; set; }
        public string InteractionOrderName { get; set; }
        public MedicationDto InteractionMedication { get; set; }

        public HateOasLinkDto InteractionOrderLink;
    }
}