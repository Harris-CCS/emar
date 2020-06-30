using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("external_ids")]
    public class ExternalId
    {
        [Key]
        [Column("internal_id", TypeName = "bigint"), Required]
        public long InternalId { get; set; }

        [Column("vendor", TypeName = "varchar(50)"), Required]
        public string Vendor { get; set; }

        [Column("entity", TypeName = "varchar(50)"), Required]
        public string Entity { get; set; }

        [Column("external_id", TypeName = "varchar(50)"), Required]
        public string External_Id { get; set; }
    }
}
