using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    public partial class DrugInteractionView
    {
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
        [Column("order_id_1")]
        public long? OrderId1 { get; set; }
        [Column("order_table_1")]
        [StringLength(18)]
        public string OrderTable1 { get; set; }
        [Column("order_name_1")]
        [StringLength(255)]
        public string OrderName1 { get; set; }
        [Column("order_id_2")]
        public long? OrderId2 { get; set; }
        [Column("order_table_2")]
        [StringLength(23)]
        public string OrderTable2 { get; set; }
        [Column("order_name_2")]
        [StringLength(255)]
        public string OrderName2 { get; set; }

        [ForeignKey(nameof(OverrideReasonId))]
        [InverseProperty(nameof(Entities.OverrideReason.DrugInteractionsView))]
        public virtual OverrideReason OverrideReason { get; set; }
        [ForeignKey(nameof(OverrideReasonUserId))]
        [InverseProperty(nameof(User.DrugInteractionsView))]
        public virtual User OverrideReasonUser { get; set; }

        [InverseProperty("DrugInteractionView")]
        public virtual ICollection<OrderInteraction> OrderInteractions { get; set; }
    }
}