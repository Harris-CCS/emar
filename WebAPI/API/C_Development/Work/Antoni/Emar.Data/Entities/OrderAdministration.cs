using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_administrations")]
    public class OrderAdministration
    {
        [Column("id", TypeName = "bigint"), Key]
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

        [Column("administration_input_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? AdministrationInputDatetime { get; set; }

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

        [NotMapped]
        public User? AcknowledgeUser { get; set; }

        [NotMapped]
        public User? AdministeringUser { get; set; }

        [NotMapped]
        public User? StopUser { get; set; }

        [NotMapped]
        public IEnumerable<OrderEvent>? OrderEvents { get; set; }
    }
}
