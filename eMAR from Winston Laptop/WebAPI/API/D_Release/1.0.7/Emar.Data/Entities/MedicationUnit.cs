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
            // For Foreign Key: fk__medication_details__medication_units
            MedicationDetails = new HashSet<MedicationDetail>();
            PatientCartOrders = new HashSet<PatientCartOrder>();
            PatientHomeMedications = new HashSet<PatientHomeMedication>();
            PatientOrders = new HashSet<PatientOrder>();
            // For Foreign Key: fk__preferred_medication_doses__medication_units
            PreferredMedicationDoses = new HashSet<PreferredMedicationDose>();
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

        [Column("priority", TypeName = "int")]
        public int? Priority { get; set; }



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

        // For Foreign Key: fk__preferred_medication_doses__medication_units
        [InverseProperty("MedicationUnit")]
        public virtual ICollection<PreferredMedicationDose> PreferredMedicationDoses { get; set; }

        [InverseProperty("MedicationUnit")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }

        // For Foreign Key: fk__medication_details__medication_units
        [InverseProperty("MedicationUnit")]
        public virtual ICollection<MedicationDetail> MedicationDetails { get; set; }
    }
}
