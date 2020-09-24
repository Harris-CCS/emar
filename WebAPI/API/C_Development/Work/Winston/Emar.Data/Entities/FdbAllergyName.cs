using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("fdb_allergy_name")]
    public partial class FdbAllergyName
    {
        //[Key]
        [Column("MEDID", TypeName = "numeric(8, 0)")]
        public decimal Medid { get; set; }
        [Column("med_name")]
        [StringLength(70)]
        public string MedName { get; set; }
        [Column("MED_NAME_ID", TypeName = "numeric(8, 0)")]
        public decimal? MedNameId { get; set; }
        [Column("PC_MED_NAME_ID")]
        [StringLength(9)]
        public string PcMedNameId { get; set; }
        [Column("HICL_SEQNO", TypeName = "numeric(6, 0)")]
        public decimal? HiclSeqno { get; set; }
        [Column("PC_HICL_SEQNO")]
        [StringLength(7)]
        public string PcHiclSeqno { get; set; }
        [Column("allergy_name")]
        [StringLength(70)]
        public string AllergyName { get; set; }
    }
}
