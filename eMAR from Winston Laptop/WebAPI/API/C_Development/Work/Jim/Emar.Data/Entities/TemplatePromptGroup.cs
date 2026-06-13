using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("template_prompt_groups")]
    public partial class TemplatePromptGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("template_id")]
        public int TemplateId { get; set; }
        
        [Column("sequence")]
        public byte Sequence { get; set; }
        
        [Column("prompt_group_id")]
        public int PromptGroupId { get; set; }
        
        [Column("required")]
        public bool Required { get; set; }

        [ForeignKey(nameof(PromptGroupId))]
        [InverseProperty(nameof(Entities.PromptGroup.TemplatePromptGroups))]
        public virtual PromptGroup PromptGroup { get; set; }

        [ForeignKey(nameof(TemplateId))]
        [InverseProperty(nameof(Entities.Template.TemplatePromptGroups))]
        public virtual Template Template { get; set; }
    }
}
