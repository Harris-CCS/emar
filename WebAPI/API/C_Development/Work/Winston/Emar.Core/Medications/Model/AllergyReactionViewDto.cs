using System;
using Emar.Core.Users.Model;

namespace Emar.Core.Medications.Model
{
    public class AllergyReactionViewDto
    {
        public long Id { get; set; }
        public long PatientAllergyId { get; set; }
        public string PatientAllergyName { get; set; }
        public string OrderTable { get; set; }
        public long? OrderId { get; set; }
        public string OrderBrandName { get; set; }
        internal int? OverrideReasonId { get; set; }
        public OverrideReasonDto OverrideReason { get; set; }
        internal int? OverrideReasonUserId { get; set; }
        public UserDto OverrideReasonUser { get; set; }
        public DateTimeOffset? OverrideReasonDatetime { get; set; }
    }
}