using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    public partial class AllergyReactionView
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("patient_allergy_id")]
        public long PatientAllergyId { get; set; }
        [Column("patient_allergy_name")]
        [StringLength(70)]
        public string PatientAllergyName { get; set; }
        [Column("order_table")]
        [StringLength(18)]
        public string OrderTable { get; set; }
        [Column("order_id")]
        public long? OrderId { get; set; }
        [Column("order_brand_name")]
        [StringLength(255)]
        public string OrderBrandName { get; set; }
        [Column("override_reason_id")]
        public int? OverrideReasonId { get; set; }
        [Column("override_reason_user_id")]
        public int? OverrideReasonUserId { get; set; }
        [Column("override_reason_datetime")]
        public DateTimeOffset? OverrideReasonDatetime { get; set; }

        [ForeignKey(nameof(OverrideReasonId))]
        [InverseProperty(nameof(Entities.OverrideReason.AllergyReactionsView))]
        public virtual OverrideReason OverrideReason { get; set; }
        [ForeignKey(nameof(OverrideReasonUserId))]
        [InverseProperty(nameof(User.AllergyReactionsView))]
        public virtual User OverrideReasonUser { get; set; }

        [InverseProperty("AllergyReactionsView")]
        public virtual PatientCartOrder PatientCartOrder { get; set; }
        [InverseProperty("AllergyReactionsView")]
        public virtual PatientOrder PatientOrder { get; set; }

        //[InverseProperty("AllergyReactionsView")]
        //public virtual ICollection<OrderReaction> OrderReactions { get; set; }
    }
}