using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("external_ids")]
    // Breaking the table/entity name pattern because the column "ExternalId" conflicts with the derived entity name
    public class ExternalIdEntity
    {
        [Key]
        [Column("internal_id", TypeName = "bigint")]
        public long InternalId { get; set; }

        [Key]
        [Column("vendor", TypeName = "varchar(50)")]
        public string Vendor { get; set; }

        [Key]
        [Column("entity", TypeName = "varchar(50)")]
        public string Entity { get; set; }

        [Required]
        [Column("external_id", TypeName = "varchar(50)")]
        public string ExternalId { get; set; }

        //[ForeignKey(nameof(InternalId))]
        //[InverseProperty(nameof(Entities.Patient.ExternalIds))]
        //public virtual Patient Patient { get; set; }

        //  This foreign key is not in the database, and can't be enforceable if it were:
        //    - The datatypes don't line up, and 
        //    - values exist in ExternalIds that don't point to the patients table
        //[ForeignKey(nameof(InternalId))]
        //[InverseProperty(nameof(Entities.Site.ExternalIds))]
        //public Site Site { get; set; }
    }
}
