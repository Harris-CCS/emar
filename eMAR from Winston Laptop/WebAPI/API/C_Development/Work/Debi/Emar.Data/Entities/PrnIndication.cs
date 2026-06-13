using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("prn_indications")]
    public class PrnIndication
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("site_id")]
        public int SiteId { get; set; }

        [StringLength(255)]
        [Column("option_description")]
        public string OptionDescription { get; set; }

        //For Foreign Key: fk__prn_indications__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.PrnIndications))]
        public virtual Site Site { get; set; }
    }
}
