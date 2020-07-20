using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patients")]
    public class Patient
    {
        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public short SiteId { get; set; }

        string medicalRecordNumber;
        [Column("medical_record_number", TypeName = "varchar(25)")]
        public string MedicalRecordNumber
        {
            get => medicalRecordNumber;
            set => medicalRecordNumber = value?.Trim();
        }

        string accountNumber;
        [Column("account_number", TypeName = "varchar(25)")]
        public string AccountNumber
        {
            get => accountNumber;
            set => accountNumber = value?.Trim();
        }

        string firstName;
        [Column("first_name", TypeName = "nvarchar(35)"), Required]
        public string FirstName
        {
            get => firstName;
            set => firstName = value?.Trim();
        }

        string middleName;
        [Column("middle_name", TypeName = "nvarchar(35)")]
        public string MiddleName
        {
            get => middleName;
            set => middleName = value?.Trim();
        }

        string lastName;
        [Column("last_name", TypeName = "nvarchar(35)"), Required]
        public string LastName
        {
            get => lastName;
            set => lastName = value?.Trim();
        }

        string nameSuffix;
        [Column("name_suffix", TypeName = "nvarchar(25)")]
        public string NameSuffix
        {
            get => nameSuffix;
            set => nameSuffix = value?.Trim();
        }

        string gender;
        [Column("gender", TypeName = "varchar(10)"), Required]
        public string Gender
        {
            get => gender;
            set => gender = value?.Trim();
        }

        [Column("date_of_birth", TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [Column("age", TypeName = "tinyint")]
        public short? Age { get; set; }

        string ageUnits;
        [Column("age_units", TypeName = "char(1)")]
        public string AgeUnits
        {
            get => ageUnits;
            set => ageUnits = value?.Trim();
        }

        string chiefComplaint;
        [Column("complaint", TypeName = "varchar(80)")]
        public string ChiefComplaint
        {
            get => chiefComplaint;
            set => chiefComplaint = value?.Trim();
        }

        [Column("height_in_cm", TypeName = "numeric(6,2")]
        public decimal? HeightInCm { get; set; }

        [Column("weight_in_kg", TypeName = "numeric(6,2)")]
        public decimal? WeightInKg { get; set; }

        string departmentCode;
        [Column("department_code", TypeName = "varchar(15)")]
        public string DepartmentCode
        {
            get => departmentCode;
            set => departmentCode = value?.Trim();
        }

        string wardCode;
        [Column("ward_code", TypeName = "varchar(15)")]
        public string WardCode
        {
            get => wardCode;
            set => wardCode = value?.Trim();
        }

        string roomBedCode;
        [Column("room_bed_code", TypeName = "varchar(15)")]
        public string RoomBedCode
        {
            get => roomBedCode;
            set => roomBedCode = value?.Trim();
        }

        string urgency;
        [Column("urgency", TypeName = "varchar(50)")]
        public string Urgency
        {
            get => urgency;
            set => urgency = value?.Trim();
        }

        string urgencyColor;
        [Column("urgency_color", TypeName = "varchar(25)")]
        public string UrgencyColor
        {
            get => urgencyColor;
            set => urgencyColor = value?.Trim();
        }

        [Column("name_alert", TypeName = "bit"), Required]
        public bool NameAlert { get; set; }

        [Column("withdraw_consent", TypeName = "bit"), Required]
        public bool WithdrawConsent { get; set; }

        [Column("vs_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? VsDatetime { get; set; }

        string vsBloodPressureIndicator;
        [Column("vs_blood_pressure_indicator", TypeName = "char(1)")]
        public string VsBloodPressureIndicator
        {
            get => vsBloodPressureIndicator;
            set => vsBloodPressureIndicator = value?.Trim();
        }

        string vsSystolic;
        [Column("vs_systolic", TypeName = "char(14)")]
        public string VsSystolic
        {
            get => vsSystolic;
            set => vsSystolic = value?.Trim();
        }

        string vsDiastolic;
        [Column("vs_diastolic", TypeName = "char(14)")]
        public string VsDiastolic
        {
            get => vsDiastolic;
            set => vsDiastolic = value?.Trim();
        }

        string vsPulseIndicator;
        [Column("vs_pulse_indicator", TypeName = "char(1)")]
        public string VsPulseIndicator
        {
            get => vsPulseIndicator;
            set => vsPulseIndicator = value?.Trim();
        }

        string vsPulse;
        [Column("vs_pulse", TypeName = "char(14)")]
        public string VsPulse
        {
            get => vsPulse;
            set => vsPulse = value?.Trim();
        }

        string vsMapLevel;
        [Column("vs_map_level", TypeName = "char(1)")]
        public string VsMapLevel
        {
            get => vsMapLevel;
            set => vsMapLevel = value?.Trim();
        }

        string vsMap;
        [Column("vs_map", TypeName = "varchar(14)")]
        public string VsMap
        {
            get => vsMap;
            set => vsMap = value?.Trim();
        }

        string vsRespiratoryIndicator;
        [Column("vs_respiratory_indicator", TypeName = "char(1)")]
        public string VsRespiratoryIndicator
        {
            get => vsRespiratoryIndicator;
            set => vsRespiratoryIndicator = value?.Trim();
        }

        string vsRespiratory;
        [Column("vs_respiratory", TypeName = "char(14)")]
        public string VsRespiratory
        {
            get => vsRespiratory;
            set => vsRespiratory = value?.Trim();
        }

        string vsTemperatureIndicator;
        [Column("vs_temperature_indicator", TypeName = "char(1)")]
        public string VsTemperatureIndicator
        {
            get => vsTemperatureIndicator;
            set => vsTemperatureIndicator = value?.Trim();
        }

        string vsTemperature;
        [Column("vs_temperature", TypeName = "char(14)")]
        public string VsTemperature
        {
            get => vsTemperature;
            set => vsTemperature = value?.Trim();
        }

        string vsEndTidalLevel;
        [Column("vs_end_tidal_level", TypeName = "char(1)")]
        public string VsEndTidalLevel
        {
            get => vsEndTidalLevel;
            set => vsEndTidalLevel = value?.Trim();
        }

        string vsEndTidal;
        [Column("vs_end_tidal", TypeName = "varchar(14)")]
        public string VsEndTidal
        {
            get => vsEndTidal;
            set => vsEndTidal = value?.Trim();
        }

        string vsOxygenSaturationIndicator;
        [Column("vs_oxygen_saturation_indicator", TypeName = "char(1)")]
        public string VsOxygenSaturationIndicator
        {
            get => vsOxygenSaturationIndicator;
            set => vsOxygenSaturationIndicator = value?.Trim();
        }

        string vsOxygenSaturation;
        [Column("vs_oxygen_saturation", TypeName = "varchar(50)")]
        public string VsOxygenSaturation
        {
            get => vsOxygenSaturation;
            set => vsOxygenSaturation = value?.Trim();
        }

        string vsPainScaleIndicator;
        [Column("vs_pain_scale_indicator", TypeName = "char(1)")]
        public string VsPainScaleIndicator
        {
            get => vsPainScaleIndicator;
            set => vsPainScaleIndicator = value?.Trim();
        }

        string vsPainScale;
        [Column("vs_pain_scale", TypeName = "char(14)")]
        public string VsPainScale
        {
            get => vsPainScale;
            set => vsPainScale = value?.Trim();
        }


        //[Column("is_active", TypeName = "char(1)")]
        [NotMapped]
        public bool Active { get; set; } = true;

        [NotMapped]
        public IEnumerable<PatientOrder>? Orders { get; set; }

        [NotMapped]
        public Site Site { get; set; }

        [NotMapped]
        public string SiteName { get; set; }
    }
}
