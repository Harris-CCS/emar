using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    public class EmarPatientsRetrieveView
    {
        [Key]
        [Column("site_id", TypeName = "tinyint")]
        public byte SiteId { get; set; }
    }
}
