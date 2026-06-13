using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("settings")]
    public class Setting
    {
        public Setting()
        {
            // For Foreign Key: fk__user_settings__settings
            UserSettings = new HashSet<UserSetting>();
        }

        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("name", TypeName = "nvarchar(40)"), Required]
        public string Name { get; set; }

        [Column("description", TypeName = "nvarchar(255)"), Required]
        public string Description { get; set; }

        // For Foreign Key: fk__user_settings__settings
        [InverseProperty("Setting")]
        public virtual ICollection<UserSetting> UserSettings { get; set; }
    }
}