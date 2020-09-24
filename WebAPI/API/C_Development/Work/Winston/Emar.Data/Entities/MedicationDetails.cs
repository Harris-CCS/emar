using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("medication_details")]
    public class MedicationDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("medication_id")]
        public int MedicationId { get; set; }

        [Column("drug_id", TypeName = "varchar(32)"), Required]
        public string DrugId { get; set; }

        [Column("brand_name", TypeName = "nvarchar(255)"), Required]
        public string BrandName { get; set; }

        [Column("dose", TypeName = "decimal(11, 2)")]
        public decimal? Dose { get; set; }
        
        [Column("medication_unit_id")]
        public int? MedicationUnitId { get; set; }
        
        [Column("medication_route_id")]
        public int? MedicationRouteId { get; set; }

        [Column("active_list", TypeName = "nvarchar(max)"), Required]
        public string ActiveList { get; set; }

        // Missing column to be added to the Entity
        [Column("is_active", TypeName = "bit")]
        public bool IsActive { get; set; }

        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.MedicationDetails))]
        public virtual Medication Medication { get; set; }
    }
}
