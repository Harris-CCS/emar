using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("fdb_ndc_info")]
    public partial class FdbNdcInfo
    {
        //[Required]
        //[Key]
        [Column("ndc")]
        [StringLength(11)]
        public string Ndc { get; set; }

        [Column("base_ndc")]
        [StringLength(11)]
        public string BaseNdc { get; set; }

        [Column("repackaged")]
        public int Repackaged { get; set; }

        [Column("medid", TypeName = "numeric(8, 0)")]
        public decimal Medid { get; set; }

        [NotMapped]
        public string MedidString => Medid.ToString();

        [Column("packaging")]
        [StringLength(26)]
        public string Packaging { get; set; }

        [Column("strength")]
        [StringLength(91)]
        public string Strength { get; set; }

        [Column("days_obsolete")]
        public int? DaysObsolete { get; set; }

        [Column("GCN_SEQNO", TypeName = "numeric(8, 0)")]
        public decimal GcnSeqNo { get; set; }

        [Column("HICL_SEQNO", TypeName = "numeric(8, 0)")]
        public decimal HiclSeqNo { get; set; }
    }
}
