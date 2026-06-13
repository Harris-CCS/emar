using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    /// <summary>
    /// For the retrieve from the [emar_patient_allergies_retrieve_sp] SP
    /// </summary>
    public class EmarPatientAllergiesRetrieveSp : EmarPatientDrugsRetrieveSp
    {
        [Column("allergy_drug_id", TypeName = "varchar(9)")]
        public string AllergyDrugId { get; set; }

        [Column("severity", TypeName = "varchar(80)")]
        public string Severity { get; set; }
    }
}
