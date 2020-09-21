using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("sites")]
    public class Site
    {
        public Site()
        {
            DepartmentPreferredListItems = new HashSet<DepartmentPreferredListItem>();
            GroupListItems = new HashSet<GroupListItem>();
            MedicationRoutes = new HashSet<MedicationRoute>();
            MedicationUnits = new HashSet<MedicationUnit>();
            OverrideReasons = new HashSet<OverrideReason>();
            Patients = new HashSet<Patient>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
            Users = new HashSet<User>();
            SiteOptions = new HashSet<SiteOption>();
        }

        [Key]
        [Column("id", TypeName = "int")]
        public int Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(40)]
        public string Name { get; set; }
        [Column("is_active")]
        public bool IsActive { get; set; }
        [Required]
        [Column("time_zone_name")]
        [StringLength(128)]
        public string TimeZoneName { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<GroupListItem> GroupListItems { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<MedicationRoute> MedicationRoutes { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<MedicationUnit> MedicationUnits { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<OverrideReason> OverrideReasons { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<Patient> Patients { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<User> Users { get; set; }

        [InverseProperty("Site")]
        public virtual ICollection<SiteOption> SiteOptions { get; set; }
    }
}