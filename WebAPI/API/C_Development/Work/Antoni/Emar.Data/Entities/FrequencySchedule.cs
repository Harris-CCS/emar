using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("frequency_schedules")]
    public class FrequencySchedule
    {
        public FrequencySchedule()
        {
            DepartmentPreferredListItems = new HashSet<DepartmentPreferredListItem>();
            GroupListItems = new HashSet<GroupListItem>();
            PatientCartOrders = new HashSet<PatientCartOrder>();
            PatientOrders = new HashSet<PatientOrder>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        [Column("name", TypeName = "nvarchar(128)"), Required]
        public string Name { get; set; }
        
        [Column("is_active", TypeName = "bit")]
        public bool IsActive { get; set; }

        [Column("point_in_time", TypeName = "bit")]
        public bool PointInTime { get; set; }

        [Column("frequency_type_id", TypeName = "int")]
        public int FrequencyTypeId { get; set; }

        [Column("frequency_type_recurring", TypeName = "int")]
        public int FrequencyTypeRecurring { get; set; }

        [Column("frequency_interval", TypeName = "int")]
        public int FrequencyInterval { get; set; }

        [Column("frequency_interval_unit_id", TypeName = "int")]
        public int FrequencyIntervalUnitId { get; set; }

        [Column("interval_start_time", TypeName = "time")]
        public TimeSpan IntervalStartTime { get; set; }

        [Column("interval_end_minutes", TypeName = "smallint")]
        public short IntervalEndMinutes { get; set; }

        [Column("notes", TypeName = "nvarchar(1000)")]
        public string Notes { get; set; }


        [InverseProperty("FrequencySchedule")]
        public virtual ICollection<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }

        [InverseProperty("FrequencySchedule")]
        public virtual ICollection<GroupListItem> GroupListItems { get; set; }

        [InverseProperty("FrequencySchedule")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        [InverseProperty("FrequencySchedule")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

        [InverseProperty("FrequencySchedule")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }
    }
}
