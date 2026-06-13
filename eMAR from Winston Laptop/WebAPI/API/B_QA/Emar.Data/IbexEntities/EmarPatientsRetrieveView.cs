using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.IbexEntities
{
    [Table("emar_patients_retrieve_view")]
    public class EmarPatientsRetrieveView
    {
        [Key]
        [Column("external_id", TypeName = "char(14)"), Required]
        public string ExternalId { get; set; }

        [Column("external_site_id", TypeName = "tinyint")]
        public byte ExternalSiteId { get; set; }

        [Column("medical_record_number", TypeName = "varchar(20)")]
        public string MedicalRecordNumber { get; set; }

        [Column("account_number", TypeName = "varchar(14)")]
        public string AccountNumber { get; set; }

        [Column("last_name", TypeName = "varchar(35)")]
        public string LastName { get; set; }

        [Column("first_name", TypeName = "varchar(35)")]
        public string FirstName { get; set; }

        [Column("middle_name", TypeName = "varchar(20)")]
        public string MiddleName { get; set; }

        [Column("name_suffix", TypeName = "varchar(4)")]
        public string NameSuffix { get; set; }

        [Column("gender", TypeName = "varchar(10)")]
        public string Gender { get; set; }

        [Column("date_of_birth", TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [Column("age", TypeName = "varchar(4)")]
        public string Age { get; set; }

        [Column("age_units", TypeName = "varchar(1)")]
        public string AgeUnits { get; set; }

        [Column("complaint", TypeName = "varchar(80)")]
        public string Complaint { get; set; }

        [Column("height_in_cm", TypeName = "varchar(41)")]
        public string HeightInCm { get; set; }

        [Column("weight_in_kg", TypeName = "varchar(41)")]
        public string WeightInKg { get; set; }

        [Column("room_bed_code", TypeName = "varchar(12)")]
        public string RoomBedCode { get; set; }

        [Column("ward_code", TypeName = "varchar(8)")]
        public string WardCode { get; set; }

        [Column("department_code", TypeName = "varchar(4)")]
        public string DepartmentCode { get; set; }

        [Column("urgency", TypeName = "varchar(255)")]
        public string Urgency { get; set; }

        [Column("urgency_color", TypeName = "varchar(7)"), Required]
        public string UrgencyColor { get; set; }

        [Column("name_alert", TypeName = "int")]
        public int NameAlert { get; set; }

        [Column("withdraw_consent", TypeName = "int")]
        public int WithdrawConsent { get; set; }

        [Column("vs_datetime", TypeName = "varchar(12)")]
        public string VsDatetime { get; set; }

        [Column("vs_blood_pressure_indicator", TypeName = "varchar(1)")]
        public string VsBloodPressureIndicator { get; set; }

        [Column("vs_systolic", TypeName = "varchar(14)")]
        public string VsSystolic { get; set; }

        [Column("vs_diastolic", TypeName = "varchar(14)")]
        public string VsDiastolic { get; set; }

        [Column("vs_pulse_indicator", TypeName = "varchar(1)")]
        public string VsPulseIndicator { get; set; }

        [Column("vs_pulse", TypeName = "varchar(14)")]
        public string VsPulse { get; set; }

        [Column("vs_map_level", TypeName = "varchar(1)")]
        public string VsMapLevel { get; set; }

        [Column("vs_map", TypeName = "varchar(14)")]
        public string VsMap { get; set; }

        [Column("vs_respiratory_indicator", TypeName = "varchar(1)")]
        public string VsRespiratoryIndicator { get; set; }

        [Column("vs_respiratory", TypeName = "varchar(14)")]
        public string VsRespiratory { get; set; }

        [Column("vs_temperature_indicator", TypeName = "varchar(1)")]
        public string VsTemperatureIndicator { get; set; }

        [Column("vs_temperature", TypeName = "varchar(14)")]
        public string VsTemperature { get; set; }

        [Column("vs_end_tidal_level", TypeName = "varchar(1)")]
        public string VsEndTidalLevel { get; set; }

        [Column("vs_end_tidal", TypeName = "varchar(14)")]
        public string VsEndTidal { get; set; }

        [Column("vs_oxygen_saturation_indicator", TypeName = "varchar(1)")]
        public string VsOxygenSaturationIndicator { get; set; }

        [Column("vs_oxygen_saturation", TypeName = "varchar(50)")]
        public string VsOxygenSaturation { get; set; }

        [Column("vs_pain_scale_indicator", TypeName = "varchar(1)")]
        public string VsPainScaleIndicator { get; set; }

        [Column("vs_pain_scale", TypeName = "varchar(14)")]
        public string VsPainScale { get; set; }

        [Column("custom_number", TypeName = "varchar(25)")]
        public string CustomNumber { get; set; }

        [Column("person_number", TypeName = "varchar(20)")]
        public string PersonNumber { get; set; }

        [Column("visit_start_datetime", TypeName = "varchar(14)")]
        public string VisitStartDatetime { get; set; }

        [Column("gender_system", TypeName = "varchar(5)")]
        public string GenderSystem { get; set; }

        [Column("is_active", TypeName = "int")]
        public int? IsActive { get; set; }
        
        [Column("disposition_type_code", TypeName = "varchar(4)")]
        public string? DispositionTypeCode { get; set; }
        
        [Column("disposition_code", TypeName = "varchar(4)")]
        public string? DispositionCode { get; set; }

        [Column("emar_pat", TypeName = "char(1)")]
        public string EmarPat { get; set; }
    }
}
