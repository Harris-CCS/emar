using System;

namespace Emar.Core.Orders.Model
{
    public class FrequencyScheduleAdministrationDto
    {
        public DateTimeOffset ScheduleDateTime { get; set; }
        public DateTimeOffset? StopDateTime { get; set; }
        public bool PointInTime { get; set; }
    }
}