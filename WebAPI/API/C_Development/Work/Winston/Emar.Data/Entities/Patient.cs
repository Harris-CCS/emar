using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patients")]
    public class Patient
    {
        public Patient()
        {
            PatientOrders = new HashSet<PatientOrder>();
        }

        [Key]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("is_active", TypeName = "bool")]
        public bool Active { get; set; } = true;

        [Column("site_id", TypeName = "int"), Required]
        public int SiteId { get; set; }

        [Column("medical_record_number", TypeName = "varchar(25)")]
        public string MedicalRecordNumber { get; set; }

        [Column("account_number", TypeName = "varchar(25)")]
        public string AccountNumber { get; set; }

        [Required]
        [Column("first_name", TypeName = "nvarchar(35)")]
        public string FirstName { get; set; }

        [Column("middle_name", TypeName = "nvarchar(35)")]
        public string MiddleName { get; set; }

        [Column("last_name", TypeName = "nvarchar(35)"), Required]
        public string LastName { get; set; }

        [Column("name_suffix", TypeName = "nvarchar(25)")]
        public string NameSuffix { get; set; }

        [Required]
        [Column("gender", TypeName = "varchar(10)")]
        public string Gender { get; set; }

        [Column("date_of_birth", TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [Column("age", TypeName = "tinyint")]
        public short? Age { get; set; }

        [Column("age_units", TypeName = "char(1)")]
        public string AgeUnits { get; set; }

        [Column("complaint", TypeName = "varchar(80)")]
        public string ChiefComplaint { get; set; }

        [Column("height_in_cm", TypeName = "numeric(6,2")]
        public decimal? HeightInCm { get; set; }

        [Column("weight_in_kg", TypeName = "numeric(6,2)")]
        public decimal? WeightInKg { get; set; }

        [Column("room_bed_code", TypeName = "varchar(15)")]
        public string RoomBedCode { get; set; }

        [Column("ward_code", TypeName = "varchar(15)")]
        public string WardCode { get; set; }

        [Column("department_code", TypeName = "varchar(15)")]
        public string DepartmentCode { get; set; }

        [Column("urgency", TypeName = "varchar(50)")]
        public string Urgency { get; set; }

        [Column("urgency_color", TypeName = "varchar(25)")]
        public string UrgencyColor { get; set; }

        [Column("name_alert", TypeName = "bit"), Required]
        public bool NameAlert { get; set; }

        [Column("withdraw_consent", TypeName = "bit"), Required]
        public bool WithdrawConsent { get; set; }

        [Column("vs_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? VsDatetime { get; set; }

        [Column("vs_blood_pressure_indicator", TypeName = "char(1)")]
        public string VsBloodPressureIndicator { get; set; }

        [Column("vs_systolic", TypeName = "char(14)")]
        public string VsSystolic { get; set; }

        [Column("vs_diastolic", TypeName = "char(14)")]
        public string VsDiastolic { get; set; }

        [Column("vs_pulse_indicator", TypeName = "char(1)")]
        public string VsPulseIndicator { get; set; }

        [Column("vs_pulse", TypeName = "char(14)")]
        public string VsPulse { get; set; }

        [Column("vs_map_level", TypeName = "char(1)")]
        public string VsMapLevel { get; set; }

        [Column("vs_map", TypeName = "varchar(14)")]
        public string VsMap { get; set; }

        [Column("vs_respiratory_indicator", TypeName = "char(1)")]
        public string VsRespiratoryIndicator { get; set; }

        [Column("vs_respiratory", TypeName = "char(14)")]
        public string VsRespiratory { get; set; }

        [Column("vs_temperature_indicator", TypeName = "char(1)")]
        public string VsTemperatureIndicator { get; set; }

        [Column("vs_temperature", TypeName = "char(14)")]
        public string VsTemperature { get; set; }

        [Column("vs_end_tidal_level", TypeName = "char(1)")]
        public string VsEndTidalLevel { get; set; }

        [Column("vs_end_tidal", TypeName = "varchar(14)")]
        public string VsEndTidal { get; set; }

        [Column("vs_oxygen_saturation_indicator", TypeName = "char(1)")]
        public string VsOxygenSaturationIndicator { get; set; }

        [Column("vs_oxygen_saturation", TypeName = "varchar(50)")]
        public string VsOxygenSaturation { get; set; }

        [Column("vs_pain_scale_indicator", TypeName = "char(1)")]
        public string VsPainScaleIndicator { get; set; }

        [Column("vs_pain_scale", TypeName = "char(14)")]
        public string VsPainScale { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.Patients))]
        public virtual Site Site { get; set; }

        [InverseProperty("Patient")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

		// No foreign key yet....
        //[InverseProperty("Patient")]
        //public virtual ICollection<PatientCartOrder>? PatientCartOrders { get; set; }
    }
}
