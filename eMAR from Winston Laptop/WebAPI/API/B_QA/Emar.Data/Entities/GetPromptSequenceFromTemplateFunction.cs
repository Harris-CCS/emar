using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [NotMapped]
    public class GetPromptSequenceFromTemplateFunction
    {
        [Column("prompt_id")]
        public int prompt_id { get; set; }
        [Column("row_num")]
        public long row_num { get; set; }
    }
}