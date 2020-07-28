using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Emar.Data.Entities
{
    [Table("user_quick_list_items")]
    public class UserQuickListItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("site_id")]
        public int SiteId { get; set; }
        
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Column("ndc")]
        [StringLength(32)]
        public string Ndc { get; set; }
        
        [Column("drug_id")]
        [StringLength(32)]
        public string DrugId { get; set; }
        
        [Required]
        [Column("brand_name")]
        [StringLength(255)]
        public string BrandName { get; set; }
        
        [Column("dose", TypeName = "decimal(11, 2)")]
        public decimal? Dose { get; set; }
        
        [Column("dose_unit")]
        [StringLength(20)]
        public string DoseUnit { get; set; }
        
        [Column("medication_route_id")]
        public int? MedicationRouteId { get; set; }
        
        [Column("frequency_id")]
        public int? FrequencyId { get; set; }
        
        [Column("order_notes")]
        public string OrderNotes { get; set; }

        [Column("usages_this_week", TypeName = "int")]
        public int? UsagesThisWeek { get; set; }

        [Column("weekly_usage_rolling_average", TypeName = "decimal(9, 3)")]
        public decimal? WeeklyUsageRollingAverage { get; set; }

        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.UserQuickListItems))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.UserQuickListItems))]
        public virtual Site Site { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Entities.User.UserQuickListItems))]
        public virtual User User { get; set; }
    }
}
