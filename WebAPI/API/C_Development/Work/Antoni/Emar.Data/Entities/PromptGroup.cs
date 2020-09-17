using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("prompt_groups")]
    public partial class PromptGroup
    {
        public PromptGroup()
        {
            Prompts = new HashSet<Prompt>();
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
        [Column("title")]
        [StringLength(50)]
        public string Title { get; set; }


        [InverseProperty("PromptGroup")]
        public virtual ICollection<Prompt> Prompts { get; set; }
        
        [InverseProperty("PromptGroup")]
        public virtual ICollection<TemplatePromptGroup> TemplatePromptGroups { get; set; }
    }
}
