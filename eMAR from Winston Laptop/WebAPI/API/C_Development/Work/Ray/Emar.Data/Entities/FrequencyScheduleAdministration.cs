using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [NotMapped]
    public class FrequencyScheduleAdministration
    {
        [Column("point_in_time")]
        public bool PointInTime { get; set; }

        [Column("sched_datetime_tz", TypeName = "datetimeoffset")]
        public DateTimeOffset ScheduleDateTime { get; set; }

        [Column("stop_datetime_tz", TypeName = "datetimeoffset")]
        public DateTimeOffset? StopDateTime { get; set; }
    }
}
