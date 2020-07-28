using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_events")]
    public class OrderEvent
    {
        [Key]
        [Column("id", TypeName = "bigint")]
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
        public DateTimeOffset SystemDateTime { get; set; }

        [Column("action_id", TypeName = "int"), Required]
        public int ActionId { get; set; }

        [ForeignKey(nameof(ActionId))]
        [InverseProperty(nameof(Entities.Action.OrderEvents))]
        public virtual Action Action { get; set; }

        [ForeignKey(nameof(OrderAdministrationId))]
        [InverseProperty(nameof(Entities.OrderAdministration.OrderEvents))]
        public virtual OrderAdministration OrderAdministration { get; set; }

        [ForeignKey(nameof(PatientOrderId))]
        [InverseProperty(nameof(Entities.PatientOrder.OrderEvents))]
        public virtual PatientOrder PatientOrder { get; set; }
    }
}
