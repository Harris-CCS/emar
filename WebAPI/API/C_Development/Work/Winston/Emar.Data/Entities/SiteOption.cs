using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("site_options")]
    public partial class SiteOption
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("site_id")]
        public int SiteId { get; set; }
        [Column("option_id")]
        public int OptionId { get; set; }
        [Required]
        [Column("option_value")]
        [StringLength(255)]
        public string OptionValue { get; set; }

        [ForeignKey(nameof(OptionId))]
        [InverseProperty(nameof (Entities.Option.SiteOptions))]
        public virtual Option Option { get; set; }
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.SiteOptions))]
        public virtual Site Site { get; set; }
    }
}
