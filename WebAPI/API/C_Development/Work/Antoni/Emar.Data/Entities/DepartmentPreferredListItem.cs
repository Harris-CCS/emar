using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("department_preferred_list_items")]
    public partial class DepartmentPreferredListItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("site_id")]
        public int SiteId { get; set; }

        [Column("department_code")]
        [StringLength(15)]
        public string DepartmentCode { get; set; }

        [Column("ndc")]
        [StringLength(32)]
        public string Ndc { get; set; }

        [Required]
        [Column("drug_id")]
        [StringLength(32)]
        public string DrugId { get; set; }

        [Required]
        [Column("brand_name")]
        [StringLength(255)]
        public string BrandName { get; set; }

        [Column("dose", TypeName = "decimal(11, 2)")]
        public decimal? Dose { get; set; }

        [Column("medication_unit_id")]
        public int? MedicationUnitId { get; set; }

        [Column("medication_route_id")]
        public int? MedicationRouteId { get; set; }

        [Column("frequency_id")]
        public int? FrequencyId { get; set; }

        [Column("order_notes")]
        public string OrderNotes { get; set; }


        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.DepartmentPreferredListItems))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        [ForeignKey(nameof(MedicationUnitId))]
        [InverseProperty(nameof(Entities.MedicationUnit.DepartmentPreferredListItems))]
        public virtual MedicationUnit MedicationUnit { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.DepartmentPreferredListItems))]
        public virtual Site Site { get; set; }
    }
}
