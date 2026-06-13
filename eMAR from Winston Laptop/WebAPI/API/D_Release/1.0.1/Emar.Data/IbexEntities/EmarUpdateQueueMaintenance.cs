using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    public class EmarUpdateQueueMaintenance
    {
        [Key]
        [Column("id", TypeName = "bigint")]
        public long MaxId { get; set; }

        [Column("entity", TypeName = "varchar(50)"), Required]
        public string Entity { get; set; }

        [Column("external_id", TypeName = "varchar(50)"), Required]
        public string ExternalId { get; set; }
    }
}
