using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("medication_units")]
    public partial class MedicationUnit
    {
        public MedicationUnit()
        {
            DepartmentPreferredListItems = new HashSet<DepartmentPreferredListItem>();
            GroupListItems = new HashSet<GroupListItem>();
            PatientCartOrders = new HashSet<PatientCartOrder>();
            PatientHomeMedications = new HashSet<PatientHomeMedication>();
            PatientOrders = new HashSet<PatientOrder>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        [Required]
        [Column("code")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [Column("print_name")]
        [StringLength(50)]
        public string PrintName { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; }


        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.MedicationUnits))]
        public virtual Site Site { get; set; }

        [InverseProperty("MedicationUnit")]
        public virtual ICollection<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }

        [InverseProperty("MedicationUnit")]
        public virtual ICollection<GroupListItem> GroupListItems { get; set; }

        [InverseProperty("MedicationUnit")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        [InverseProperty("MedicationUnit")]
        public virtual ICollection<PatientHomeMedication> PatientHomeMedications { get; set; }

        [InverseProperty("MedicationUnit")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

        [InverseProperty("MedicationUnit")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }
    }
}
