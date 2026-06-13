using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("pharmacy_notification_orders")]
    public class PharmacyNotificationOrder
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("pharmacy_notification_id")]
        public long PharmacyNotificationId { get; set; }

        [Column("patient_order_id")]
        public long PatientOrderId { get; set; }

        //For Foreign Key: fk__pharmacy_notifications_orders__inpatient_notifications
        [ForeignKey(nameof(PharmacyNotificationId))]
        [InverseProperty(nameof(Entities.PharmacyNotification.PharmacyNotificationOrders))]
        public virtual PharmacyNotification PharmacyNotification { get; set; }

        //For Foreign Key: fk__pharmacy_notifications_orders__patient_orders
        [ForeignKey(nameof(PatientOrderId))]
        [InverseProperty(nameof(Entities.PatientOrder.PharmacyNotificationOrders))]
        public virtual PatientOrder PatientOrder { get; set; }

    }
}
