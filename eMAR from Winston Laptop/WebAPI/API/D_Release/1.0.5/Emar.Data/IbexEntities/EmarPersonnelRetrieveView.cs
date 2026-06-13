using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("emar_personnel_retrieve_view")]
    public class EmarPersonnelRetrieveView
    {
        [Key]
        [Column("external_patient_id", TypeName = "char(14)"), Required]
        public string ExternalId { get; set; }

        [Column("external_site_id", TypeName = "tinyint")]
        public byte ExternalSiteId { get; set; }

        [Column("external_user_id", TypeName = "int")]
        public int ExternalUserId { get; set; }

        [Column("role_name", TypeName = "varchar(25)")]
        public string RoleName { get; set; }
    }
}
