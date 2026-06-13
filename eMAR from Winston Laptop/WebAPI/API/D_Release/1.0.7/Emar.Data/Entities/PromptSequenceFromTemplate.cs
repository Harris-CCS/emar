using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    public class PromptSequenceFromTemplate
    {
        public int promptId { get; set; }
        public long rowNum { get; set; }

    }
}