using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("templates")]
    public class Template
    {
        public Template()
        {
            // For Foreign Key: fk__action_route_templates__templates
            ActionRouteTemplates = new HashSet<ActionRouteTemplate>();
            // For Foreign Key: fk__order_events__templates
            OrderEvents = new HashSet<OrderEvent>();
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
        
        [Column("save_button_text", TypeName = "nvarchar(25)")]
        public string SaveButtonText { get; set; }

        [Column("cancel_button_text", TypeName = "nvarchar(25)")]
        public string CancelButtonText { get; set; }

        [Column("event_datetime_prompt_id", TypeName = "int")]
        public int? EventDatetimePromptId { get; set; }


        // For Foreign Key: fk__templates__prompts
        [ForeignKey(nameof(EventDatetimePromptId))]
        [InverseProperty(nameof(Entities.Prompt.Templates))]
        public virtual Prompt Prompt { get; set; }

        // For Foreign Key: fk__action_route_templates__templates
        [InverseProperty("Template")]
        public virtual ICollection<ActionRouteTemplate> ActionRouteTemplates { get; set; }

        // For Foreign Key: fk__order_events__templates
        [InverseProperty("Template")]
        public virtual ICollection<OrderEvent> OrderEvents { get; set; }

        [InverseProperty("Template")]
        public virtual ICollection<TemplatePromptGroup> TemplatePromptGroups { get; set; }
    }
}