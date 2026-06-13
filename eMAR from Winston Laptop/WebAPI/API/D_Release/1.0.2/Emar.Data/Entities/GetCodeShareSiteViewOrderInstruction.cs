using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("get_code_share_site_view__order_instructions")]
    public class GetCodeShareSiteViewOrderInstruction
    {
        [Column("id", TypeName = "int")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int? SiteId { get; set; }


        // For Foreign Key: FK_get_code_share_site_view__order_instructions_sites_site_id 
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.CodeShareSiteOrderInstructions))]
        public virtual Site Site { get; set; }
    }
}