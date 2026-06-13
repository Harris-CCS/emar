using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_reactions")]
    public partial class OrderReaction
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("patient_allergy_id")]
        public long PatientAllergyId { get; set; }
        [Column("patient_order_id")]
        public long? PatientOrderId { get; set; }
        [Column("patient_cart_order_id")]
        public long? PatientCartOrderId { get; set; }
        [Column("override_reason_id")]
        public int? OverrideReasonId { get; set; }
        [Column("override_reason_user_id")]
        public int? OverrideReasonUserId { get; set; }
        [Column("override_reason_datetime")]
        public DateTimeOffset? OverrideReasonDatetime { get; set; }

        [ForeignKey(nameof(OverrideReasonId))]
        [InverseProperty(nameof(Entities.OverrideReason.OrderReactions))]
        public virtual OverrideReason OverrideReason { get; set; }
        [ForeignKey(nameof(OverrideReasonUserId))]
        [InverseProperty(nameof(User.OrderReactions))]
        public virtual User OverrideReasonUser { get; set; }
        [ForeignKey(nameof(PatientAllergyId))]
        [InverseProperty(nameof(Entities.PatientAllergy.OrderReactions))]
        public virtual PatientAllergy PatientAllergy { get; set; }
        [ForeignKey(nameof(PatientCartOrderId))]
        [InverseProperty(nameof(Entities.PatientCartOrder.OrderReactions))]
        public virtual PatientCartOrder PatientCartOrder { get; set; }
        [ForeignKey(nameof(PatientOrderId))]
        [InverseProperty(nameof(Entities.PatientOrder.OrderReactions))]
        public virtual PatientOrder PatientOrder { get; set; }
    }
}