using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("users")]
    public class User
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public short SiteId { get; set; }

        [Column("is_active", TypeName = "char(1)"), Required]
        public bool Active { get; set; }

        [Column("initials_display", TypeName = "varchar(4)"), Required]
        public bool InitialsDisplay { get; set; }

        [Column("first_name", TypeName = "varchar(20)"), Required]
        public string FirstName { get; set; }

        [Column("last_name", TypeName = "varchar(20)"), Required]
        public string LastName { get; set; }

        [Column("ordering_only_physician", TypeName = "bit")]
        public bool OrderingOnlyPhysician { get; set; }

        [Column("name_display_preference", TypeName = "bit")]
        public bool NameDisplayPreference { get; set; }

        [Column("login_name", TypeName = "varchar(255)"), Required]
        public string LoginName { get; set; }

        [Column("login_password", TypeName = "varchar(255)"), Required]
        public string LoginPassword { get; set; }

        [Column("salt", TypeName = "binary(16)"), Required]
        public byte[] Salt { get; set; }

        [Column("last_login_time", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset LastLoginTime { get; set; }

        [Column("failed_login_attempts", TypeName = "int"), Required]
        public int FailedLoginAttempts { get; set; }
    }
}
