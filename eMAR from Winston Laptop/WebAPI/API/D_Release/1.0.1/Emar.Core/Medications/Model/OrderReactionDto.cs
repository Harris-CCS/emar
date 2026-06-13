using System;
using Emar.Core.Users.Model;

namespace Emar.Core.Medications.Model
{
    public class OrderReactionDto
    {
        public long Id { get; set; }
        public long PatientAllergyId { get; set; }
        public long? PatientOrderId { get; set; }
        public long? PatientCartOrderId { get; set; }
        internal int? OverrideReasonId { get; set; }
        public virtual OverrideReasonDto OverrideReason { get; set; }
        internal int? OverrideReasonUserId { get; set; }
        public virtual UserDto OverrideReasonUser { get; set; }
        public DateTimeOffset? OverrideReasonDatetime { get; set; }
    }
}