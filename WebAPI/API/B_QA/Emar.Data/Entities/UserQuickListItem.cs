using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("user_quick_list_items")]
    public class UserQuickListItem
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public int SiteId { get; set; }

        [Column("user_id", TypeName = "int"), Required]
        public int UserId { get; set; }

        [Column("ndc", TypeName = "varchar(32)")]
        public string Ndc { get; set; }

        [Column("drug_id", TypeName = "varchar(32)")]
        public string DrugId { get; set; }

        [Column("brand_name", TypeName = "nvarchar(255)"), Required]
        public string BrandName { get; set; }

        [Column("dose", TypeName = "decimal(11, 2)")]
        public decimal? Dose { get; set; }

        [Column("dose_unit", TypeName = "varchar(20)")]
        public string DoseUnit { get; set; }

        [Column("medication_route_id", TypeName = "int")]
        public int? MedicationRouteId { get; set; }

        [Column("frequency_id", TypeName = "int")]
        public int? FrequencyId { get; set; }

        [Column("order_notes", TypeName = "nvarchar(MAX)")]
        public string OrderNotes { get; set; }

        [Column("usages_this_week", TypeName = "int")]
        public int? UsagesThisWeek { get; set; }

        [Column("weekly_usage_rolling_average", TypeName = "decimal(9, 3)")]
        public decimal? WeeklyUsageRollingAverage { get; set; }

        [NotMapped]
        public MedicationRoute MedicationRoute { get; set; }
        //////////[ForeignKey(nameof(MedicationRouteId))]
        //////////[InverseProperty(nameof(Entities.MedicationRoute.UserQuickListItems))]
        //////////public virtual MedicationRoute MedicationRoute { get; set; }

        [NotMapped]
        public Site Site { get; set; }
        //////////[ForeignKey(nameof(SiteId))]
        //////////[InverseProperty(nameof(UserQuickListItem))]
        //////////public virtual Site Site { get; set; }

        [NotMapped]
        public User User { get; set; }
        //////////[ForeignKey(nameof(UserId))]
        //////////[InverseProperty(nameof(UserQuickListItem))]
        //////////public virtual User User { get; set; }
    }
}
