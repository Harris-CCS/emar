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
            PatientCartOrders = new HashSet<PatientCartOrder>();
            PatientOrders = new HashSet<PatientOrder>();
            PatientIndicators = new HashSet<PatientIndicator>();
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("site_id", TypeName = "int")]
        public long SiteId { get; set; }
        [Column("medical_record_number")]
        [StringLength(25)]
        public string MedicalRecordNumber { get; set; }
        [Column("account_number")]
        [StringLength(25)]
        public string AccountNumber { get; set; }
        [Required]
        [Column("last_name")]
        [StringLength(35)]
        public string LastName { get; set; }
        [Required]
        [Column("first_name")]
        [StringLength(35)]
        public string FirstName { get; set; }
        [Column("middle_name")]
        [StringLength(35)]
        public string MiddleName { get; set; }
        [Column("name_suffix")]
        [StringLength(25)]
        public string NameSuffix { get; set; }
        [Required]
        [Column("gender")]
        [StringLength(10)]
        public string Gender { get; set; }
        [Column("date_of_birth", TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }
        [Column("age")]
        public byte? Age { get; set; }
        [Column("age_units")]
        [StringLength(1)]
        public string AgeUnits { get; set; }
        [Column("complaint")]
        [StringLength(80)]
        public string Complaint { get; set; }
        [Column("height_in_cm", TypeName = "numeric(6, 2)")]
        public decimal? HeightInCm { get; set; }
        [Column("weight_in_kg", TypeName = "numeric(6, 2)")]
        public decimal? WeightInKg { get; set; }
        [Column("room_bed_code")]
        [StringLength(15)]
        public string RoomBedCode { get; set; }
        [Column("ward_code")]
        [StringLength(15)]
        public string WardCode { get; set; }
        [Column("department_code")]
        [StringLength(15)]
        public string DepartmentCode { get; set; }
        [Column("urgency")]
        [StringLength(50)]
        public string Urgency { get; set; }
        [Column("urgency_color")]
        [StringLength(25)]
        public string UrgencyColor { get; set; }
        [Column("name_alert")]
        public bool NameAlert { get; set; }
        [Column("withdraw_consent")]
        public bool WithdrawConsent { get; set; }
        [Column("vs_datetime")]
        public DateTimeOffset? VsDatetime { get; set; }
        [Column("vs_blood_pressure_indicator")]
        [StringLength(1)]
        public string VsBloodPressureIndicator { get; set; }
        [Column("vs_systolic")]
        [StringLength(14)]
        public string VsSystolic { get; set; }
        [Column("vs_diastolic")]
        [StringLength(14)]
        public string VsDiastolic { get; set; }
        [Column("vs_pulse_indicator")]
        [StringLength(1)]
        public string VsPulseIndicator { get; set; }
        [Column("vs_pulse")]
        [StringLength(14)]
        public string VsPulse { get; set; }
        [Column("vs_map_level")]
        [StringLength(1)]
        public string VsMapLevel { get; set; }
        [Column("vs_map")]
        [StringLength(14)]
        public string VsMap { get; set; }
        [Column("vs_respiratory_indicator")]
        [StringLength(1)]
        public string VsRespiratoryIndicator { get; set; }
        [Column("vs_respiratory")]
        [StringLength(14)]
        public string VsRespiratory { get; set; }
        [Column("vs_temperature_indicator")]
        [StringLength(1)]
        public string VsTemperatureIndicator { get; set; }
        [Column("vs_temperature")]
        [StringLength(14)]
        public string VsTemperature { get; set; }
        [Column("vs_end_tidal_level")]
        [StringLength(1)]
        public string VsEndTidalLevel { get; set; }
        [Column("vs_end_tidal")]
        [StringLength(14)]
        public string VsEndTidal { get; set; }
        [Column("vs_oxygen_saturation_indicator")]
        [StringLength(1)]
        public string VsOxygenSaturationIndicator { get; set; }
        [Column("vs_oxygen_saturation")]
        [StringLength(50)]
        public string VsOxygenSaturation { get; set; }
        [Column("vs_pain_scale_indicator")]
        [StringLength(1)]
        public string VsPainScaleIndicator { get; set; }
        [Column("vs_pain_scale")]
        [StringLength(14)]
        public string VsPainScale { get; set; }
        [Column("is_active")]
        public bool Active { get; set; }
        [Column("custom_number")]
        [StringLength(25)]
        public string CustomNumber { get; set; }
        [Column("person_number")]
        [StringLength(25)]
        public string PersonNumber { get; set; }

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.Patients))]
        public virtual Site Site { get; set; }

        [InverseProperty("Patient")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        [InverseProperty("Patient")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }

        [InverseProperty("Patient")]
        public virtual ExternalIdEntity ExternalIds { get; set; }

        [InverseProperty("Patient")]
        public virtual ICollection<PatientIndicator> PatientIndicators { get; set; }
    }
}
