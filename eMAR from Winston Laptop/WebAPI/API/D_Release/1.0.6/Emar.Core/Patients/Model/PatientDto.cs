using System;
using System.Collections.Generic;
using Emar.Core.ExternalIds.Model;
using Emar.Core.Helpers;
using Emar.Core.HomeMedications.Model;
using Emar.Core.Orders.Model;
using Emar.Core.Sites.Model;

namespace Emar.Core.Patients.Model
{
    public class PatientDto
    {
        public long Id { get; set; }

        public ExternalIdDto ExternalId { get; set; }

        public bool Active { get; set; }

        public bool MyPatient { get; set; }

        private string _accountNumber;
        public string AccountNumber
        {
            get => _accountNumber?.Trim();
            set => _accountNumber = value?.Trim();
        }

        string _medicalRecordNumber;
        public string MedicalRecordNumber
        {
            get => _medicalRecordNumber?.Trim();
            set => _medicalRecordNumber = value?.Trim();
        }

        #region name

        string _firstName;
        public string FirstName
        {
            get => _firstName?.Trim();
            set => _firstName = value?.Trim();
        }

        string _middleName;
        public string MiddleName
        {
            get => _middleName?.Trim();
            set => _middleName = value?.Trim();
        }


        string _lastName;
        public string LastName
        {
            get => _lastName?.Trim();
            set => _lastName = value?.Trim();
        }

        string _nameSuffix;
        public string NameSuffix
        {
            get => _nameSuffix?.Trim();
            set => _nameSuffix = value?.Trim();
        }

        public string FullName => NameHelper.GetDisplayName(_firstName, _middleName, _lastName, _nameSuffix);

        #endregion

        string _gender;
        public string Gender
        {
            get => _gender?.Trim();
            set => _gender = value?.Trim();
        }

        public DateTime? DateOfBirth { get; set; }

        public int? Age { get; set; }

        string _ageUnits;
        public string AgeUnits
        {
            get => _ageUnits?.Trim();
            set => _ageUnits = value?.Trim();
        }

        string _complaint;
        public string Complaint
        {
            get => _complaint?.Trim();
            set => _complaint = value?.Trim();
        }

        public decimal? HeightInCm { get; set; }

        public decimal? WeightInKg { get; set; }

        #region geography - room, ward, department
        public int SiteId { get; set; }

        string _departmentCode;
        public string DepartmentCode
        {
            get => _departmentCode?.Trim();
            set => _departmentCode = value?.Trim();
        }

        string _wardCode;
        public string WardCode
        {
            get => _wardCode?.Trim();
            set => _wardCode = value?.Trim();
        }

        string _roomBedCode;
        public string RoomBedCode
        {
            get => _roomBedCode?.Trim();
            set => _roomBedCode = value?.Trim();
        }
        #endregion

        string _urgency;
        public string Urgency
        {
            get => _urgency?.Trim();
            set => _urgency = value?.Trim();
        }

        string _urgencyColor;
        public string UrgencyColor
        {
            get => _urgencyColor?.Trim();
            set => _urgencyColor = value?.Trim();
        }

        public bool? NameAlert { get; set; }

        public bool? WithdrawConsent { get; set; }

        public DateTimeOffset? VisitStartDatetime { get; set; }

        public DateTimeOffset? DeactivationDatetime { get; set; }

        #region vital signs
        public DateTimeOffset? VsDatetime { get; set; }

        string _vsBloodPressureIndicator;
        public string VsBloodPressureIndicator
        {
            get => _vsBloodPressureIndicator?.Trim();
            set => _vsBloodPressureIndicator = value?.Trim();
        }

        string _vsSystolic;
        public string VsSystolic
        {
            get => _vsSystolic?.Trim();
            set => _vsSystolic = value?.Trim();
        }

        string _vsDiastolic;
        public string VsDiastolic
        {
            get => _vsDiastolic?.Trim();
            set => _vsDiastolic = value?.Trim();
        }

        string _vsPulseIndicator;
        public string VsPulseIndicator
        {
            get => _vsPulseIndicator?.Trim();
            set => _vsPulseIndicator = value?.Trim();
        }

        string _vsPulse;
        public string VsPulse
        {
            get => _vsPulse?.Trim();
            set => _vsPulse = value?.Trim();
        }

        string _vsMapLevel;
        public string VsMapLevel
        {
            get => _vsMapLevel?.Trim();
            set => _vsMapLevel = value?.Trim();
        }

        string _vsMap;
        public string VsMap
        {
            get => _vsMap?.Trim();
            set => _vsMap = value?.Trim();
        }

        string _vsRespiratoryIndicator;
        public string VsRespiratoryIndicator
        {
            get => _vsRespiratoryIndicator?.Trim();
            set => _vsRespiratoryIndicator = value?.Trim();
        }

        string _vsRespiratory;
        public string VsRespiratory
        {
            get => _vsRespiratory?.Trim();
            set => _vsRespiratory = value?.Trim();
        }

        string _vsTemperatureIndicator;
        public string VsTemperatureIndicator
        {
            get => _vsTemperatureIndicator?.Trim();
            set => _vsTemperatureIndicator = value?.Trim();
        }

        string _vsTemperature;
        public string VsTemperature
        {
            get => _vsTemperature?.Trim();
            set => _vsTemperature = value?.Trim();
        }

        string _vsEndTidalLevel;
        public string VsEndTidalLevel
        {
            get => _vsEndTidalLevel?.Trim();
            set => _vsEndTidalLevel = value?.Trim();
        }

        string _vsEndTidal;
        public string VsEndTidal
        {
            get => _vsEndTidal?.Trim();
            set => _vsEndTidal = value?.Trim();
        }

        string _vsOxygenSaturationIndicator;
        public string VsOxygenSaturationIndicator
        {
            get => _vsOxygenSaturationIndicator?.Trim();
            set => _vsOxygenSaturationIndicator = value?.Trim();
        }

        string _vsOxygenSaturation;
        public string VsOxygenSaturation
        {
            get => _vsOxygenSaturation?.Trim();
            set => _vsOxygenSaturation = value?.Trim();
        }

        string _vsPainScaleIndicator;
        public string VsPainScaleIndicator
        {
            get => _vsPainScaleIndicator?.Trim();
            set => _vsPainScaleIndicator = value?.Trim();
        }

        string _vsPainScale;
        public string VsPainScale
        {
            get => _vsPainScale?.Trim();
            set => _vsPainScale = value?.Trim();
        }

        string _customNumber;
        public string CustomNumber
        {
            get => _customNumber?.Trim();
            set => _customNumber = value?.Trim();
        }

        string _personNumber;
        public string PersonNumber
        {
            get => _personNumber?.Trim();
            set => _personNumber = value?.Trim();
        }

        string _patientImageSrc;
        public string PatientImageSrc
        {
            get => _patientImageSrc?.Trim();
            set => _patientImageSrc = value?.Trim();
        }
        #endregion

        //private List<Allergy> Allergies { get; set; }
        //private List<CurrentMedication> HomeMedications { get; set; }
        public IEnumerable<PatientOrderDto> Orders { get; set; }

        public SiteDto Site { get; set; }

        public IEnumerable<PatientIndicatorDto> PatientIndicators { get; set; }
        public List<PatientAllergyDto> PatientAllergies { get; set; }
        public List<HomeMedicationDto> HomeMedications { get; set; }
        public List<PatientProblemDto> PatientProblems { get; set; }
    }
}
