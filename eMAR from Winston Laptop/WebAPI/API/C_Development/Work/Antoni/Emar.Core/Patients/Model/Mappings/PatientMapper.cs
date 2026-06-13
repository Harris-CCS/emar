using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.ExternalIds.Model.Mappings;
using Emar.Core.Helpers;
using Emar.Core.HomeMedications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.ResourceParameters;
using Emar.Core.Sites.Model.Mappings;
using Emar.Core.Templates.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Patients.Model.Mappings
{
    public static class PatientMapper
    {
        public static PatientDto MapPatient(Patient pt, int userId)
        {
            if (pt == null)
                return null;

            var patientDto = new PatientDto
            {
                Id = pt.Id,
                SiteId = pt.SiteId,
                Active = pt.Active,
                MyPatient = pt.UserPatients.Any(p => p.UserId == userId),
                FirstName = pt.FirstName.Trim(),
                MiddleName = (pt.MiddleName == null) ? pt.MiddleName : pt.MiddleName.Trim(),
                LastName = pt.LastName.Trim(),
                NameSuffix = (pt.NameSuffix == null) ? pt.NameSuffix : pt.NameSuffix.Trim(),
                AccountNumber = pt.AccountNumber,
                MedicalRecordNumber = pt.MedicalRecordNumber,
                Gender = pt.Gender,
                DateOfBirth = pt.DateOfBirth,
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
                PatientImageSrc = EmarHttpContext.AppBaseUrl + "/" + AppConstants.ImagesRoute + "/patients/" + pt.Id,
                Site = SiteMapper.MapSite(pt.Site),
                PatientIndicators = pt.PatientIndicators?.Select(MapPatientIndicator).ToList(),
                PatientProblems = pt.PatientProblems?.Select(MapPatientProblem).ToList(),
                ExternalId = ExternalIdMapper.MapExternalId(pt.ExternalId)
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

        public static PatientDto MapPatient(Patient pt, string drugDbVendor, OrderActionMapperHelper mapperHelper,
            List<CodeSharedId> codeShareSites, int userId)
        {
            if (pt == null)
                return null;

            var patientDto = MapPatient(pt, userId);
            patientDto.Orders = pt.PatientOrders?.Select(o => OrderMapper.MapOrder(o, drugDbVendor, mapperHelper, codeShareSites)).ToList();
            patientDto.PatientAllergies = pt.PatientAllergies?
                    .Select(a =>
                        MapPatientAllergy(
                            a,
                            codeShareSites.
                                FirstOrDefault(c =>
                                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                                .SharedSiteId))
                    .ToList();
            patientDto.HomeMedications = pt.PatientHomeMedications?.Select(h => MapHomeMedication(h, codeShareSites)).ToList();

            return patientDto;
        }

        private static PatientIndicatorDto MapPatientIndicator(PatientIndicator indicator)
        {
            if (indicator == null)
            {
                return null;
            }

            var indicatorDto = new PatientIndicatorDto
            {
                Id = indicator.Id,
                PatientId = indicator.PatientId,
                OrdinalPosition = indicator.OrdinalPosition,
                Code = indicator.Code,
                Type = indicator.Type,
                Description = indicator.Description,
                ImageName = indicator.ImageName,
                ImageSrc = string.IsNullOrEmpty(indicator.ImageName)
                            ? null
                            : EmarHttpContext.AppBaseUrl + "/" + AppConstants.ImagesRoute + "/patients/" + indicator.PatientId.ToString() + "/indicators/" + indicator.ImageName
            };

            return indicatorDto;
        }

        private static PatientAllergyDto MapPatientAllergy(PatientAllergy allergy, int? codeShareSiteMedicationUnit)
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
                MedicationId = allergy.MedicationId,
                Medication = MedicationMapper.MapMedication(allergy.Medication, codeShareSiteMedicationUnit),
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
                InformationSourceCode = allergy.InformationSource,
                PersonNumber = allergy.PersonNumber,
                AccountNumber = allergy.AccountNumber
            };

            return retDto;
        }

        private static HomeMedicationDto MapHomeMedication(PatientHomeMedication dbObj, List<CodeSharedId> codeShareSites)
        {
            if (dbObj == null)
                return null;

            var retDto = new HomeMedicationDto
            {
                Id = dbObj.Id,
                PatientId = dbObj.PatientId,
                Class = dbObj.Class,
                Category = dbObj.Category,
                InternalDrugId = dbObj.InternalDrugId,
                MedicationId = dbObj.MedicationId,
                Medication = dbObj.Medication != null
                    ? MedicationMapper.MapMedication(dbObj.Medication, null)
                    : null,
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
                MedicationUnit = dbObj.MedicationUnit != null
                                 && dbObj.Medication?.DrugId != "COMBO"
                                 //////&& dbObj.Medication.SiteId != -1
                                 && dbObj.MedicationUnit.SiteId == codeShareSites
                                     .FirstOrDefault(c =>
                                         c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                                     .SharedSiteId
                    ? OrderMapper.MapMedicationUnit(dbObj.MedicationUnit)
                    : null,
                MedicationRouteId = dbObj.MedicationRouteId,
                MedicationRoute = dbObj.MedicationRoute != null
                                  && dbObj.Medication?.DrugId != "COMBO"
                                  //////&& dbObj.Medication.SiteId != -1
                                  && dbObj.MedicationRoute.SiteId == codeShareSites
                                      .FirstOrDefault(c =>
                                          c.Entity == OrderRepository.CodeShareEntity.MedicationRoute)?
                                      .SharedSiteId
                    ? OrderMapper.MapMedicationRoute(dbObj.MedicationRoute)
                    : null,

                //AddUserId = allergy.AddUserId,
                //AddUser = UserMapper.MapUser(allergy.AddUser),
                //AddDatetime = allergy.AddDatetime,
                //ChangeUserId = allergy.ChangeUserId,
                //ChangeUser = UserMapper.MapUser(allergy.ChangeUser),
                //ChangeDatetime = allergy.ChangeDatetime,

                ActionStatus = dbObj.ActionStatus,
                LastTakenNote = dbObj.LastTakenNote
            };

            if (retDto.MedicationRoute == null && retDto.MedicationRouteId != null)
                retDto.MedicationRoute = new MedicationRouteDto { Id = retDto.MedicationRouteId.Value };

            if (retDto.MedicationUnit == null && retDto.MedicationUnitId != null)
                retDto.MedicationUnit = new MedicationUnitDto { Id = retDto.MedicationUnitId.Value };

            return retDto;
        }

        public static PatientProblemDto MapPatientProblem(PatientProblem dbObj)
        {
            if (dbObj == null)
                return null;

            var retDto = new PatientProblemDto
            {
                Id = dbObj.Id,
                PatientId = dbObj.PatientId,
                CodeSetName = dbObj.CodeSetName,
                CodeSetValue = dbObj.CodeSetValue,
                ProblemName = dbObj.ProblemName,
                DiagnosisType = dbObj.DiagnosisType
            };

            return retDto;
        }
    }
}