using System;
using System.Collections.Generic;
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
        public int SiteId { get; set; }

        [Column("type", TypeName = "char(1)"), Required]
        public string Type { get; set; }

        [Column("is_active", TypeName = "bit"), Required]
        public bool IsActive { get; set; }

        [Column("initials_display", TypeName = "nvarchar(4)"), Required]
        public string InitialsDisplay { get; set; }

        string firstName;
        [Column("first_name", TypeName = "nvarchar(20)"), Required]
        public string FirstName { get; set; }

        [Column("last_name", TypeName = "nvarchar(20)"), Required]
        public string LastName { get; set; }

        [Column("ordering_only_physician", TypeName = "bit")]
        public bool? OrderingOnlyPhysician { get; set; }

        [Column("name_display_initials", TypeName = "bit")]
        public bool? NameDisplayInitials { get; set; }

        string loginName;
        [Column("login_name", TypeName = "varchar(255)"), Required]
        public string LoginName { get; set; }

        [Column("login_password", TypeName = "varchar(255)"), Required]
        public string LoginPassword { get; set; }

        [Column("salt", TypeName = "binary(16)"), Required]
        public byte[] Salt { get; set; }

        [Column("last_login_time", TypeName = "datetimeoffset")]
        public DateTimeOffset? LastLoginTime { get; set; }

        [Column("failed_login_attempts", TypeName = "int"), Required]
        public int FailedLoginAttempts { get; set; }

        [NotMapped]
        public Site Site { get; set; }
    }
}
