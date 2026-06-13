using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_administrations")]
    public class OrderAdministration
    {
        public OrderAdministration()
        {
            OrderEvents = new HashSet<OrderEvent>();
            OrderAdministrationNotifications = new HashSet<Notification>();

            //For Foreign Key: fk__pharmacy_notifications_administrations__order_administrations
            PharmacyNotificationAdministrations = new HashSet<PharmacyNotificationAdministration>();
        }

        [Key]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("patient_order_id", TypeName = "bigint"), Required]
        public long PatientOrderId { get; set; }

        [Column("point_in_time", TypeName = "bit"), Required]
        public bool PointInTime { get; set; }

        [Column("on_hold", TypeName = "bit"), Required]
        public bool OnHold { get; set; }

        [Column("missed_dose", TypeName = "bit"), Required]
        public bool MissedDose { get; set; }

        [Column("administration_scheduled_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset AdministrationScheduledDatetime { get; set; }

        [Column("administration_system_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? AdministrationSystemDatetime { get; set; }

        [Column("administering_user_id", TypeName = "int")]
        public int? AdministeringUserId { get; set; }

        [Column("administration_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? AdministrationDatetime { get; set; }

        [Column("stop_scheduled_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? StopScheduledDatetime { get; set; }

        [Column("stop_input_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? StopInputDatetime { get; set; }

        [Column("stop_user_id", TypeName = "int")]
        public int? StopUserId { get; set; }

        [Column("stop_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? StopDatetime { get; set; }

        [Column("acknowledge_user_id", TypeName = "int")]
        public int? AcknowledgeUserId { get; set; }

        [Column("acknowledge_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? AcknowledgeDatetime { get; set; }

        [ForeignKey(nameof(AcknowledgeUserId))]
        [InverseProperty(nameof(User.OrderAdministrationsAcknowledgeUser))]
        public virtual User AcknowledgeUser { get; set; }

        [ForeignKey(nameof(AdministeringUserId))]
        [InverseProperty(nameof(User.OrderAdministrationAdministeringUser))]
        public virtual User AdministeringUser { get; set; }

        [ForeignKey(nameof(PatientOrderId))]
        [InverseProperty(nameof(Entities.PatientOrder.OrderAdministrations))]
        public virtual PatientOrder PatientOrder { get; set; }

        [ForeignKey(nameof(StopUserId))]
        [InverseProperty(nameof(User.OrderAdministrationStopUser))]
        public virtual User StopUser { get; set; }

        [InverseProperty("OrderAdministration")]
        public virtual ICollection<OrderEvent> OrderEvents { get; set; }

        [InverseProperty("OrderAdministration")]
        public virtual ICollection<Notification> OrderAdministrationNotifications { get; set; }

        //For Foreign Key: fk__pharmacy_notifications_administrations__order_administrations
        [InverseProperty("OrderAdministration")]
        public virtual ICollection<PharmacyNotificationAdministration> PharmacyNotificationAdministrations { get; set; }
    }
}
