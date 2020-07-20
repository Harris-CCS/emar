using System;
using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Model
{
    public class PatientDto
    {
        public long Id { get; set; }
        public bool Active { get; set; }
        public string AccountNumber { get; set; }
        #region name
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NameSuffix { get; set; }
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
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string AgeUnits { get; set; }
        public string ChiefComplaint { get; set; }
        public decimal? HeightInCm { get; set; }
        public decimal? WeightInKg { get; set; }
        #region geography - room, ward, department
        public short SiteId { get; set; }
        public string DepartmentCode { get; set; }
        public string WardCode { get; set; }
        public string RoomBedCode { get; set; }
        #endregion
        public string UrgencyColor { get; set; }
        public bool? NameAlert { get; set; }
        public bool? WithdrawConsent { get; set; }
        #region vital signs
        public DateTimeOffset? VsDatetime { get; set; }
        public string VsBloodPressureIndicator { get; set; }
        public string VsSystolic { get; set; }
        public string VsDiastolic { get; set; }
        public string VsPulseIndicator { get; set; }
        public string VsPulse { get; set; }
        public string VsMapLevel { get; set; }
        public string VsMap { get; set; }
        public string VsRespiratoryIndicator { get; set; }
        public string VsRespiratory { get; set; }
        public string VsTemperatureIndicator { get; set; }
        public string VsTemperature { get; set; }
        public string VsEndTidalLevel { get; set; }
        public string VsEndTidal { get; set; }
        public string VsOxygenSaturationIndicator { get; set; }
        public string VsOxygenSaturation { get; set; }
        public string VsPainScaleIndicator { get; set; }
        public string VsPainScale { get; set; }
        #endregion

        //private List<Allergy> Allergies { get; set; }
        //private List<CurrentMedication> HomeMedications { get; set; }
        public IEnumerable<PatientOrder>? Orders { get; set; }

        public Site Site { get; set; }
        public string SiteName
        {
            get => Site?.Name;
        }
    }
}
