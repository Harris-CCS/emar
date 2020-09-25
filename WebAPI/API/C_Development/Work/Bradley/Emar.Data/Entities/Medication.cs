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
            // For Foreign Key: fk__department_preferred_list_items__medications
            DepartmentPreferredListItems = new HashSet<DepartmentPreferredListItem>();
            // For Foreign Key: fk__group_list_items__medications
            GroupListItems = new HashSet<GroupListItem>();
            // For Foreign Key: fk__medication_details__medications
            MedicationDetails = new HashSet<MedicationDetail>();
            // For Foreign Key: fk__patient_allergies__medications
            PatientAllergys = new HashSet<PatientAllergy>();
            // For Foreign Key: fk__patient_cart_orders__medications
            PatientCartOrders = new HashSet<PatientCartOrder>();
            // For Foreign Key: fk__patient_home_medications__medications
            PatientHomeMedications = new HashSet<PatientHomeMedication>();
            // For Foreign Key: fk__patient_orders__medications
            PatientOrders = new HashSet<PatientOrder>();
            // For Foreign Key: fk__site_formulary__medications
            SiteFormularys = new HashSet<SiteFormulary>();
            // For Foreign Key: fk__site_formulary_match__medications
            SiteFormularyMatchs = new HashSet<SiteFormularyMatch>();
            // For Foreign Key: fk__user_quick_list_items__medications
            UserQuickListItems = new HashSet<UserQuickListItem>();
        }

        [Column("id", TypeName = "int"), Key] public int Id { get; set; }

        [Column("site_id", TypeName = "int")] public int SiteId { get; set; }

        [Column("drug_id", TypeName = "varchar(32)"), Required]
        public string DrugId { get; set; }

        [Column("display_name", TypeName = "nvarchar(255)"), Required]
        public string DisplayName { get; set; }

        [Column("drug_vendor", TypeName = "char(1)"), Required]
        public string DrugVendor { get; set; }

        // For Foreign Key: fk__medications__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.Medications))]
        public virtual Site Site { get; set; }

        // For Foreign Key: fk__department_preferred_list_items__medications
        [InverseProperty("Medication")]
        public virtual ICollection<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }

        // For Foreign Key: fk__group_list_items__medications
        [InverseProperty("Medication")]
        public virtual ICollection<GroupListItem> GroupListItems { get; set; }

        // For Foreign Key: fk__medication_details__medications
        [InverseProperty("Medication")]
        public virtual ICollection<MedicationDetail> MedicationDetails { get; set; }

        // For Foreign Key: fk__patient_allergies__medications
        [InverseProperty("Medication")]
        public virtual ICollection<PatientAllergy> PatientAllergys { get; set; }

        // For Foreign Key: fk__patient_cart_orders__medications
        [InverseProperty("Medication")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        // For Foreign Key: fk__patient_home_medications__medications
        [InverseProperty("Medication")]
        public virtual ICollection<PatientHomeMedication> PatientHomeMedications { get; set; }

        // For Foreign Key: fk__patient_orders__medications
        [InverseProperty("Medication")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

        // For Foreign Key: fk__user_quick_list_items__medications
        [InverseProperty("Medication")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }


        [InverseProperty("Medication")]
        public virtual ICollection<SiteFormulary> SiteFormularys { get; set; }

        [InverseProperty("Medication")]
        public virtual ICollection<SiteFormularyMatch> SiteFormularyMatchs { get; set; }

    }
}
