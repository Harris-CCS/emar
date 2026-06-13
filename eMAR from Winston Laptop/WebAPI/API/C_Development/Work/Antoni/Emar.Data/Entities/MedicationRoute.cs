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
            // For Foreign Key: fk__action_route_templates__medication_routes
            ActionRouteTemplates = new HashSet<ActionRouteTemplate>();
            DepartmentPreferredListItems = new HashSet<DepartmentPreferredListItem>();
            GroupListItems = new HashSet<GroupListItem>();
            PatientCartOrders = new HashSet<PatientCartOrder>();
            PatientHomeMedications = new HashSet<PatientHomeMedication>();
            PatientOrders = new HashSet<PatientOrder>();
            // For Foreign Key: fk__preferred_medication_routes__medication_routes
            PreferredMedicationRoutes = new HashSet<PreferredMedicationRoute>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public int SiteId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }

        [Column("priority", TypeName = "int")]
        public int? Priority { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.MedicationRoutes))]
        public virtual Site Site { get; set; }

        // For Foreign Key: fk__action_route_templates__medication_routes
        [InverseProperty("MedicationRoute")]
        public virtual ICollection<ActionRouteTemplate> ActionRouteTemplates { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<DepartmentPreferredListItem> DepartmentPreferredListItems { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<GroupListItem> GroupListItems { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<PatientHomeMedication> PatientHomeMedications { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

        // For Foreign Key: fk__preferred_medication_routes__medication_routes
        [InverseProperty("MedicationRoute")]
        public virtual ICollection<PreferredMedicationRoute> PreferredMedicationRoutes { get; set; }

        [InverseProperty("MedicationRoute")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }
    }
}
