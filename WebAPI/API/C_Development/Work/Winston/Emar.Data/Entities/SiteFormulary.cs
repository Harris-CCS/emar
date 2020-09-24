using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("site_formulary")]
    public partial class SiteFormulary
    {
        [Required]
        [Column("id")]
        public long Id { get; set; }

        [Column("site_id")]
        public int SiteId { get; set; }

        [Column("hospital_drug_code", TypeName = "varchar(32)")]
        public string HospitalDrugCode { get; set; }

        [Column("service_code", TypeName = "varchar(32)")]
        public string ServiceCode { get; set; }

        [Column("is_inpatient")]
        public bool IsInpatient { get; set; }

        [Column("is_outpatient")]
        public bool IsOutpatient { get; set; }
        
        [Column("is_pyxis")]
        public bool IsPyxis { get; set; }

        [Column("medication_id")]
        public int MedicationId { get; set; }

        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.SiteFormularys))]
        public virtual Medication Medication { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.SiteFormularys))]
        public virtual Site Site { get; set; }
    }
}
