using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_orders")]
    public class PatientOrder
    {
        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

        [Column("patient_id", TypeName = "bigint"), Required]
        public long PatientId { get; set; }

        [Column("add_user_id", TypeName = "int"), Required]
        public int AddUserId { get; set; }

        [Column("add_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset AddDatetime { get; set; }

        [Column("ndc", TypeName = "varchar(32)")]
        public string Ndc { get; set; }

        [Column("drug_id", TypeName = "varchar(32)"), Required]
        public string DrugId { get; set; }

        [Column("brand_name", TypeName = "varchar(255)"), Required]
        public string BrandName { get; set; }

        [Column("dose", TypeName = "decimal(11,2)")]
        public decimal? Dose { get; set; }

        [Column("dose_unit", TypeName = "varchar(20)")]
        public string DoseUnit { get; set; }

        [Column("medication_route_id", TypeName = "int")]
        public int MedicationRouteId { get; set; }

        [Column("priority", TypeName = "tinyint"), Required]
        public short Priority { get; set; }

        [Column("frequency_id", TypeName = "int")]
        public int FrequencyId { get; set; }

        [Column("prn", TypeName = "bit"), Required]
        public bool Prn { get; set; }

        [Column("point_in_time", TypeName = "bit"), Required]
        public bool PointInTime { get; set; }

        [Column("order_status", TypeName = "varchar(10)"), Required]
        public string OrderStatus { get; set; }

        [Column("begin_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset BeginDateTime { get; set; }

        [Column("end_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? EndDateTime { get; set; }

        [Column("order_notes", TypeName = "nvarchar(MAX)")]
        public string OrderNotes { get; set; }

        [NotMapped]
        public string OrderStatusCode { get; set; } = "Pending";

        [NotMapped]
        public string OrderType { get; set; }

        [NotMapped]
        public IEnumerable<OrderAdministration>? Administrations { get; set; }

        [NotMapped]
        public IEnumerable<OrderEvent>? Events { get; set; }
    }
}
