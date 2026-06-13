using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("preferred_medication_routes")]
    public class PreferredMedicationRoute
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        [Column("medication_route_id", TypeName = "int")]
        public int MedicationRouteId { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }

        // For Foreign Key: fk__preferred_medication_routes__medication_routes
        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.PreferredMedicationRoutes))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        // For Foreign Key: fk__preferred_medication_routes__medications
        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.PreferredMedicationRoutes))]
        public virtual Medication Medication { get; set; }

        // For Foreign Key: fk__preferred_medication_routes__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.PreferredMedicationRoutes))]
        public virtual Site Site { get; set; }
    }
}