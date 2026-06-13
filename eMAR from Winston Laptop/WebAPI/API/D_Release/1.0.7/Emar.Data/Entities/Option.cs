using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("options")]
    public partial class Option
    {
        public Option()
        {
            SiteOptions = new HashSet<SiteOption>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(40)]
        public string Name { get; set; }
        [Required]
        [Column("description")]
        [StringLength(1000)]
        public string Description { get; set; }

        [InverseProperty("Option")]
        public virtual ICollection<SiteOption> SiteOptions { get; set; }
    }
}
