using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("cart_order_administrations")]
    public class CartOrderAdministration
    {
        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

        [Column("patient_cart_order_id", TypeName = "bigint"), Required]
        public long PatientCartOrderId { get; set; }

        [Column("point_in_time", TypeName = "bit"), Required]
        public bool PointInTime { get; set; }

        [Column("administration_scheduled_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset AdministrationScheduledDatetime { get; set; }

        [Column("stop_scheduled_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? StopScheduledDatetime { get; set; }

        // For Foreign Key: fk__cart_order_administrations__patient_cart_orders
        [ForeignKey(nameof(PatientCartOrderId))]
        [InverseProperty(nameof(Entities.PatientCartOrder.CartOrderAdministrations))]
        public virtual PatientCartOrder PatientCartOrder { get; set; }
    }
}
