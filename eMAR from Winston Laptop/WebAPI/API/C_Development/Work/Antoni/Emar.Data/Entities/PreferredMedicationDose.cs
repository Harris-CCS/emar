using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("preferred_medication_doses")]
    public class PreferredMedicationDose
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        [Column("dose", TypeName = "decimal(11,2)")]
        public decimal Dose { get; set; }

        [Column("medication_unit_id", TypeName = "int")]
        public int MedicationUnitId { get; set; }

        [Column("site_id", TypeName = "int")]
        public int SiteId { get; set; }


        // For Foreign Key: fk__preferred_medication_doses__medication_units
        [ForeignKey(nameof(MedicationUnitId))]
        [InverseProperty(nameof(Entities.MedicationUnit.PreferredMedicationDoses))]
        public virtual MedicationUnit MedicationUnit { get; set; }

        // For Foreign Key: fk__preferred_medication_doses__medications
        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.PreferredMedicationDoses))]
        public virtual Medication Medication { get; set; }

        // For Foreign Key: fk__preferred_medication_doses__sites
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.PreferredMedicationDoses))]
        public virtual Site Site { get; set; }
    }
}