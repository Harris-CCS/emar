using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_events")]
    public class OrderEvent
    {
        public OrderEvent()
        {
            // For Foreign Key: fk__order_event_details__order_events
            OrderEventDetails = new HashSet<OrderEventDetail>();
        }

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
        public DateTimeOffset AddDatetime { get; set; }

        [Column("action_id", TypeName = "int"), Required]
        public int ActionId { get; set; }

        [Column("template_id", TypeName = "int")]
        public int? TemplateId { get; set; }

        
        // For Foreign Key: fk__order_events__actions
        [ForeignKey(nameof(ActionId))]
        [InverseProperty(nameof(Entities.Action.OrderEvents))]
        public virtual Action Action { get; set; }

        // For Foreign Key: fk__order_events__users
        [ForeignKey(nameof(AddUserId))]
        [InverseProperty(nameof(Entities.User.OrderEvents))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(OrderAdministrationId))]
        [InverseProperty(nameof(Entities.OrderAdministration.OrderEvents))]
        public virtual OrderAdministration OrderAdministration { get; set; }

        [ForeignKey(nameof(PatientOrderId))]
        [InverseProperty(nameof(Entities.PatientOrder.OrderEvents))]
        public virtual PatientOrder PatientOrder { get; set; }

        // For Foreign Key: fk__order_events__templates
        [ForeignKey(nameof(TemplateId))]
        [InverseProperty(nameof(Entities.Template.OrderEvents))]
        public virtual Template Template { get; set; }

        // For Foreign Key: fk__order_event_details__order_events
        [InverseProperty("OrderEvent")]
        public virtual ICollection<OrderEventDetail> OrderEventDetails { get; set; }
    }
}