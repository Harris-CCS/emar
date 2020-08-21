using System;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Sites.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Model.Mappings
{
    public static class PatientMapper
    {
        public static PatientDto MapPatient(Patient pt)
        {
            if (pt == null)
                return null;

            var dateFormat = pt.Site.SiteOptions.FirstOrDefault(si => si.Option.Name == AppConstants.LongDateFormat).OptionValue;
            
            PatientDto patientDto = new PatientDto
            {
                Id = pt.Id,
                SiteId = pt.SiteId,
                Active = pt.Active,
                FirstName = pt.FirstName.Trim(),
                MiddleName = (pt.MiddleName == null) ? pt.MiddleName : pt.MiddleName.Trim(),
                LastName = pt.LastName.Trim(),
                NameSuffix = (pt.NameSuffix == null) ? pt.NameSuffix : pt.NameSuffix.Trim(),
                AccountNumber = pt.AccountNumber,
                MedicalRecordNumber = pt.MedicalRecordNumber,
                Gender = pt.Gender,
                DateOfBirth = pt.DateOfBirth,
                BirthDate = DateTimeHelper.GetDate(pt.DateOfBirth, dateFormat),
                Age = pt.Age,
                AgeUnits = pt.AgeUnits,
                Complaint = pt.Complaint,
                HeightInCm = pt.HeightInCm,
                WeightInKg = pt.WeightInKg,
                DepartmentCode = pt.DepartmentCode,
                WardCode = pt.WardCode,
                RoomBedCode = pt.RoomBedCode,
                Urgency = pt.Urgency,
                UrgencyColor = pt.UrgencyColor,
                NameAlert = pt.NameAlert,
                WithdrawConsent = pt.WithdrawConsent,
                VsDatetime = pt.VsDatetime,
                VsDatetimeDate = DateTimeHelper.GetDate(pt.VsDatetime, dateFormat),
                VsDatetimeTime = DateTimeHelper.GetTime(pt.VsDatetime),
                VsBloodPressureIndicator = pt.VsBloodPressureIndicator,
                VsSystolic = pt.VsSystolic,
                VsDiastolic = pt.VsDiastolic,
                VsPulseIndicator = pt.VsPulseIndicator,
                VsPulse = pt.VsPulse,
                VsMapLevel = pt.VsMapLevel,
                VsMap = pt.VsMap,
                VsRespiratoryIndicator = pt.VsRespiratoryIndicator,
                VsRespiratory = pt.VsRespiratory,
                VsTemperatureIndicator = pt.VsTemperatureIndicator,
                VsTemperature = pt.VsTemperature,
                VsEndTidalLevel = pt.VsEndTidalLevel,
                VsEndTidal = pt.VsEndTidal,
                VsOxygenSaturationIndicator = pt.VsOxygenSaturationIndicator,
                VsOxygenSaturation = pt.VsOxygenSaturation,
                VsPainScaleIndicator = pt.VsPainScaleIndicator,
                VsPainScale = pt.VsPainScale,
                CustomNumber = pt.CustomNumber,
                PersonNumber = pt.PersonNumber,
                VisitStartDatetime = pt.VisitStartDatetime,
                DeactivationDatetime = pt.DeactivationDatetime,
                PatientImageSrc = EmarHttpContext.AppBaseUrl + "/" + AppConstants.ImagesRoute + "/patients/" + pt.Id.ToString(),
                Orders = pt.PatientOrders?.Select(OrderMapper.MapOrder).ToList(),
                Site = SiteMapper.MapSite(pt.Site),
                PatientIndicators = pt.PatientIndicators?.Select(MapPatientIndicators).ToList()
            };

            // Calculate the age if the date-of-birth is present
            if (pt.DateOfBirth == null) return patientDto;
            var dateOfBirth = (DateTime)pt.DateOfBirth;
            var ageTimeSpan = DateTime.Now.Subtract(dateOfBirth);
            if (ageTimeSpan.TotalDays < 180)
            {
                patientDto.Age = (int)Math.Truncate(ageTimeSpan.TotalDays);
                patientDto.AgeUnits = "days";
            }
            else if (ageTimeSpan.TotalDays < 700)
            {
                patientDto.Age = (DateTime.Now.Day < dateOfBirth.Day ? -1 : 0) +
                          DateTime.Now.Month - dateOfBirth.Month +
                          (DateTime.Now.Year - dateOfBirth.Year) * 12;
                patientDto.AgeUnits = "months";
            }
            else
            {
                patientDto.Age = (DateTime.Now.Month < dateOfBirth.Month || (DateTime.Now.Month == dateOfBirth.Month) &&
                              DateTime.Now.Day < dateOfBirth.Day
                                  ? -1
                                  : 0) +
                          (DateTime.Now.Year - dateOfBirth.Year);
                patientDto.AgeUnits = "years";
            }
            return patientDto;
        }

        public static PatientIndicatorDto MapPatientIndicators(PatientIndicator indicator)
        {
            if (indicator == null)
            {
                return null;
            }

            PatientIndicatorDto indicatorDto = new PatientIndicatorDto
            {
                Id = indicator.Id,
                PatientId = indicator.PatientId,
                OrdinalPosition = indicator.OrdinalPosition,
                Code = indicator.Code,
                Type = indicator.Type,
                Description = indicator.Description,
                ImageName = indicator.ImageName,
                ImageSrc = String.IsNullOrEmpty(indicator.ImageName)
                            ? null
                            : EmarHttpContext.AppBaseUrl + "/" + AppConstants.ImagesRoute + "/patients/" + indicator.PatientId.ToString() + "/indicators/" + indicator.ImageName
            };

            return indicatorDto;
        }
    }
}
