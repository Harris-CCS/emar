using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("get_code_share_site_view__medication_units")]
    public class GetCodeShareSiteViewMedicationUnit
    {
        [Column("id", TypeName = "int")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int? SiteId { get; set; }


        // For Foreign Key: FK_get_code_share_site_view__medication_units_sites_site_id 
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.CodeShareSiteMedicationUnits))]
        public virtual Site Site { get; set; }
    }
}