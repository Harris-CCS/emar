using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("medication_interactions")]
    public partial class MedicationInteraction
    {
        public MedicationInteraction()
        {
            OrderInteractions = new HashSet<OrderInteraction>();
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Required]
        [Column("interaction_drug_1")]
        [StringLength(50)]
        public string InteractionDrug1 { get; set; }
        [Required]
        [Column("interaction_drug_2")]
        [StringLength(50)]
        public string InteractionDrug2 { get; set; }
        [Column("severity")]
        public byte Severity { get; set; }
        [Column("override_reason_id")]
        public int? OverrideReasonId { get; set; }
        [Column("override_reason_user_id")]
        public int? OverrideReasonUserId { get; set; }
        [Column("override_reason_datetime")]
        public DateTimeOffset? OverrideReasonDatetime { get; set; }

        [ForeignKey(nameof(OverrideReasonId))]
        [InverseProperty(nameof(Entities.OverrideReason.MedicationInteractions))]
        public virtual OverrideReason OverrideReason { get; set; }
        [ForeignKey(nameof(OverrideReasonUserId))]
        [InverseProperty(nameof(User.MedicationInteractions))]
        public virtual User OverrideReasonUser { get; set; }
        [InverseProperty("MedicationInteraction")]
        public virtual ICollection<OrderInteraction> OrderInteractions { get; set; }

        [NotMapped]
        public string InteractionDrugName2 { get; set; }
        [NotMapped]
        public long? InteractionOrderId { get; set; }
        [NotMapped]
        public string InteractionOrderTable { get; set; }
        [NotMapped]
        public string InteractionOrderName { get; set; }
        [NotMapped]
        public Medication InteractionMedication { get; set; }
    }
}