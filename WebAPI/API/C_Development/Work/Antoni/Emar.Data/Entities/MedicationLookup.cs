using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text;

namespace Emar.Data.Entities
{
    public class MedicationLookup
    {
        [Column("medication_id")]
        public int MedicationId { get; set; }

        [Column("brand_name", TypeName = "varchar(255)")]
        public string BrandName { get; set; }

        [Column("drug_id", TypeName = "varchar(32)")]
        public string DrugId { get; set; }

        [Column("inpatient_match")]
        public byte InpatientMatch { get; set; }

        [Column("outpatient_match")]
        public byte OutpatientMatch { get; set; }

        [Column("pyxis_match")]
        public byte PyxisMatch { get; set; }

        [Column("medid", TypeName = "numeric(8, 0)")]
        public decimal Medid { get; set; }

        [Column("GCN_SEQNO", TypeName = "numeric(8, 0)")]
        public decimal GcnSeqNo { get; set; }

        [Column("HICL_SEQNO", TypeName = "numeric(8, 0)")]
        public decimal HiclSeqNo { get; set; }
    }
}
