using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("fdb_brand_name")]
    public partial class FdbBrandName
    {
        public FdbBrandName()
        {
            // For Foreign Key: fk__medication_details__fdb_brand_name
            MedicationDetails = new HashSet<MedicationDetail>();
            // For Foreign Key: FK_medications_fdb_brand_name_drug_id
            Medications = new HashSet<Medication>();
        }

        [Column("MEDID_string", TypeName = "varchar(32)"), Key]
        [ReadOnly(true)]
        public string MedidString { get; set; }

        [Column("MEDID", TypeName = "numeric(8, 0)")]
        public decimal Medid { get; set; }
        [Column("long_brand_name")]
        [StringLength(70)]
        public string LongBrandName { get; set; }
        [Column("active")]
        public string Active { get; set; }
        [Column("MED_NAME_ID", TypeName = "numeric(8, 0)")]
        public decimal? MedNameId { get; set; }
        [Column("PC_MED_NAME_ID")]
        [StringLength(9)]
        public string PcMedNameId { get; set; }
        [Column("ROUTED_GEN_ID", TypeName = "numeric(8, 0)")]
        public decimal? RoutedGenId { get; set; }
        [Column("PC_ROUTED_GEN_ID")]
        [StringLength(9)]
        public string PcRoutedGenId { get; set; }
        [Column("brand_name")]
        [StringLength(70)]
        public string BrandName { get; set; }
        [Required]
        [Column("dea_schedule")]
        [StringLength(1)]
        public string DeaSchedule { get; set; }
        [Column("rx_otc")]
        [StringLength(1)]
        public string RxOtc { get; set; }
        [Column("erx_search")]
        public int ErxSearch { get; set; }


        // For Foreign Key: fk__medication_details__fdb_brand_name
        [InverseProperty(nameof(FdbBrandName))]
        public virtual ICollection<MedicationDetail> MedicationDetails { get; set; }

        // For Foreign Key: FK_medications_fdb_brand_name_drug_id
        [InverseProperty(nameof(FdbBrandName))]
        public virtual ICollection<Medication> Medications { get; set; }
    }
}
