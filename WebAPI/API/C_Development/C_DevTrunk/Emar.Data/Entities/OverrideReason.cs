using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("override_reasons")]
    public partial class OverrideReason
    {
        public OverrideReason()
        {
            MedicationInteractions = new HashSet<MedicationInteraction>();
            OrderReactions = new HashSet<OrderReaction>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("site_id")]
        public int SiteId { get; set; }
        [Column("is_medication")]
        public bool IsMedication { get; set; }
        [Required]
        [Column("description")]
        [StringLength(80)]
        public string Description { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.OverrideReasons))]
        public virtual Site Site { get; set; }
        [InverseProperty("OverrideReason")]
        public virtual ICollection<MedicationInteraction> MedicationInteractions { get; set; }
        [InverseProperty("OverrideReason")]
        public virtual ICollection<OrderReaction> OrderReactions { get; set; }


        [InverseProperty("OverrideReason")]
        public virtual ICollection<AllergyReactionView> AllergyReactionsView { get; set; }
        [InverseProperty("OverrideReason")]
        public virtual ICollection<DrugInteractionView> DrugInteractionsView { get; set; }
    }
}
