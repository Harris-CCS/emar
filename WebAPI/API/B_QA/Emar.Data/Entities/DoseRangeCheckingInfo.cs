using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    public class DoseRangeCheckingInfo
    {
        [Column("GCN_SEQNO")]
        public int GcnSeqno { get; set; }
        
        [Column("type_description", TypeName = "varchar(200)")]
        public string TypeDescription { get; set; }
        
        [Column("age_description", TypeName = "varchar(200)")]
        public string AgeDdescription { get; set; }
        
        [Column("weight_description", TypeName = "varchar(200)")]
        public string WeightDescription { get; set; }
        
        [Column("amount_low", TypeName = "varchar(200)")]
        public string AmountLow { get; set; }
        
        [Column("amount_high", TypeName = "varchar(200)")]
        public string AmountHigh { get; set; } 
        
        [Column("unit_dose_abbreviation", TypeName = "varchar(200)")]
        public string UnitDoseAbbreviation { get; set; }
        
        [Column("max_frequency", TypeName = "varchar(200)")]
        public string MaxFrequency { get; set; }
        
        [Column("condition1_description", TypeName = "varchar(200)")]
        public string Condition1Description { get; set; }
        
        [Column("renal_description", TypeName = "varchar(200)")]
        public string RenalDescription { get; set; }
        
        [Column("route_description", TypeName = "varchar(200)")]
        public string RouteDescription { get; set; }
    }
}
