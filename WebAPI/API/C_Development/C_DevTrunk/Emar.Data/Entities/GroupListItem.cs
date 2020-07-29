using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("group_list_items")]
    public class GroupListItem
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public int SiteId { get; set; }

        [Column("group_name", TypeName = "nvarchar(255)"), Required]
        public string GroupName { get; set; }

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


        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.GroupListItems))]
        public virtual MedicationRoute MedicationRoute { get; set; }
 
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.GroupListItems))]
        public virtual Site Site { get; set; }
    }
}
