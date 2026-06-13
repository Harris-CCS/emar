using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_instructions")]
    public class OrderInstruction
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        [Column("description", TypeName = "nvarchar(255)"), Required]
        public string Description { get; set; }

        [Column("is_active", TypeName = "bit")]
        public bool IsActive { get; set; }

        // For Foreign Key: fk__order_instructions__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.OrderInstructions))]
        public virtual Site Site { get; set; }
    }
}