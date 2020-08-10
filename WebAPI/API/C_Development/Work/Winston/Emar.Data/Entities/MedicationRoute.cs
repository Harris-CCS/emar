using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("medication_routes")]
    public class MedicationRoute
    {
        public MedicationRoute()
        {
            DepartmentPreferredListItems = new HashSet<DepartmentPreferredListItem>();
            GroupListItems = new HashSet<GroupListItem>();
            PatientCartOrders = new HashSet<PatientCartOrder>();
            PatientOrders = new HashSet<PatientOrder>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public long SiteId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.MedicationRoutes))]
        public virtual Site Site { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<GroupListItem> GroupListItems { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }
    }
}
