using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("sites")]
    public class Site
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("name", TypeName = "nvarchar(40)"), Required]
        public string Name { get; set; }

        [Column("is_active", TypeName = "bit"), Required]
        public bool IsActive { get; set; }

        [Column("time_zone_name", TypeName = "sys.sysname"), Required]
        public string TimeZoneName { get; set; }
    }
}
