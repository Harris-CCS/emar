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
            // For Foreign Key: fk__frequency_schedules__sites
            FrequencySchedules = new HashSet<FrequencySchedule>();
            GroupListItems = new HashSet<GroupListItem>();
            MedicationRoutes = new HashSet<MedicationRoute>();
            //For Foreign Key: fk__medications__sites
            Medications = new HashSet<Medication>();
            MedicationUnits = new HashSet<MedicationUnit>();
            OverrideReasons = new HashSet<OverrideReason>();
            Patients = new HashSet<Patient>();
            // For Foreign Key: fk__site_formulary__sites
            SiteFormularys = new HashSet<SiteFormulary>();
            // For Foreign Key: fk__site_formulary_match__sites
            SiteFormularyMatchs = new HashSet<SiteFormularyMatch>();
            SiteOptions = new HashSet<SiteOption>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
            Users = new HashSet<User>();
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

        // For Foreign Key: fk__frequency_schedules__sites
        [InverseProperty("Site")]
        public virtual ICollection<FrequencySchedule> FrequencySchedules { get; set; }

        // For Foreign Key: fk__medications__sites
        [InverseProperty("Site")]
        public virtual ICollection<Medication> Medications { get; set; }

        // For Foreign Key: fk__site_formulary__sites
        [InverseProperty("Site")]
        public virtual ICollection<SiteFormulary> SiteFormularys { get; set; }

        // For Foreign Key: fk__site_formulary_match__sites
        [InverseProperty("Site")]
        public virtual ICollection<SiteFormularyMatch> SiteFormularyMatchs { get; set; }
    }
}