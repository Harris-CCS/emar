using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("prompt_choices")]
    public partial class PromptChoice
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("prompt_id")]
        public int PromptId { get; set; }

        [Column("sequence")]
        public short Sequence { get; set; }

        [Column("choice_text", TypeName = "varchar(50)"), Required]
        public string ChoiceText { get; set; }

        [ForeignKey(nameof(PromptId))]
        [InverseProperty(nameof(Entities.Prompt.PromptChoices))]
        public virtual Prompt Prompt { get; set; }
    }
}
