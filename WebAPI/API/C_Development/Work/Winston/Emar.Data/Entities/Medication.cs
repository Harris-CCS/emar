using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("medications")]
    public class Medication
    {
        public Medication()
        {
            MedicationDetails = new HashSet<MedicationDetail>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
            DepartmentPreferredListItems = new HashSet<DepartmentPreferredListItem>();
            GroupListItems = new HashSet<GroupListItem>();
            PatientAllergys = new HashSet<PatientAllergy>();
            PatientCartOrders = new HashSet<PatientCartOrder>();
            PatientHomeMedications = new HashSet<PatientHomeMedication>();
            PatientOrders = new HashSet<PatientOrder>();
            SiteFormularys = new HashSet<SiteFormulary>();
            SiteFormularyMatchs = new HashSet<SiteFormularyMatch>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("site_id")]
        public int SiteId { get; set; }
        [Required]
        [Column("display_name")]
        [StringLength(255)]
        public string DisplayName { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.Medications))]
        public virtual Site Site { get; set; }

        [Column("drug_id", TypeName = "varchar(32)"), Required]
        public string DrugId { get; set; }

        [Column("drug_vendor", TypeName = "char(1)"), Required]
        public string DrugVendor { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<MedicationDetail> MedicationDetails { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<GroupListItem> GroupListItems { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<PatientAllergy> PatientAllergys { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<PatientHomeMedication> PatientHomeMedications { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<SiteFormulary> SiteFormularys { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<SiteFormularyMatch> SiteFormularyMatchs { get; set; }
    }
}
