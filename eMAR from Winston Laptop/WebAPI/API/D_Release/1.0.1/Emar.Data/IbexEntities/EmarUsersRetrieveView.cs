using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("emar_users_retrieve_view")]
    public class EmarUsersRetrieveView  
    {
        [Key]
        [Column("id", TypeName = "int")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "tinyint")]
        public byte SiteId { get; set; }

        [Column("type", TypeName = "varchar(1)")]
        public string Type { get; set; }

        [Column("is_active", TypeName = "int")]
        public int? IsActive { get; set; }

        [Column("initials_display", TypeName = "varchar(4)")]
        public string InitialsDisplay { get; set; }

        [Column("first_name", TypeName = "varchar(20)")]
        public string FirstName { get; set; }

        [Column("last_name", TypeName = "varchar(20)")]
        public string LastName { get; set; }

        [Column("middle_name", TypeName = "varchar(1)"), Required]
        public string MiddleName { get; set; }

        [Column("name_suffix", TypeName = "varchar(1)"), Required]
        public string NameSuffix { get; set; }

        [Column("ordering_only_physician", TypeName = "int")]
        public int OrderingOnlyPhysician { get; set; }

        [Column("name_display_initials", TypeName = "int")]
        public int NameDisplayInitials { get; set; }

        [Column("login_name", TypeName = "varchar(20)")]
        public string LoginName { get; set; }

        [Column("login_password", TypeName = "varchar(39)")]
        public string LoginPassword { get; set; }

        [Column("salt", TypeName = "varbinary(1)"), Required]
        public byte[] Salt { get; set; }

        [Column("last_login_time", TypeName = "int")]
        public int? LastLoginTime { get; set; }

        [Column("failed_login_attempts", TypeName = "int")]
        public int FailedLoginAttempts { get; set; }

        [Column("medication_services_access", TypeName = "varchar(1)")]
        public string MedicationServicesAccess { get; set; }
    }
}