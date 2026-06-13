using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_orders")]
    public class PatientOrder
    {
        public PatientOrder()
        {
            OrderAdministrations = new HashSet<OrderAdministration>();
            OrderEvents = new HashSet<OrderEvent>();
            OrderInteractions = new HashSet<OrderInteraction>();
            OrderReactions = new HashSet<OrderReaction>();
        }

        [Key]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("patient_id", TypeName = "bigint"), Required]
        public long PatientId { get; set; }

        [Column("add_user_id", TypeName = "int"), Required]
        public int AddUserId { get; set; }

        [Column("add_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset AddDatetime { get; set; }

        [Column("order_physician_user_id", TypeName = "int"), Required]
        public int OrderingPhysicianId { get; set; }

        [Column("begin_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset BeginDatetime { get; set; }

        [Column("end_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? EndDateTime { get; set; }

        [Column("medication_id", TypeName = "int")]
        public int MedicationId { get; set; }

        [Column("dose", TypeName = "decimal(11,2)")]
        public decimal? Dose { get; set; }

        [Column("medication_unit_id")]
        public int? MedicationUnitId { get; set; }

        [Column("medication_route_id", TypeName = "int")]
        public int? MedicationRouteId { get; set; }

        [Column("priority", TypeName = "tinyint"), Required]
        public byte Priority { get; set; }

        [Column("frequency_schedule_id", TypeName = "int")]
        public int? FrequencyScheduleId { get; set; }

        [Column("prn", TypeName = "bit"), Required]
        public bool Prn { get; set; }

        [Column("point_in_time", TypeName = "bit"), Required]
        public bool PointInTime { get; set; }

        [Column("order_status", TypeName = "varchar(25)"), Required]
        public string OrderStatus { get; set; }

        [Column("order_notes", TypeName = "nvarchar(MAX)")]
        public string OrderNotes { get; set; }

        [Column("antimicrobial_indication_id", TypeName = "int")]
        public int? AntimicrobialIndicationId { get; set; }

        [Column("antimicrobial_indication_text", TypeName = "nvarchar(255)")]
        public string AntimicrobialIndicationText { get; set; }

        [Column("pharmacy_verification_status", TypeName = "tinyint"), Required]
        public byte PharmacyVerificationStatus { get; set; }

        [Column("patient_problem_id", TypeName = "bigint")]
        public long? PatientProblemId { get; set; }

        [Column("duration", TypeName = "int")]
        public int? Duration { get; set; }

        [Column("duration_unit_id", TypeName = "int")]
        public int? DurationUnitId { get; set; }

        [Column("ndc")]
        public string? Ndc { get; set; }


        [ForeignKey(nameof(AddUserId))]
        [InverseProperty(nameof(User.PatientOrdersAddUser))]
        public virtual User AddUser { get; set; }

        // For Foreign Key: FK_patient_orders_antimicrobial_indications_antimicrobial_indication_id 
        [ForeignKey(nameof(AntimicrobialIndicationId))]
        [InverseProperty(nameof(Entities.AntimicrobialIndication.PatientOrders))]
        public virtual AntimicrobialIndication AntimicrobialIndication { get; set; }

        // For Foreign Key: fk__patient_orders__duration_units
        [ForeignKey(nameof(DurationUnitId))]
        [InverseProperty(nameof(Entities.DurationUnit.PatientOrders))]
        public virtual DurationUnit DurationUnit { get; set; }

        [ForeignKey(nameof(FrequencyScheduleId))]
        [InverseProperty(nameof(Entities.FrequencySchedule.PatientOrders))]
        public virtual FrequencySchedule FrequencySchedule { get; set; }

        // For Foreign Key: fk__patient_orders__medications
        [ForeignKey(nameof(MedicationId))]
        [InverseProperty(nameof(Entities.Medication.PatientOrders))]
        public virtual Medication Medication { get; set; }

        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.PatientOrders))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        [ForeignKey(nameof(MedicationUnitId))]
        [InverseProperty(nameof(Entities.MedicationUnit.PatientOrders))]
        public virtual MedicationUnit MedicationUnit { get; set; }

        [ForeignKey(nameof(OrderingPhysicianId))]
        [InverseProperty(nameof(User.PatientOrdersOrderPhysicianUser))]
        public virtual User OrderPhysicianUser { get; set; }

        [ForeignKey(nameof(PatientId))]
        [InverseProperty(nameof(Entities.Patient.PatientOrders))]
        public virtual Patient Patient { get; set; }
        // For Foreign Key: fk__patient_orders__patient_problems
        [ForeignKey(nameof(PatientProblemId))]
        [InverseProperty(nameof(Entities.PatientProblem.PatientOrders))]
        public virtual PatientProblem PatientProblem { get; set; }


        [InverseProperty("PatientOrder")]
        public virtual ICollection<OrderAdministration> OrderAdministrations { get; set; }

        [InverseProperty("PatientOrder")]
        public virtual ICollection<OrderEvent> OrderEvents { get; set; }

        [InverseProperty("PatientOrder")]
        public virtual ICollection<OrderInteraction> OrderInteractions { get; set; }

        [InverseProperty("PatientOrder")]
        public virtual ICollection<OrderReaction> OrderReactions { get; set; }


        [ForeignKey("OrderId")]
        [InverseProperty("PatientOrder")]
        public virtual ICollection<AllergyReactionView> AllergyReactionsView { get; set; }
    }
}