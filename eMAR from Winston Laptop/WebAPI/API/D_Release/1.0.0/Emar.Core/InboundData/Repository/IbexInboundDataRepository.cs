using Emar.Core.InboundData.Model;
using Emar.Core.InboundData.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.Sites.Repository;
using Emar.Core.Users.Repository;
using Emar.Data;
using Emar.Data.Entities;
using Emar.Data.IbexEntities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace Emar.Core.InboundData.Repository
{
    public class IbexInboundDataRepository : IIbexInboundDataRepository
    {
        private readonly IbexContext _ibexContext;
        private readonly EmarContext _emarContext;
        private readonly IUserRepository _userRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<IbexInboundDataRepository> _logger;

        public IbexInboundDataRepository(IbexContext ibexContext, EmarContext emarContext, 
            IUserRepository userRepository, ISiteRepository siteRepository, IOrderRepository orderRepository, ILogger<IbexInboundDataRepository> logger)
        {
            _ibexContext = ibexContext;
            _emarContext = emarContext;
            _userRepository = userRepository;
            _siteRepository = siteRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public EmarUpdateQueueMaintenance GetNextQueueRecordToProcess(
            ref NextQueueRecordToProcessDto nextQueueRecordToProcessDto)
        {
            List<EmarUpdateQueueMaintenance> result;

            if (nextQueueRecordToProcessDto == null)
                result = _ibexContext.EmarUpdateQueueMaintenances
                    .FromSqlInterpolated($"EXEC dbo.emar_update_queue_maintenance").ToList();
            else
            {
                var highId = nextQueueRecordToProcessDto.HighestQueueIdWhenQuerying;
                var type = nextQueueRecordToProcessDto.RecordType.ToString();
                var exId = nextQueueRecordToProcessDto.RecordExternalId;
                result = _ibexContext.EmarUpdateQueueMaintenances
                    .FromSqlInterpolated($"EXEC dbo.emar_update_queue_maintenance {highId}, {type}, {exId}")
                    .ToList();
            }

            if (result.Count == 0)
                return null;
            return result[0];
        }

        public EmarUsersRetrieveView GetUser(string externalId)
        {
            if (!int.TryParse(externalId, out int id))
            {
                _logger.LogError(
                    $"Found [external_id] in [emar_update_queue] for [entity] = 'users' ({externalId}) which was not an integer.");
                return null;
            }

            var usersRetrieve = _ibexContext.EmarUsersRetrieveViews.FirstOrDefault(u => u.Id == id);

            return usersRetrieve;
        }

        public EmarPatientsRetrieveView GetPatient(string ibex)
        {
            return _ibexContext.EmarPatientsRetrieveViews.FirstOrDefault(u => u.ExternalId == ibex);
        }

        public EmarArchivedPatientsRetrieveView GetArchivedPatient(string ibex)
        {
            return _ibexContext.EmarArchivedPatientsRetrieveViews.FirstOrDefault(u => u.ExternalId == ibex);
        }

        public List<EmarPatientIndicatorsRetrieveView> GetPatientIndicators(string ibex)
        {
            byte site = 0;
            if (ibex.IndexOf("|") > 0)
            {
                string[] parts = ibex.Split("|");
                byte.TryParse(parts[0], out site);
                ibex = parts[1];
            }
            return _ibexContext.EmarPatientIndicatorsRetrieveViews.Where(u => u.ExternalId == ibex && (site == 0 || u.ExternalSiteId == site)).ToList();
        }
        
        public List<EmarPersonnelRetrieveView> GetPatientUsers(string ibex)
        {
            byte site = 0;
            if (ibex.IndexOf("|") > 0)
            {
                string[] parts = ibex.Split("|");
                byte.TryParse(parts[0], out site);
                ibex = parts[1];
            }
            return _ibexContext.EmarPersonnelRetrieveViews.Where(u => u.ExternalId == ibex && (site == 0 || u.ExternalSiteId == site)).ToList();
        }

        public void AddPatientAllergies(Patient emarPatientEntity)
        {
            // SQL Injection-proofing
            Debug.Assert(long.TryParse(emarPatientEntity.ExternalPatientId, out var bogus));

            var patientAllergies =
                _ibexContext.EmarPatientAllergiesRetrieveSps.FromSqlInterpolated(
                        $"EXEC dbo.emar_patient_allergies_retrieve_sp {emarPatientEntity.ExternalPatientId}")
                    .ToList();

            if (!patientAllergies.Any())
                return;

            // Call the eMAR stored procedure that will try and get a MedicationId for each existing allergy
            List<UpdateDrugIdItem> medicationItems = GetEmarDrugMappingToPatientDrugs(patientAllergies);

            // Getting the time zone so we can convert the allergy adddatetime and changedatetime
            var siteTimeZone = _siteRepository.GetSiteTimeZone(emarPatientEntity.SiteId);

            // loop through the allergies, assigning the mapped data to each patient allergy
            // Imitated code (from $/Database/C_Development/C_DevTrunk/Scripts/Data-Loader/phi_data/patient_allergies.sql
            //      update [target] set    
            //      [medication_id] = [source].[medication_id]
            //      from[#medication_items] [source]
            //          inner join[#patient_allergies] [target] on [source].[ndc] = [target].[ndc]
            //          and[source].[brand_name] = [target].[name]
            //      and[source].[drug_id] = [target].[drug_id]
            //      where[source].[medication_id] > 0;
            foreach (var allergy in patientAllergies)
            {
                var mappedAllergy = medicationItems
                    .FirstOrDefault(m => m.Ndc == allergy.Ndc
                                         && m.BrandName == allergy.Name
                                         && m.DrugId == allergy.DrugId
                                         && m.MedicationId > 0);

                // When the user has an Id of 0, or if the userid isn't recognized in external_ids, 0 will be entered
                // If the user ID is Null, NULL will be entered
                emarPatientEntity.PatientAllergies.Add(new PatientAllergy
                {
                    Class = allergy.Class,
                    Category = allergy.Category,
                    InternalDrugId = allergy.InternalDrugId,
                    Name = allergy.Name,
                    AllergyDrugId = allergy.AllergyDrugId,
                    IsActive = allergy.IsActive ?? true,
                    Comment = allergy.Comment,
                    Schedule = allergy.Schedule,
                    Reaction = allergy.Reaction,
                    Severity = allergy.Severity,
                    Source = allergy.Source,
                    ParentDrugId = allergy.ParentDrugId,
                    ParentDrugName = allergy.ParentDrugName,
                    AddUserId = allergy.AddUserId.HasValue
                        ? _userRepository.GetInternalUserId(allergy.AddUserId.Value.ToString())
                        : (int?) null,
                    AddDatetime = string.IsNullOrWhiteSpace(allergy.AddDatetime)
                        ? null
                        : IbexInboundMapper.IbexTimeStampToDateTimeOffset(siteTimeZone, allergy.AddDatetime,
                            "allergy.AddDatetime", _logger),
                    ChangeUserId = allergy.ChangeUserId.HasValue
                        ? _userRepository.GetInternalUserId(allergy.ChangeUserId.Value.ToString())
                        : (int?) null,
                    ChangeDatetime = string.IsNullOrWhiteSpace(allergy.ChangeDatetime)
                        ? null
                        : IbexInboundMapper.IbexTimeStampToDateTimeOffset(siteTimeZone, allergy.ChangeDatetime,
                            "allergy.ChangeDatetime", _logger),
                    ActionStatus = allergy.ActionStatus,
                    InformationSource = allergy.InformationSource,
                    PersonNumber = allergy.PersonNumber,
                    AccountNumber = allergy.AccountNumber,
                    MedicationId = mappedAllergy?.MedicationId,
                });
            }
        }

        public void AddPatientHomeMedications(Patient emarPatientEntity)
        {
            // SQL Injection-proofing
            Debug.Assert(long.TryParse(emarPatientEntity.ExternalPatientId, out var bogus));

            var patientMedications =
                _ibexContext.EmarPatientMedicationsRetrieveSps.FromSqlInterpolated(
                        $"EXEC dbo.emar_patient_medications_retrieve_sp {emarPatientEntity.ExternalPatientId}")
                    .ToList();

            if (!patientMedications.Any())
                return;

            // Call the eMAR stored procedure that will try and get a MedicationId for each existing home medications
            List<UpdateDrugIdItem> medicationItems = GetEmarDrugMappingToPatientDrugs(patientMedications);

            // Getting the time zone so we can convert the medication adddatetime and changedatetime
            var siteTimeZone = _siteRepository.GetSiteTimeZone(emarPatientEntity.SiteId);

            // Get the code share sites so we can convert the incoming medication unit and route code strings to the appropriate id values
            var codeShareSites = _orderRepository.GetCodeShareSites(emarPatientEntity.SiteId).ToList();

            var medicationRoutesSiteId = 
                codeShareSites.FirstOrDefault(c => c.Entity == OrderRepository.CodeShareEntity.MedicationRoute)?.SharedSiteId;

            var medicationUnitsSiteId =
                codeShareSites.FirstOrDefault(c => c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?.SharedSiteId;

            var medicationRoutes = _emarContext.Sites
                .Include(i => i.MedicationRoutes)
                .FirstOrDefault(s => s.Id == medicationRoutesSiteId)?.MedicationRoutes;

            var medicationUnits = _emarContext.Sites
                .Include(i => i.MedicationUnits)
                .FirstOrDefault(s => s.Id == medicationUnitsSiteId)?.MedicationUnits;

            foreach (var medication in patientMedications)
            {
                var mappedMedication = medicationItems
                    .FirstOrDefault(
                           m => m.Ndc == medication.Ndc
                        && m.BrandName == medication.Name
                        && m.DrugId == medication.DrugId
                        && m.MedicationId > 0
                    );

                // Dose handling:
                // Dose is freetext in EDPC, and may come through in "dose unit" format, or as a number that we can actually use in eMAR.
                // If incoming dose is non-numeric, split it apart on the first space we find, then try to use the first piece of the 
                // split as the dose and try to use the second piece as a coded medication unit if the incoming med doesn't have a unit
                // and we can match what we've found to a unit in the database.
                // When incoming dose is non-numeric, change the incoming comment to "Entered Dose: [dose]. [incoming comment]"
                decimal? decimalDose = null;

                int? medicationUnitId =
                    medication.Unit != null && medicationUnits != null ?
                        medicationUnits.FirstOrDefault(c => c.Code == medication.Unit)?.Id : null;

                if (!string.IsNullOrWhiteSpace(medication.Dose)) {
                    decimal value;
                    if (decimal.TryParse(medication.Dose, out value))
                    {
                        decimalDose = value;
                    } else 
                    {
                        // Updating comment here before attempting post-split unit matching, so we don't lose the original
                        // value in the case where we can't match to a unit in the database but can parse out a decimal dose.
                        medication.Comment = ("Entered Dose: " + medication.Dose + ". " + medication.Comment?.ToString()).Trim();
                        if (medication.Comment.Length > 255)
                        {
                            medication.Comment = medication.Comment.Substring(0, 255);
                        }

                        string[] doseParts = medication.Dose.Split(' ', 2);
                        if (doseParts.Length == 2)
                        {
                            if (decimal.TryParse(doseParts[0], out value))
                            {
                                decimalDose = value;
                            }

                            // Try to use the second piece to match to a unit in the database if we did not initially receive a matching unit.
                            if (medicationUnitId == null)
                            {
                                medicationUnitId = medicationUnits.FirstOrDefault(
                                    c => c.Code == doseParts[1] || c.Name == doseParts[1]
                                )?.Id;
                            }
                        }
                    }
                }

                // When the user has an Id of 0, or if the userid isn't recognized in external_ids, 0 will be entered
                // If the user ID is Null, NULL will be entered
                emarPatientEntity.PatientHomeMedications.Add(new PatientHomeMedication
                {
                    Class = medication.Class,
                    Category = medication.Category,
                    InternalDrugId = medication.InternalDrugId,
                    Name = medication.Name,
                    MedicationDrugId = medication.MedicationDrugId,
                    IsActive = medication.IsActive ?? true,
                    Comment = medication.Comment,
                    LastTakenNote = medication.LastTakenNote,
                    Schedule = medication.Schedule,
                    MedicationUnitId = medicationUnitId,
                    MedicationRouteId = medication.Route != null && medicationRoutes != null ? 
                        medicationRoutes.FirstOrDefault(c => c.Code == medication.Route)?.Id : null,
                    Dose = decimalDose,
                    Reaction = medication.Reaction,
                    ParentDrugId = medication.ParentDrugId,
                    ParentDrugName = medication.ParentDrugName,
                    AddUserId = medication.AddUserId.HasValue
                        ? _userRepository.GetInternalUserId(medication.AddUserId.Value.ToString())
                        : (int?)null,
                    AddDatetime = string.IsNullOrWhiteSpace(medication.AddDatetime)
                        ? null
                        : IbexInboundMapper.IbexTimeStampToDateTimeOffset(siteTimeZone, medication.AddDatetime,
                            "medication.AddDatetime", _logger),
                    ChangeUserId = medication.ChangeUserId.HasValue
                        ? _userRepository.GetInternalUserId(medication.ChangeUserId.Value.ToString())
                        : (int?)null,
                    ChangeDatetime = string.IsNullOrWhiteSpace(medication.ChangeDatetime)
                        ? null
                        : IbexInboundMapper.IbexTimeStampToDateTimeOffset(siteTimeZone, medication.ChangeDatetime,
                            "medication.ChangeDatetime", _logger),
                    ActionStatus = medication.ActionStatus,
                    MedicationId = mappedMedication?.MedicationId,                    
                });
            }
        }

        private List<UpdateDrugIdItem> GetEmarDrugMappingToPatientDrugs<T>(List<T> drugs) where T : EmarPatientDrugsRetrieveSp
        {
            // Create the table that will be passed into the SP
            var table = new DataTable();
            table.Columns.AddRange(new[]
                {
                    new DataColumn("ndc", typeof(string)),
                    new DataColumn("drug_id", typeof(string)),
                    new DataColumn("name", typeof(string))
                }
            );

            foreach (var drug in drugs.ToList()
                .GroupBy(a => new {ndc = a.Ndc, drug_id = a.DrugId, name = a.Name}))
            {
                var row = table.NewRow();
                row[0] = drug.Key.ndc ?? "";
                row[1] = drug.Key.drug_id ?? "";
                row[2] = drug.Key.name;
                table.Rows.Add(row);
            }

            var param = new SqlParameter
            {
                SqlDbType = SqlDbType.Structured,
                ParameterName = "@MedicationItems",
                TypeName = "dbo.MedicationItemsType",
                Value = table
            };

            // For the sp, call a wrapper to emar.dbo.update_medication_id_list
            return _emarContext.UpdateMedicationIdItemList
                .FromSqlInterpolated($"EXECUTE dbo.update_medication_id_list_wrapper {param}").ToList();
        }
    }
}