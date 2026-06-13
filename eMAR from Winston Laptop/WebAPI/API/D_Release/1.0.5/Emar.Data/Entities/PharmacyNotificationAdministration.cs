using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("pharmacy_notification_administrations")]
    public class PharmacyNotificationAdministration
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("pharmacy_notification_id")]
        public long PharmacyNotificationId { get; set; }

        [Column("order_administration_id")]
        public long OrderAdminisrtrationId { get; set; }

        //For Foreign Key: fk__pharmacy_notifications_administrations__inpatient_notifications
        [ForeignKey(nameof(PharmacyNotificationId))]
        [InverseProperty(nameof(Entities.PharmacyNotification.PharmacyNotificationAdministrations))]
        public virtual PharmacyNotification PharmacyNotification { get; set; }

        //For Foreign Key: fk__pharmacy_notifications_administrations__order_administrations
        [ForeignKey(nameof(OrderAdminisrtrationId))]
        [InverseProperty(nameof(Entities.OrderAdministration.PharmacyNotificationAdministrations))]
        public virtual OrderAdministration OrderAdministration { get; set; }
    }
}
