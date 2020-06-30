using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_order_administrations")]
    public class OrderAdministration
    {
        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

        [Column("patient_order_id", TypeName = "bigint"), Required]
        public long OrderId { get; set; }

        [Column("point_in_time", TypeName = "bit"), Required]
        public bool PointInTime { get; set; }

        [Column("on_hold", TypeName = "bit"), Required]
        public bool OnHold { get; set; }

        [Column("missed_dose", TypeName = "bit"), Required]
        public bool MissedDose { get; set; }

        [Column("scheduled_administration_time", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset ScheduledAdministrationTime { get; set; }

        [Column("actual_administration_time", TypeName = "datetimeoffset")]
        public DateTimeOffset? ActualAdministrationTime { get; set; }

        [Column("system_administration_time", TypeName = "datetimeoffset")]
        public DateTimeOffset? SystemAdministrationTime { get; set; }

        //[Column("user_administering_id", TypeName = "int")]
        [NotMapped]
        public int? AdministrationUserId { get; set; }

        [Column("scheduled_stop_time", TypeName = "datetimeoffset")]
        public DateTimeOffset? ScheduledStopTime { get; set; }

        [Column("actual_stop_time", TypeName = "datetimeoffset")]
        public DateTimeOffset? ActualStopTime { get; set; }

        [Column("system_stop_time", TypeName = "datetimeoffset")]
        public DateTimeOffset? SystemStopTime { get; set; }

        [Column("user_stopping_id", TypeName = "int")]
        public int? StopUserId { get; set; }

        [Column("acknowledge_user_id", TypeName = "int")]
        public int? AcknowledgeUserId { get; set; }

        [Column("acknowledge_time", TypeName = "datetimeoffset")]
        public DateTimeOffset? AcknowledgeTime { get; set; }

        [NotMapped]
        public IEnumerable<OrderEvent>? Events { get; set; }
    }
}
