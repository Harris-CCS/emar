using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("pharmacy_notifications")]

    public class PharmacyNotification
    {
        public PharmacyNotification()
        {
            //For Foreign Key: fk__pharmacy_notifications_orders__inpatient_notifications
            PharmacyNotificationOrders = new HashSet<PharmacyNotificationOrder>();

            //For Foreign Key: fk__pharmacy_notifications_administrations__order_administrations
            PharmacyNotificationAdministrations = new HashSet<PharmacyNotificationAdministration>();
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("patient_id")]
        public long PatientId { get; set; }

        [Column("type")]
        [StringLength(20)]
        public string Type { get; set; }

        [Column("entered_datetime")]
        public DateTimeOffset? EnteredDatetime { get; set; }

        [Column("completed_datetime")]
        public DateTimeOffset? CompletedDatetime { get; set; }

        //For Foreign Key: fk__inpatient_notifications__patients
        [ForeignKey(nameof(PatientId))]
        [InverseProperty(nameof(Entities.Patient.PharmacyNotifications))]
        public virtual Patient Patient { get; set; }

        //For Foreign Key: fk__inpatient_notifications_orders__inpatient_notifications
        [InverseProperty("PharmacyNotification")]
        public virtual ICollection<PharmacyNotificationOrder> PharmacyNotificationOrders { get; set; }

        //For Foreign Key: fk__inpatient_notifications_administrations__order_administrations
        [InverseProperty("PharmacyNotification")]
        public virtual ICollection<PharmacyNotificationAdministration> PharmacyNotificationAdministrations { get; set; }
    }
}
