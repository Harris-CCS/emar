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

        [Column("name", TypeName = "varchar(20)"), Required]
        public string Name { get; set; }

        [Column("title", TypeName = "varchar(50)"), Required]
        public string Title { get; set; }


        [InverseProperty("PromptGroup")]
        public virtual ICollection<Prompt> Prompts { get; set; }
        
        [InverseProperty("PromptGroup")]
        public virtual ICollection<TemplatePromptGroup> TemplatePromptGroups { get; set; }
    }
}
