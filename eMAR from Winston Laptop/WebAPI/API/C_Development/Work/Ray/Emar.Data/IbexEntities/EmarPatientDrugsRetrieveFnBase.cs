using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    /// <summary>
    /// Base for the retrieve from the [emar_patient_allergies_retrieve_fn] or the 
    /// [emar_patient_medications_retrieve_fn]
    /// </summary>
    public class EmarPatientDrugsRetrieveFnBase
    {
        [Column("patient_id", TypeName = "varchar(20)")]
        public string PatientId { get; set; }

        [Column("internal_key", TypeName = "varchar(50)")]
        public string InternalKey { get; set; }

        [Column("class", TypeName = "varchar(12)")]
        public string Class { get; set; }

        [Column("category", TypeName = "varchar(12)")]
        public string Category { get; set; }

        [Column("internal_drug_id", TypeName = "varchar(9)")]
        public string InternalDrugId { get; set; }

        [Column("ndc", TypeName = "varchar(32)")]
        public string Ndc { get; set; }

        [Column("drug_id", TypeName = "varchar(25)"), Required]
        public string DrugId { get; set; }

        [Column("name", TypeName = "varchar(255)")]
        public string Name { get; set; }

        [Column("alternate_name", TypeName = "varchar(255)")]
        public string AlternateName { get; set; }

        [Column("is_active", TypeName = "bit")]
        public bool? IsActive { get; set; }

        [Column("comment", TypeName = "varchar(255)")]
        public string Comment { get; set; }

        [Column("schedule", TypeName = "varchar(40)")]
        public string Schedule { get; set; }

        [Column("reaction", TypeName = "varchar(80)")]
        public string Reaction { get; set; }

        [Column("source", TypeName = "varchar(80)")]
        public string Source { get; set; }

        [Column("parent_drug_id", TypeName = "varchar(255)")]
        public string ParentDrugId { get; set; }

        [Column("parent_drug_name", TypeName = "varchar(255)")]
        public string ParentDrugName { get; set; }

        [Column("add_user_id", TypeName = "int")]
        public int? AddUserId { get; set; }

        [Column("add_datetime", TypeName = "varchar(12)")]
        public string AddDatetime { get; set; }

        [Column("change_user_id", TypeName = "int")]
        public int? ChangeUserId { get; set; }

        [Column("change_datetime", TypeName = "varchar(12)")]
        public string ChangeDatetime { get; set; }

        [Column("action_status", TypeName = "varchar(1)")]
        public string ActionStatus { get; set; }

        [Column("information_source", TypeName = "varchar(25)")]
        public string InformationSource { get; set; }

        [Column("person_number", TypeName = "varchar(20)")]
        public string PersonNumber { get; set; }

        [Column("account_number", TypeName = "varchar(14)")]
        public string AccountNumber { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int? MedicationId { get; set; }

        [Column("match", TypeName = "nvarchar(255)")]
        public string Match { get; set; }
    }
}
