using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_allergies")]
    public partial class PatientAllergy
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("patient_id")]
        public long? PatientId { get; set; }
        [Column("class")]
        [StringLength(32)]
        public string Class { get; set; }
        [Column("category")]
        [StringLength(32)]
        public string Category { get; set; }
        [Column("internal_drug_id")]
        [StringLength(32)]
        public string InternalDrugId { get; set; }
        [Column("ndc")]
        [StringLength(32)]
        public string Ndc { get; set; }
        [Column("drug_id")]
        [StringLength(32)]
        public string DrugId { get; set; }
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }
        [Column("alternate_name")]
        [StringLength(255)]
        public string AlternateName { get; set; }
        [Column("allergy_drug_id")]
        [StringLength(32)]
        public string AllergyDrugId { get; set; }
        [Column("is_active")]
        public bool IsActive { get; set; }
        [Column("comment")]
        [StringLength(255)]
        public string Comment { get; set; }
        [Column("schedule")]
        [StringLength(40)]
        public string Schedule { get; set; }
        [Column("reaction")]
        [StringLength(80)]
        public string Reaction { get; set; }
        [Column("severity")]
        [StringLength(80)]
        public string Severity { get; set; }
        [Column("parent_drug_id")]
        [StringLength(32)]
        public string ParentDrugId { get; set; }
        [Column("parent_drug_name")]
        [StringLength(255)]
        public string ParentDrugName { get; set; }
        [Column("add_user_id")]
        public int AddUserId { get; set; }
        [Column("add_datetime")]
        public DateTimeOffset? AddDatetime { get; set; }
        [Column("change_user_id")]
        public int ChangeUserId { get; set; }
        [Column("change_datetime")]
        public DateTimeOffset? ChangeDatetime { get; set; }
        [Column("action_status")]
        [StringLength(1)]
        public string ActionStatus { get; set; }
        [Column("information_source")]
        [StringLength(25)]
        public string InformationSource { get; set; }
        [Column("person_number")]
        [StringLength(25)]
        public string PersonNumber { get; set; }
        [Column("account_number")]
        [StringLength(25)]
        public string AccountNumber { get; set; }

        [ForeignKey(nameof(AddUserId))]
        [InverseProperty(nameof(User.PatientAllergiesAddUser))]
        public virtual User AddUser { get; set; }
        [ForeignKey(nameof(ChangeUserId))]
        [InverseProperty(nameof(User.PatientAllergiesChangeUser))]
        public virtual User ChangeUser { get; set; }
        [ForeignKey(nameof(PatientId))]
        [InverseProperty(nameof(Entities.Patient.PatientAllergies))]
        public virtual Patient Patient { get; set; }
    }
}
