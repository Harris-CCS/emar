using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("med_details")]
    public class MedicationDetails
    {
        public MedicationDetails()
        {

        }

        [Column("id", TypeName = "int"), Key] public int Id { get; set; }

        [Column("ibex", TypeName = "char(14)")] public string Ibex { get; set; }

        [Column("site", TypeName = "smallint")] public int Site { get; set; }

        [Column("losecs", TypeName = "int")] public int Losecs { get; set; }

        [Column("brand_name", TypeName = "varchar(255)")] public string BrandName { get; set; }

        [Column("active_name", TypeName = "varchar(max)")] public string ActiveName { get; set; }

        [Column("drug_route", TypeName = "varchar(255)")] public string DrugRoute { get; set; }

        [Column("drug_form", TypeName = "varchar(255)")] public string DrugForm { get; set; }

        [Column("drug_strength", TypeName = "varchar(255)")] public string DrugStrength { get; set; }

        [Column("drug_db_type", TypeName = "varchar(1)")] public string DrugDbType { get; set; }

        [Column("active_id", TypeName = "varchar(9)")] public string ActiveId { get; set; }

        [Column("drug_id", TypeName = "varchar(32)")] public string DrugId { get; set; }

        [Column("packaging_id", TypeName = "varchar(32)")] public string PackagingId { get; set; }

        [Column("drug_category_id", TypeName = "varchar(8)")] public string DrugCategoryId { get; set; }

        [Column("type", TypeName = "char(1)")] public string Type { get; set; }

        [Column("emar_medication_id", TypeName = "int")] public int EmarMedicationId { get; set; }
    }
}

