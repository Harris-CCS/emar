using System;
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
            PromptChoices = new HashSet<PromptChoice>();
        }

        [Key]
        [Column("id")]
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

        [Column("prompt_type", TypeName = "varchar(20)"), Required]
        public string PromptType { get; set; }

        [Column("prompt_default", TypeName = "varchar(100)")]
        public string PromptDefault { get; set; }

        [Column("required")]
        public bool Required { get; set; }

        [ForeignKey(nameof(PromptGroupId))]
        [InverseProperty(nameof(Entities.PromptGroup.Prompts))]
        public virtual PromptGroup PromptGroup { get; set; }
        [InverseProperty("Prompt")]
        public virtual ICollection<PromptChoice> PromptChoices { get; set; }


    }
}
