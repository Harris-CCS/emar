using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("medication_details")]
    public class MedicationDetail
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        [Column("drug_id", TypeName = "varchar(32)"), Required]
        public string DrugId { get; set; }

        [Column("brand_name", TypeName = "nvarchar(255)"), Required]
        public string BrandName { get; set; }

        [Column("active_list", TypeName = "nvarchar(max)"), Required]
        public string ActiveList { get; set; }

        [Column("dose", TypeName = "decimal(11,2)")]
        public decimal? Dose { get; set; }

        [Column("medication_unit_id", TypeName = "int")]
        public int? MedicationUnitId { get; set; }

        [Column("is_active", TypeName = "bit")]
        public bool IsActive { get; set; }


        // For Foreign Key: fk__medication_details__medications
        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.MedicationDetails))]
        public virtual Medication Medication { get; set; }

        // For Foreign Key: fk__medication_details__fdb_brand_name 
        [ForeignKey(nameof(DrugId))]
        [InverseProperty(nameof(Entities.FdbBrandName.MedicationDetails))]
        public FdbBrandName FdbBrandName { get; set; }

        // For Foreign Key: fk__medication_details__medication_units
        [ForeignKey(nameof(MedicationUnitId))]
        [InverseProperty(nameof(Entities.MedicationUnit.MedicationDetails))]
        public virtual MedicationUnit MedicationUnit { get; set; }
    }
}