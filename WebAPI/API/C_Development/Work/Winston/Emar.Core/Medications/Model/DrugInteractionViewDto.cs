using System;
using System.Collections.Generic;
using Emar.Core.Users.Model;

namespace Emar.Core.Medications.Model
{
    public class DrugInteractionViewDto
    {
        public long Id { get; set; }
        public string InteractionDrug1 { get; set; }
        public string InteractionDrug2 { get; set; }
        public string Severity { get; set; }
        public long? OrderId1 { get; set; }
        public string OrderTable1 { get; set; }
        public string OrderName1 { get; set; }
        public long? OrderId2 { get; set; }
        public string OrderTable2 { get; set; }
        public string OrderName2 { get; set; }
        internal int? OverrideReasonId { get; set; }
        public OverrideReasonDto OverrideReason { get; set; }
        internal int? OverrideReasonUserId { get; set; }
        public UserDto OverrideReasonUser { get; set; }
        public DateTimeOffset? OverrideReasonDatetime { get; set; }
    }
}