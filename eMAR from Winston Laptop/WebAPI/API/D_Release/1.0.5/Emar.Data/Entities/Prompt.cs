using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("prompts")]
    public partial class Prompt
    {
        public Prompt()
        {
            // For Foreign Key: fk__order_event_details__prompts
            OrderEventDetails = new HashSet<OrderEventDetail>();
            PromptChoices = new HashSet<PromptChoice>();
            // For Foreign Key: fk__templates__prompts
            Templates = new HashSet<Template>();
        }

        [Column("id"), Key]
        public int Id { get; set; }

        [Column("prompt_group_id")]
        public int PromptGroupId { get; set; }

        [Column("sequence")]
        public short Sequence { get; set; }
        [Required]

        [Column("prompt")]
        [StringLength(200)]
        public string PromptText { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("prompt_type", TypeName = "varchar(25)"), Required]
        public string PromptType { get; set; }

        [Column("prompt_default", TypeName = "varchar(100)")]
        public string PromptDefault { get; set; }

        [Column("required")]
        public bool Required { get; set; }

        [Column("is_on_newline", TypeName = "bit")]
        public bool IsOnNewline { get; set; }

        [Column("placeholder_text", TypeName = "nvarchar(100)")]
        public string PlaceholderText { get; set; }

        [Column("display_child_prompts_value", TypeName = "nvarchar(100)")]
        public string DisplayChildPromptsValue { get; set; }

        [Column("chart_markup", TypeName = "nvarchar(256)")]
        public string ChartMarkup { get; set; }

        [ForeignKey(nameof(PromptGroupId))]
        [InverseProperty(nameof(Entities.PromptGroup.Prompts))]
        public virtual PromptGroup PromptGroup { get; set; }

        // For Foreign Key: fk__order_event_details__prompts
        [InverseProperty("Prompt")]
        public virtual ICollection<OrderEventDetail> OrderEventDetails { get; set; }

        [InverseProperty("Prompt")]
        public virtual ICollection<PromptChoice> PromptChoices { get; set; }

        // For Foreign Key: fk__templates__prompts
        [InverseProperty("Prompt")]
        public virtual ICollection<Template> Templates { get; set; }
    }
}