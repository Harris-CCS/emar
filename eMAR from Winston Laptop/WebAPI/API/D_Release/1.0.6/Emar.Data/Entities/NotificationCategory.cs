using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("notification_categories")]
    public class NotificationCategory
    {
        [Column("id")]
        public int Id { get; set; }

        [Key]
        [Column("code", TypeName = "varchar(20)"), Required]
        public string Code { get; set; }

        [Column("description", TypeName = "nvarchar(150)"), Required]
        public string Description { get; set; }

        [Column("priority", TypeName = "smallint"), Required]
        public int Priority { get; set; }

        [Column("action_url", TypeName = "varchar(255)")]
        public string ActionUrl { get; set; }
    }
}