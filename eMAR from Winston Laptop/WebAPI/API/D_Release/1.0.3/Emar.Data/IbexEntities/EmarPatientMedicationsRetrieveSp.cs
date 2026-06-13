using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    /// <summary>
    /// For the retrieve from the [emar_patient_medications_retrieve_sp] SP
    /// </summary>
    public class EmarPatientMedicationsRetrieveSp : EmarPatientDrugsRetrieveSp
    {
        [Column("medication_drug_id", TypeName = "varchar(9)")]
        public string MedicationDrugId { get; set; }

        [Column("dose", TypeName = "varchar(20)")]
        public string Dose { get; set; }

        [Column("route", TypeName = "varchar(20)")]
        public string Route { get; set; }

        [Column("unit", TypeName = "varchar(20)")]
        public string Unit { get; set; }

        [Column("last_taken_note", TypeName = "varchar(20)")]
        public string LastTakenNote { get; set; }
    }
}
