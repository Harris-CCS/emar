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

        [Column("ndc")]
        [StringLength(32)]
        public string Ndc { get; set; }

        [Column("drug_id")]
        [StringLength(32)]
        public string DrugId { get; set; }

        [Column("brand_name", TypeName = "nvarchar(255)"), Required]
        public string BrandName { get; set; }

        [Column("dose", TypeName = "decimal(11, 2)")]
        public decimal? Dose { get; set; }

        [Column("medication_unit_id")]
        public int? MedicationUnitId { get; set; }

        [Column("medication_route_id", TypeName = "int")]
        public int? MedicationRouteId { get; set; }

        [Column("frequency_schedule_id", TypeName = "int")]
        public int? FrequencyScheduleId { get; set; }

        [Column("order_notes", TypeName = "nvarchar(MAX)")]
        public string OrderNotes { get; set; }

        [Column("usages_this_week", TypeName = "int")]
        public int? UsagesThisWeek { get; set; }

        [Column("weekly_usage_rolling_average", TypeName = "decimal(9, 3)")]
        public decimal? WeeklyUsageRollingAverage { get; set; }

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
