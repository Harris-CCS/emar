using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_order_events")]
    public class OrderEvent
    {
        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

        [Column("patient_order_id", TypeName = "bigint"), Required]
        public long OrderId { get; set; }

        [Column("patient_order_administration_id", TypeName = "bigint")]
        public long? AdministrationId { get; set; }

        [Column("event_time", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset EventDateTime { get; set; }

        [Column("system_time", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset SystemDateTime { get; set; }

        [Column("user_id", TypeName = "int"), Required]
        public int UserId { get; set; }

        [Column("action_id", TypeName = "int"), Required]
        public int ActionId { get; set; }
    }
}
