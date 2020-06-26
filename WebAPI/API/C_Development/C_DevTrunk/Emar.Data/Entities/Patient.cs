using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    public class Patient
    {
        [Key]
        public long Id { get; set; }
        
        [Column("site_id", TypeName = "int")]
        public short SiteId { get; set; }

        //[Column("is_active", TypeName = "int")]
        //public bool Active { get; set; }

        [Column("first_name", TypeName = "varchar(35)")]
        public string FirstName { get; set; }

        [Column("middle_name", TypeName = "varchar(35)")]
        public string MiddleName { get; set; }

        [Column("last_name", TypeName = "varchar(35)")]
        public string LastName { get; set; }

        [Column("name_suffix", TypeName = "varchar(25)")]
        public string NameSuffix { get; set; }

        [Column("gender", TypeName = "varchar(10)")]
        public string Gender { get; set; }

        [Column("date_of_birth", TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [Column("age", TypeName = "tinyint")]
        public short? Age { get; set; }

        [Column("age_units", TypeName = "char(1)")]
        public string AgeUnits { get; set; }
    }
}
