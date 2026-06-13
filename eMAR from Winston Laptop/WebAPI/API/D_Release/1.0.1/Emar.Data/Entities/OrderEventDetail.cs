using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_event_details")]
    public class OrderEventDetail
    {
        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

        [Column("order_event_id", TypeName = "bigint")]
        public long OrderEventId { get; set; }

        [Column("prompt_id", TypeName = "int")]
        public int PromptId { get; set; }

        [Column("prompt_text", TypeName = "varchar(200)"), Required]
        public string PromptText { get; set; }

        [Column("entered_text", TypeName = "varchar(max)")]
        public string EnteredText { get; set; }

        [Column("chart_markup", TypeName = "nvarchar(256)")]
        public string ChartMarkup { get; set; }

        // For Foreign Key: fk__order_event_details__order_events
        [ForeignKey(nameof(OrderEventId))]
        [InverseProperty(nameof(Entities.OrderEvent.OrderEventDetails))]
        public virtual OrderEvent OrderEvent { get; set; }

        // For Foreign Key: fk__order_event_details__prompts
        [ForeignKey(nameof(PromptId))]
        [InverseProperty(nameof(Entities.Prompt.OrderEventDetails))]
        public virtual Prompt Prompt { get; set; }

    }
}