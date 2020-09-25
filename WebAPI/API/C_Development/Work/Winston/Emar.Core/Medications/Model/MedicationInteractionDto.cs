using System;
using System.Collections.Generic;
using Emar.Core.Users.Model;

namespace Emar.Core.Medications.Model
{
    public class MedicationInteractionDto
    {
        public long Id { get; set; }
        public string InteractionDrug1 { get; set; }
        public string InteractionDrug2 { get; set; }
        public string InteractionDrugName2 { get; set; }
        public string Severity { get; set; }
        public int? OverrideReasonId { get; set; }
        public int? OverrideReasonUserId { get; set; }
        public DateTimeOffset? OverrideReasonDatetime { get; set; }

        public virtual OverrideReasonDto OverrideReason { get; set; }
        public virtual UserDto OverrideReasonUser { get; set; }
        public virtual ICollection<OrderInteractionDto> OrderInteractions { get; set; }
    }
}