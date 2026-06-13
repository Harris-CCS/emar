using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("antimicrobial_indication_items")]
    public class AntimicrobialIndicationItem
    {

        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        [Column("sub_category", TypeName = "int")]
        public int SubCategory { get; set; }

        // For Foreign Key: fk__antimicrobial_indication_items__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.AntimicrobialIndicationItems))]
        public virtual Site Site { get; set; }
    }
}
