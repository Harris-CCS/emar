using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("site_formulary_match")]
    public partial class SiteFormularyMatch
    {
        [Required]
        [Column("id")]
        public long Id { get; set; }

        [Column("site_id")]
        public int SiteId { get; set; }

        [Column("inpatient_match")]
        public byte InpatientMatch { get; set; }

        [Column("outpatient_match")]
        public byte OutpatientMatch { get; set; }

        [Column("pyxis_match")]
        public byte PyxisMatch { get; set; }

        [Column("medication_id")]
        public int MedicationId { get; set; }
        
        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.SiteFormularyMatchs))]
        public virtual Site Site { get; set; }

        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.SiteFormularyMatchs))]
        public virtual Medication Medication { get; set; }
    }
}
