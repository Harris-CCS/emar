using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Emar.Data.Entities
{
    [Table("medications_high_risk")]
    public class MedicationsHighRisk
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("long_brand_name", TypeName = "nvarchar(255)")]
        public string LongBrandName { get; set; }

        [Column("active", TypeName = "nvarchar(255)")]
        public string Active { get; set; }


        //This column matches to the routed_gen_id column in both
        //fdb_ndc_info and fdb_brand_name.
        [Column("RoutedGenId", TypeName = "numeric(8, 0)")]
        public decimal RoutedGenId { get; set; }

        [Column("pc_routed_gen_id", TypeName = "varchar(9)")]
        public string PcRoutedGenId { get; set; }

        [Column("route", TypeName = "varchar(40)")]
        public string Route { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        //For foreign key fk__medications_high_risk__medications
        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.MedicationsHighRisks))]
        public virtual Medication Medication { get; set; }
    }
}
