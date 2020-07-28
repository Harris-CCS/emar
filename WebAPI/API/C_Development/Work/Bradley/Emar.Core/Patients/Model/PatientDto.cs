using System;
using System.Collections.Generic;
using Emar.Core.Orders.Model;
using Emar.Core.Sites.Model;

namespace Emar.Core.Patients.Model
{
    public class PatientDto
    {
        public long Id { get; set; }

        public bool Active { get; set; }

        private string _accountNumber;
        public string AccountNumber
        {
            get => _accountNumber?.Trim();
            set => _accountNumber = value?.Trim();
        }

        string medicalRecordNumber;
        public string MedicalRecordNumber
        {
            get => medicalRecordNumber?.Trim();
            set => medicalRecordNumber = value?.Trim();
        }
        #region name

        string firstName;
        public string FirstName
        {
            get => firstName?.Trim();
            set => firstName = value?.Trim();
        }

        string middleName;
        public string MiddleName
        {
            get => middleName?.Trim();
            set => middleName = value?.Trim();
        }


        string lastName;
        public string LastName
        {
            get => lastName?.Trim();
            set => lastName = value?.Trim();
        }

        string nameSuffix;
        public string NameSuffix
        {
            get => nameSuffix?.Trim();
            set => nameSuffix = value?.Trim();
        }

        public string FullName
        {
            get
            {
                var firstName = (FirstName ?? "").Trim();
                if (firstName.Length == 1)
                    firstName += ".";

                var middleName = (MiddleName ?? "").Trim();
                if (middleName.Length == 1)
                    middleName += ".";

                var ret = firstName;
                ret += (ret != "" && !string.IsNullOrWhiteSpace(middleName)) ? " " : "";
                ret += middleName;
                ret += (ret != "" && !string.IsNullOrWhiteSpace(LastName)) ? " " : "";
                ret += (LastName ?? "").Trim();
                ret += ((!string.IsNullOrWhiteSpace(ret) && !string.IsNullOrWhiteSpace(NameSuffix)) ? ", " : "") +
                    (NameSuffix ?? "").Trim();
                return ret;
            }
        }
        #endregion

        string gender;
        public string Gender
        {
            get => gender?.Trim();
            set => gender = value?.Trim();
        }

        public DateTime? DateOfBirth { get; set; }

        public int? Age { get; set; }

        string ageUnits;
        public string AgeUnits
        {
            get => ageUnits?.Trim();
            set => ageUnits = value?.Trim();
        }

        string complaint;
        public string ChiefComplaint 
        {
            get => complaint?.Trim();
            set => complaint = value?.Trim();
        }

        public decimal? HeightInCm { get; set; }

        public decimal? WeightInKg { get; set; }

        #region geography - room, ward, department
        public int SiteId { get; set; }

        string departmentCode;
        public string DepartmentCode
        {
            get => departmentCode?.Trim();
            set => departmentCode = value?.Trim();
        }

        string wardCode;
        public string WardCode
        {
            get => wardCode?.Trim();
            set => wardCode = value?.Trim();
        }

        string roomBedCode;
        public string RoomBedCode
        {
            get => roomBedCode?.Trim();
            set => roomBedCode = value?.Trim();
        }
        #endregion

        string urgency;
        public string Urgency
        {
            get => urgency?.Trim();
            set => urgency = value?.Trim();
        }

        string urgencyColor;
        public string UrgencyColor
        {
            get => urgencyColor?.Trim();
            set => urgencyColor = value?.Trim();
        }

        public bool? NameAlert { get; set; }

        public bool? WithdrawConsent { get; set; }

        #region vital signs
        public DateTimeOffset? VsDatetime { get; set; }

        string vsBloodPressureIndicator;
        public string VsBloodPressureIndicator
        {
            get => vsBloodPressureIndicator?.Trim();
            set => vsBloodPressureIndicator = value?.Trim();
        }

        string vsSystolic;
        public string VsSystolic
        {
            get => vsSystolic?.Trim();
            set => vsSystolic = value?.Trim();
        }

        string vsDiastolic;
        public string VsDiastolic
        {
            get => vsDiastolic?.Trim();
            set => vsDiastolic = value?.Trim();
        }

        string vsPulseIndicator;
        public string VsPulseIndicator
        {
            get => vsPulseIndicator?.Trim();
            set => vsPulseIndicator = value?.Trim();
        }

        string vsPulse;
        public string VsPulse
        {
            get => vsPulse?.Trim();
            set => vsPulse = value?.Trim();
        }

        string vsMapLevel;
        public string VsMapLevel
        {
            get => vsMapLevel?.Trim();
            set => vsMapLevel = value?.Trim();
        }

        string vsMap;
        public string VsMap
        {
            get => vsMap?.Trim();
            set => vsMap = value?.Trim();
        }

        string vsRespiratoryIndicator;
        public string VsRespiratoryIndicator
        {
            get => vsRespiratoryIndicator?.Trim();
            set => vsRespiratoryIndicator = value?.Trim();
        }

        string vsRespiratory;
        public string VsRespiratory
        {
            get => vsRespiratory?.Trim();
            set => vsRespiratory = value?.Trim();
        }

        string vsTemperatureIndicator;
        public string VsTemperatureIndicator
        {
            get => vsTemperatureIndicator?.Trim();
            set => vsTemperatureIndicator = value?.Trim();
        }

        string vsTemperature;
        public string VsTemperature
        {
            get => vsTemperature?.Trim();
            set => vsTemperature = value?.Trim();
        }

        string vsEndTidalLevel;
        public string VsEndTidalLevel
        {
            get => vsEndTidalLevel?.Trim();
            set => vsEndTidalLevel = value?.Trim();
        }

        string vsEndTidal;
        public string VsEndTidal
        {
            get => vsEndTidal?.Trim();
            set => vsEndTidal = value?.Trim();
        }

        string vsOxygenSaturationIndicator;
        public string VsOxygenSaturationIndicator
        {
            get => vsOxygenSaturationIndicator?.Trim();
            set => vsOxygenSaturationIndicator = value?.Trim();
        }

        string vsOxygenSaturation;
        public string VsOxygenSaturation
        {
            get => vsOxygenSaturation?.Trim();
            set => vsOxygenSaturation = value?.Trim();
        }

        string vsPainScaleIndicator;
        public string VsPainScaleIndicator
        {
            get => vsPainScaleIndicator?.Trim();
            set => vsPainScaleIndicator = value?.Trim();
        }

        string vsPainScale;
        public string VsPainScale
        {
            get => vsPainScale?.Trim();
            set => vsPainScale = value?.Trim();
        }
        #endregion

        //private List<Allergy> Allergies { get; set; }
        //private List<CurrentMedication> HomeMedications { get; set; }
        public IEnumerable<PatientOrderDto>? Orders { get; set; }

        public SiteDto Site { get; set; }

        public string SiteName
        {
            get => Site?.Name;
        }
    }
}
