using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("fdb_ndc_info")]

    public partial class FdbNdcInfo
    {
        [Column("ndc", TypeName = "varchar(11)"), Key]
        public string Ndc { get; set; }

        [Column("base_ndc")]
        [StringLength(11)]
        public string BaseNdc { get; set; }
        
        [Column("repackaged")]
        public int Repackaged { get; set; }
        
        [Column("medid", TypeName = "numeric(8, 0)")]
        public decimal Medid { get; set; }

        [Column("MEDID_string", TypeName = "varchar(32)")]
        public string MedidString { get; set; }

        [Column("packaging")]
        [StringLength(26)]
        public string Packaging { get; set; }
        
        [Column("strength")]
        [StringLength(91)]
        public string Strength { get; set; }
        
        [Column("days_obsolete")]
        public int? DaysObsolete { get; set; }
        
        [Column("GCN_SEQNO", TypeName = "numeric(6, 0)")]
        public decimal? GcnSeqno { get; set; }
        
        [Column("HICL_SEQNO", TypeName = "numeric(6, 0)")]
        public decimal? HiclSeqno { get; set; }
        
        [Column("ROUTED_GEN_ID", TypeName = "numeric(8, 0)")]
        public decimal? RoutedGenId { get; set; }

        [Column("dose_form", TypeName = "varchar(30)")]
        public string DoseForm { get; set; }

        [Column("route", TypeName = "varchar(40)")]
        public string Route { get; set; }

        [Column("drugcat", TypeName = "numeric(8, 0)")]
        public decimal? DrugCat { get; set; }
    }
}