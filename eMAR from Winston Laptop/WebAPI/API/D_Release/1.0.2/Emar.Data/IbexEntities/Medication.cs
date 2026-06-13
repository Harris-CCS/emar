using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("med")]
    public class Medication
    {
        public Medication()
        {

        }

        [Column("id", TypeName = "int"), Key]                    public int Id { get; set; }

        [Column("ibex", TypeName = "char(14)")]                  public string Ibex { get; set; }
		
        [Column("site", TypeName = "smallint")]                  public int Site { get; set; }

        [Column("losecs", TypeName = "int")]                     public int Losecs { get; set; }

        [Column("status", TypeName = "char(1)")]                 public string Status { get; set; }

        [Column("type", TypeName = "char(1)")]                   public string Type { get; set; }

        [Column("name", TypeName = "varchar(255)")]              public string Name { get; set; } 

        [Column("route", TypeName = "varchar(20)")]              public string Route { get; set; }

        [Column("unit", TypeName = "varchar(160)")]              public string Unit { get; set; }	

        [Column("dose", TypeName = "varchar(40)")]               public string Dose { get; set; }

        [Column("med_notes", TypeName = "text")]                 public string MedNotes { get; set; }

        [Column("order_date", TypeName = "char(14)")]            public string OrderDate { get; set; }

        [Column("give_date", TypeName = "char(14)")]             public string GiveDate { get; set; }

        [Column("give_sysdate", TypeName = "char(14)")]          public string GiveSysDate { get; set; }

        [Column("order_for_usr", TypeName = "int")]              public int OrderForUser { get; set; }

        [Column("order_usr", TypeName = "int")]                  public int OrderUser { get; set; }

        [Column("give_usr", TypeName = "int")]                   public int? GiveUser { get; set; }

        [Column("iv_type", TypeName = "varchar(25)")]            public string IVType { get; set; }

        [Column("iv_site", TypeName = "int")]                    public int? IVSite { get; set; }

        [Column("iv_location", TypeName = "varchar(255)")]       public string IVLocation { get; set; }

        [Column("cpt_losecslink", TypeName = "int")]              public int? CptLosecsLink { get; set; }

        [Column("indication", TypeName = "varchar(80)")]         public string Indication { get; set; }

        [Column("iv_edit", TypeName = "char(1)")]                public string IVEdit { get; set; }

        [Column("data_source", TypeName = "char(1)")]            public string DataSource { get; set; }

        [Column("emar_patient_order_id", TypeName = "bigint")]   public long EmarPatientOrderId { get; set; }
    }
}
