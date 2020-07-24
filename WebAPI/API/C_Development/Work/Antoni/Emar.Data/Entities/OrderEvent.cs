using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_events")]
    public class OrderEvent
    {
        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

        [Column("patient_order_id", TypeName = "bigint"), Required]
        public long PatientOrderId { get; set; }

        [Column("order_administration_id", TypeName = "bigint")]
        public long? OrderAdministrationId { get; set; }

        [Column("event_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset EventDateTime { get; set; }

        [Column("add_user_id", TypeName = "int"), Required]
        public int AddUserId { get; set; }

        [Column("add_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset AddDatetime { get; set; }

        [Column("action_id", TypeName = "int"), Required]
        public int ActionId { get; set; }

        [NotMapped]
        public Action Action { get; set; }

        [NotMapped]
        public OrderAdministration OrderAdministration { get; set; }

        [NotMapped]
        public PatientOrder PatientOrder { get; set; }
    }
}
