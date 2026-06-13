using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_home_medications")]
    public partial class PatientHomeMedication
    {
        public PatientHomeMedication()
        {
            OrderInteractions = new HashSet<OrderInteraction>();
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("patient_id")]
        public long PatientId { get; set; }

        [Column("class")]
        [StringLength(32)]
        public string Class { get; set; }

        [Column("category")]
        [StringLength(32)]
        public string Category { get; set; }

        [Column("internal_drug_id")]
        [StringLength(32)]
        public string InternalDrugId { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int? MedicationId { get; set; }

        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("alternate_name")]
        [StringLength(255)]
        public string AlternateName { get; set; }

        [Column("dose", TypeName = "decimal(11, 2)")]
        public decimal? Dose { get; set; }
        [Column("medication_unit_id")]

        public int? MedicationUnitId { get; set; }
        [Column("medication_route_id")]
        public int? MedicationRouteId { get; set; }

        [Column("medication_drug_id")]
        [StringLength(32)]
        public string MedicationDrugId { get; set; }

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
        public int? AddUserId { get; set; }

        [Column("add_datetime")]
        public DateTimeOffset? AddDatetime { get; set; }

        [Column("change_user_id")]
        public int? ChangeUserId { get; set; }

        [Column("change_datetime")]
        public DateTimeOffset? ChangeDatetime { get; set; }

        [Column("action_status", TypeName = "char(1)")]
        public string ActionStatus { get; set; }

        [Column("last_taken_note", TypeName = "nvarchar(100)")]
        public string LastTakenNote { get; set; }


        [ForeignKey(nameof(AddUserId))]
        [InverseProperty(nameof(Entities.User.PatientHomeMedicationsAddUser))]
        public virtual User AddUser { get; set; }

        [ForeignKey(nameof(ChangeUserId))]
        [InverseProperty(nameof(Entities.User.PatientHomeMedicationsChangeUser))]
        public virtual User ChangeUser { get; set; }

        // For Foreign Key: fk__patient_home_medications__medications
        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.PatientHomeMedications))]
        public virtual Medication Medication { get; set; }

        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.PatientHomeMedications))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        [ForeignKey(nameof(MedicationUnitId))]
        [InverseProperty(nameof(Entities.MedicationUnit.PatientHomeMedications))]
        public virtual MedicationUnit MedicationUnit { get; set; }

        // For Foreign Key: fk__patients__patient_home_medications
        [ForeignKey(nameof(PatientId))]
        [InverseProperty(nameof(Entities.Patient.PatientHomeMedications))]
        public virtual Patient Patient { get; set; }

        [InverseProperty("PatientHomeMedication")]
        public virtual ICollection<OrderInteraction> OrderInteractions { get; set; }
    }
}