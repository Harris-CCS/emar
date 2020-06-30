using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_orders")]
    public class Order
    {
        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

        [Column("patient_id", TypeName = "bigint"), Required]
        public long PatientId { get; set; }

        [Column("create_stamp", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset CreatedDateTime { get; set; }

        [Column("medication_id", TypeName = "varchar(50)"), Required]
        public string MedicationId { get; set; }

        [NotMapped]
        public string OrderType { get; set; }

        [Column("priority", TypeName = "tinyint"), Required]
        public short Priority { get; set; }

        [Column("prn", TypeName = "bit"), Required]
        public bool Prn { get; set; }

        [Column("point_in_time", TypeName = "bit"), Required]
        public bool PointInTime { get; set; }

        [Column("order_status", TypeName = "varchar(10)"), Required]
        public string OrderStatus { get; set; }

        [NotMapped]
        public string OrderStatusCode { get; set; } = "Pending";

        [Column("begin_stamp", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset BeginDateTime { get; set; }

        [Column("end_stamp", TypeName = "datetimeoffset")]
        public DateTimeOffset? EndDateTime { get; set; }

        [Column("frequency_id", TypeName = "int"), Required]
        public int FrequencyId { get; set; }

        [Column("medication_route_id", TypeName = "int"), Required]
        public int MedicationRouteId { get; set; }

        [Column("order_notes", TypeName = "ntext")]
        public string OrderNotes { get; set; }

        [NotMapped]
        public string Name { get; set; }

        [NotMapped]
        public string Unit { get; set; }

        [NotMapped]
        public string Dose { get; set; }

        [NotMapped]
        public int OrderingProviderId { get; set; }

        [NotMapped]
        public IEnumerable<OrderAdministration>? Administrations { get; set; }

        [NotMapped]
        public IEnumerable<OrderEvent>? Events { get; set; }
    }
}
