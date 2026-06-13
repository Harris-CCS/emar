using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("user_quick_list_items")]
    public class UserQuickListItem
    {
        public UserQuickListItem()
        {
            PatientCartOrders = new HashSet<PatientCartOrder>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public int SiteId { get; set; }

        [Column("user_id", TypeName = "int"), Required]
        public int UserId { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        [Column("duration_in_minutes", TypeName = "int")]
        public int DurationInMinutes { get; set; }
        
        [Column("dose", TypeName = "decimal(11, 2)")]
        public decimal? Dose { get; set; }

        [Column("medication_unit_id")]
        public int? MedicationUnitId { get; set; }

        [Column("medication_route_id", TypeName = "int")]
        public int? MedicationRouteId { get; set; }

        [Column("frequency_schedule_id", TypeName = "int")]
        public int? FrequencyScheduleId { get; set; }

        [Column("duration", TypeName = "int")]
        public int? Duration { get; set; }

        [Column("duration_unit_id", TypeName = "int")]
        public int? DurationUnitId { get; set; }

        [Column("priority", TypeName = "tinyint")]
        public byte? Priority { get; set; }

        [Column("order_notes", TypeName = "nvarchar(MAX)")]
        public string OrderNotes { get; set; }

        [Column("usages_this_week", TypeName = "int")]
        public int? UsagesThisWeek { get; set; }

        [Column("weekly_usage_rolling_average", TypeName = "decimal(9, 3)")]
        public decimal WeeklyUsageRollingAverage { get; set; }

        [Column("ndc")]
        public string? Ndc { get; set; }

        [Column("prn_indication")]
        public string? PrnIndication { get; set; }

        // For Foreign Key: fk__user_quick_list_items__duration_units
        [ForeignKey(nameof(DurationUnitId))]
        [InverseProperty(nameof(Entities.DurationUnit.UserQuickListItems))]
        public virtual DurationUnit DurationUnit { get; set; }

        [ForeignKey(nameof(FrequencyScheduleId))]
        [InverseProperty(nameof(Entities.FrequencySchedule.UserQuickListItems))]
        public virtual FrequencySchedule FrequencySchedule { get; set; }

        // For Foreign Key: fk__user_quick_list_items__medications
        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.UserQuickListItems))]
        public virtual Medication Medication { get; set; }

        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.UserQuickListItems))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        [ForeignKey(nameof(MedicationUnitId))]
        [InverseProperty(nameof(Entities.MedicationUnit.UserQuickListItems))]
        public virtual MedicationUnit MedicationUnit { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.UserQuickListItems))]
        public virtual Site Site { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Entities.User.UserQuickListItems))]
        public virtual User User { get; set; }

        [InverseProperty("UserQuickListItem")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }
    }
}