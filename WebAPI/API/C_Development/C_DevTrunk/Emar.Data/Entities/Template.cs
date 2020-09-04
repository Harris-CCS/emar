using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("templates")]
    public partial class Template
    {
        public Template()
        {
            TemplatePromptGroups = new HashSet<TemplatePromptGroup>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(20)]
        public string Name { get; set; }
        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; }
        [Required]
        [Column("title")]
        [StringLength(50)]
        public string Title { get; set; }
        [Column("site_id")]
        public int SiteId { get; set; }

        [InverseProperty("Template")]
        public virtual ICollection<TemplatePromptGroup> TemplatePromptGroups { get; set; }
    }
}
