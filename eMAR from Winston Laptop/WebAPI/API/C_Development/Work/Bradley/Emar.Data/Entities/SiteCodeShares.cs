using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("site_code_shares")]
    public partial class SiteCodeShares
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("source_site_id", TypeName = "int")]
        public int SourceSiteId { get; set; }
        [Column("target_site_id")]
        public int TargetSiteId { get; set; }
        [Column("entity")]
        [StringLength(50)]
        public string Entity { get; set; }

//        [ForeignKey(nameof(SourceSiteId))]
//        [InverseProperty(nameof(Entities.Site.Id))]
//        public virtual Site SiteSource { get; set; }

//        [ForeignKey(nameof(TargetSiteId))]
//        [InverseProperty(nameof(Entities.Site.Id))]
//        public virtual Site SiteTarget { get; set; }
    }
}