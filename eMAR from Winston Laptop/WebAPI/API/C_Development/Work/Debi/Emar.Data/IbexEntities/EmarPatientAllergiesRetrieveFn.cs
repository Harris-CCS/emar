using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    /// <summary>
    /// For the retrieve from the [emar_patient_allergies_retrieve_fn] SQL TVF
    /// </summary>
    public class EmarPatientAllergiesRetrieveFn : EmarPatientDrugsRetrieveFnBase
    {
        [Column("allergy_drug_id", TypeName = "varchar(9)")]
        public string AllergyDrugId { get; set; }

        [Column("severity", TypeName = "varchar(80)")]
        public string Severity { get; set; }
    }
}
