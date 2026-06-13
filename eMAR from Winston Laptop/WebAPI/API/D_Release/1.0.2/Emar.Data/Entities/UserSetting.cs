using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("user_settings")]
    public class UserSetting
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        [Column("user_id", TypeName = "int")]
        public int UserId { get; set; }

        [Column("setting_id", TypeName = "int")]
        public int SettingId { get; set; }

        [Column("setting_value", TypeName = "varchar(255)"), Required]
        public string SettingValue { get; set; }


        /// <summary>
        /// IDS: The following property is provided so that we don't have to go through a generic object
        /// </summary>
        [NotMapped]
        public bool DefaultOnlySetting { get; set; }


        // For Foreign Key: fk__user_settings__settings
        [ForeignKey(nameof(SettingId))]
        [InverseProperty(nameof(Entities.Setting.UserSettings))]
        public virtual Setting Setting { get; set; }

        // For Foreign Key: fk__user_settings__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.UserSettings))]
        public virtual Site Site { get; set; }

        // For Foreign Key: fk__user_settings__users
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Entities.User.UserSettings))]
        public virtual User User { get; set; }
    }
}