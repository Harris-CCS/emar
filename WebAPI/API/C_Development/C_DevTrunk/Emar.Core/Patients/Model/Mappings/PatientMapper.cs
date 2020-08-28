using System;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.HomeMedications.Model;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Sites.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Model.Mappings
{
    public static class PatientMapper
    {
        public static PatientDto MapPatient(Patient pt, string dateFormat)
        {
            if (pt == null)
                return null;

            PatientDto patientDto = new PatientDto
            {
                DateFormat = dateFormat,
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
                //BirthDate = DateTimeHelper.GetDate(pt.DateOfBirth, dateFormat),
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
                //VsDatetimeDate = DateTimeHelper.GetDate(pt.VsDatetime, dateFormat),
                //VsDatetimeTime = DateTimeHelper.GetTime(pt.VsDatetime),
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
                Orders = pt.PatientOrders?.Select(o => OrderMapper.MapOrder(o, dateFormat)).ToList(),
                Site = SiteMapper.MapSite(pt.Site),
                PatientIndicators = pt.PatientIndicators?.Select(MapPatientIndicator).ToList(),
                PatientAllergies = pt.PatientAllergies?.Select(MapPatientAllergy).ToList(),
                HomeMedications = pt.PatientHomeMedications?.Select(MapHomeMedication).ToList()
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

        private static PatientIndicatorDto MapPatientIndicator(PatientIndicator indicator)
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

        private static PatientAllergyDto MapPatientAllergy(PatientAllergy allergy)
        {
            if (allergy == null)
                return null;

            var retDto = new PatientAllergyDto
            {
                Id = allergy.Id,
                PatientId = allergy.PatientId,
                Class = allergy.Class,
                Category = allergy.Category,
                InternalDrugId = allergy.InternalDrugId,
                Ndc = allergy.Ndc,
                DrugId = allergy.DrugId,
                Name = allergy.Name,
                AlternateName = allergy.AlternateName,
                AllergyDrugId = allergy.AllergyDrugId,
                IsActive = allergy.IsActive,
                Comment = allergy.Comment,
                Schedule = allergy.Schedule,
                Reaction = allergy.Reaction,
                Severity = allergy.Severity,
                ParentDrugId = allergy.ParentDrugId,
                ParentDrugName = allergy.ParentDrugName,
                //AddUserId = allergy.AddUserId,
                //AddUser = UserMapper.MapUser(allergy.AddUser),
                //AddDatetime = allergy.AddDatetime,
                //ChangeUserId = allergy.ChangeUserId,
                //ChangeUser = UserMapper.MapUser(allergy.ChangeUser),
                //ChangeDatetime = allergy.ChangeDatetime,
                ActionStatus = allergy.ActionStatus,
                InformationSourceCode = allergy.InformationSource
            };

            return retDto;
        }

        private static HomeMedicationDto MapHomeMedication(PatientHomeMedication dbObj)
        {
            if (dbObj == null)
                return null;

            var retDto = new HomeMedicationDto
            {
                Id = dbObj.Id,
                PatientId = dbObj.PatientId ?? 0,
                Class = dbObj.Class,
                Category = dbObj.Category,
                InternalDrugId = dbObj.InternalDrugId,
                Ndc = dbObj.Ndc,
                DrugId = dbObj.DrugId,
                Name = dbObj.Name,
                AlternateName = dbObj.AlternateName,
                MedicationDrugId = dbObj.MedicationDrugId,
                IsActive = dbObj.IsActive,
                Comment = dbObj.Comment,
                Schedule = dbObj.Schedule,
                Reaction = dbObj.Reaction,
                Severity = dbObj.Severity,
                ParentDrugName = dbObj.ParentDrugName,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                MedicationUnit = MedicationMapper.MapMedicationUnit(dbObj.MedicationUnit),
                MedicationRouteId = dbObj.MedicationRouteId,
                MedicationRoute = MedicationMapper.MapMedicationRoute(dbObj.MedicationRoute)

                //AddUserId = allergy.AddUserId,
                //AddUser = UserMapper.MapUser(allergy.AddUser),
                //AddDatetime = allergy.AddDatetime,
                //ChangeUserId = allergy.ChangeUserId,
                //ChangeUser = UserMapper.MapUser(allergy.ChangeUser),
                //ChangeDatetime = allergy.ChangeDatetime,
            };

            if (retDto.MedicationRoute == null && retDto.MedicationRouteId != null)
                retDto.MedicationRoute = new MedicationRouteDto {Id = retDto.MedicationRouteId.Value};

            if (retDto.MedicationUnit == null && retDto.MedicationUnitId != null)
                retDto.MedicationUnit = new MedicationUnitDto { Id = retDto.MedicationUnitId.Value };

            return retDto;
        }
    }
}
