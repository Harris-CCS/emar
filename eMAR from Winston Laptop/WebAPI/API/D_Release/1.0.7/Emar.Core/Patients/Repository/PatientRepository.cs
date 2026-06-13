using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Emar.Core.Helpers;
using Emar.Core.InboundData.Repository;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Patients.Model;
using Emar.Core.ResourceParameters;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Emar.Core.Patients.Model.Constants;

namespace Emar.Core.Patients.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly EmarContext _context;
        private readonly IPropertyMappingService _propertyMappingService;
        private IIbexInboundDataRepository _inboundDataRepository;
        private readonly ILogger<PatientRepository> _logger;
        private readonly IOptionRepository _optionRepository;


        // <summary>
        //  Not sure why this particular parameter-less constructor was created, but it is screwing up the DI pipeline...
        // </summary>
        //public PatientRepository()
        //{
        //}

        public PatientRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService,
            IIbexInboundDataRepository inboundDataRepository, ILogger<PatientRepository> logger, IOptionRepository optionRepository)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _inboundDataRepository = inboundDataRepository;
            _logger = logger;
            _optionRepository = optionRepository;
        }

        public PagedList<Patient> GetPatients(PatientsResourceParameters resourceParameters, bool includeOrders)
        {
            Expression<Func<Patient, bool>> whereLambda = null;

            if (!resourceParameters.IncludeInactive)
            {
                whereLambda = //whereLambda.And(pt => pt.Active == true);
                    pt => pt.Active;
            }

            if (resourceParameters.SiteId != 0)
            {
                whereLambda = whereLambda.And(pt => pt.SiteId == resourceParameters.SiteId);
            }

            if (resourceParameters.DepartmentCode != null)
            {
                whereLambda = whereLambda.And(pt => pt.DepartmentCode == resourceParameters.DepartmentCode);
            }

            if (resourceParameters.WardCodes != null)
            {
                var wardCodes = resourceParameters.WardCodes.Split(",");

                whereLambda = whereLambda.And(pt => wardCodes.Contains(pt.WardCode));
            }

            if (resourceParameters.RoomBedCode != null)
            {
                whereLambda = whereLambda.And(pt => pt.RoomBedCode == resourceParameters.RoomBedCode);
            }

            if (resourceParameters.IncludeMyPatientsOnly)
            {
                whereLambda = whereLambda.And(pt => pt.UserPatients.Any(up => up.UserId == resourceParameters.UserId));
            } //end if

            //If we have a value for pharmacy verification status...
            //only pull patients who have one or more orders with the specified status.
            //Winston MUrdock, 04/13/2021.  EMAR-805.
            if (resourceParameters.PharmacyVerificationStatus != null)
            {
                whereLambda = whereLambda.And(pt => pt.PatientOrders.Any(po => po.PharmacyVerificationStatus == Convert.ToByte(resourceParameters.PharmacyVerificationStatus)));
            }

            var patients = GetPatients(whereLambda, resourceParameters.IncludeOrders || includeOrders);

            //Romel and I discovered that the UI is never sending an order by parameter to us.
            //So this is always null.
            //Therefore we're not addressing saving the user's order by preference into the DB for now.
            //We can address this later if we need to.
            //Winston Murdock, 06/15/2021.
            if (resourceParameters.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<PatientDto, Patient>();

                patients = patients
                    .AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }

            //EMAR-616.  Change the page size to allow enough patients
            //so that we don't show multiple pages.
            //Rather than changing PageResource.cs (which affects everywhere),
            //I'm just setting this 100 (Romel suggested that number) here.
            //Winston Murdock, 01/19/2021.
            //return PagedList<Patient>.Create(patients.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
            return PagedList<Patient>.Create(patients.AsQueryable(), resourceParameters.PageNumber, 100);
        }

        public Patient GetPatient(long? patientId, PatientsResourceParameters resourceParameters, bool includeOrders)
        {
            return GetPatients(patient => patient.Id == patientId, resourceParameters != null && resourceParameters.IncludeOrders || includeOrders)
                    .FirstOrDefault();
        }

        private IEnumerable<Patient> GetPatients(Expression<Func<Patient, bool>> wherePredicate, bool includeOrders = true)
        {
            //This was checking includeOrders, grabbing the patients with their order info,
            //and then ovewriting the return variable with the patients (excluding their order info).
            //To fix that, I put the second part (that only gets the patients and excludes their orders)
            //into the else.
            //I also included FrequencyType below FrequencySchedule.
            //Winston Murdock, 04/14/2021.  EMAR-805.

            //Also include the order events (really administration events, but order events is the table name)
            //and action for the administrations.
            //The UI needs this for something related to the follow-up action on an administration.
            //Winston Murdock. 11/03/2021.  PC-26741.
            List<Patient> patients = includeOrders
                ? _context.Patients
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.OrderAdministrations)
                        .ThenInclude(admin => admin.OrderEvents)
                        .ThenInclude(oe => oe.Action)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.Medication)
                            .ThenInclude(m => m.MedicationDetails)
                                .ThenInclude(md => md.MedicationUnit)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.MedicationRoute)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.MedicationUnit)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.FrequencySchedule)
                            .ThenInclude(fs => fs.FrequencyType)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.AddUser)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.OrderPhysicianUser)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.DurationUnit)
                    .Include(patient => patient.PatientIndicators)
                    .Include(p => p.PatientAllergies)
                    .Include(p => p.PatientHomeMedications)
                        .ThenInclude(h => h.MedicationUnit)
                    .Include(p => p.PatientHomeMedications)
                        .ThenInclude(h => h.MedicationRoute)
                    .Include(p => p.PatientProblems)
                    .Include(p => p.UserPatients)
                    .Where(wherePredicate)
                    .ToList()
                : _context.Patients
                    .Include(patient => patient.PatientIndicators)
                    .Include(p => p.PatientAllergies)
                    .Include(p => p.PatientHomeMedications)
                        .ThenInclude(h => h.MedicationUnit)
                    .Include(p => p.PatientHomeMedications)
                        .ThenInclude(h => h.MedicationRoute)
                    .Include(p => p.PatientProblems)
                    .Include(p => p.UserPatients)
                    .Where(wherePredicate)
                    .ToList();

            var extIds = patients
                .Join(_context.ExternalIds
                        .Where(x => x.Vendor == "pulsecheck" && x.Entity == "patients"),
                    p => p.Id,
                    x => x.InternalId,
                    (p, x) => x)
                .ToList();

            foreach (var p in patients)
            {
                p.ExternalId = extIds.FirstOrDefault(x => x.InternalId == p.Id);

                //Get the site option for indicator image here.
                //We need it in the patient mapper but cannot call out to the DB there.
                //Winston Murdock, 02/25/2022.  PC-26953
                string sIndicatorPath = _optionRepository.GetOption(p.SiteId, OptionNames.CUSTOM_INDICATORS_IMAGE_PATH);

                //For each patient indicator, set the image path to the site option page.
                //This way we have it in the mapper so that we can return the full path to the image to the UI.
                foreach (var pi in p.PatientIndicators)
                {
                    pi.ImagePath = sIndicatorPath;
                } //end foreach patient indicator
            } //end foreach patient

            return patients;
        }

        //public long? GetPatientId(long? patientId, PatientsResourceParameters resourceParameters)
        //{
        //    if (resourceParameters != null &&
        //        resourceParameters.ExtId1 != null &&
        //        resourceParameters.ExtId2 != null)
        //    {
        //        patientId = _context.ExternalIds
        //            .FirstOrDefault(xId => xId.ExternalId.Equals(resourceParameters.ExtId1 + "|" + resourceParameters.ExtId2) &&
        //                                     xId.Entity.ToLower().Equals(@"patients") &&
        //                                     xId.Vendor.ToLower().Equals(@"pulsecheck"))
        //            ?.InternalId;
        //    }

        //    return patientId;
        //}

        public long GetInternalPatientId(short extId1, string extId2)
        {
            return GetInternalPatientId(extId1 + "|" + extId2);
        }

        public long GetInternalPatientId(string externalId)
        {
            var ptId = from e in _context.ExternalIds
                       where e.ExternalId == externalId
                             && e.Entity == "patients"
                             && e.Vendor == "pulsecheck"
                       select e.InternalId;

            return ptId.FirstOrDefault();
        }

        public Dictionary<string, string> GetExternalRootSitePatientId(string number, GetPatientBy getPatientBy, string rootType)
        {
            Expression<Func<Patient, bool>> predicate = GetWherePredicate(number, getPatientBy);

            var pathQuery =
                from p in (from p in _context.Patients select p).Where(predicate)
                join s in _context.Sites on p.SiteId equals s.Id
                join so in _context.SiteOptions on s.Id equals so.SiteId
                join o in _context.Options on so.OptionId equals o.Id
                where o.Name == rootType
                select so.OptionValue;

            var extQuery =
                from p in (from p in _context.Patients select p).Where(predicate)
                join e in _context.ExternalIds on p.Id equals e.InternalId
                where e.Entity == "patients"
                      && e.Vendor == "pulsecheck"
                select e.ExternalId;

            var path = pathQuery.FirstOrDefault();
            var extId = extQuery.FirstOrDefault();

            var extIdParts = extId?.Split('|');

            return new Dictionary<string, string>
            {
                {"root", path},
                {"siteId", extIdParts?[0]},
                {"patientId", extIdParts?[1]}
            };

            #region Version 346 code

            //var list = _context.Patients
            //    .Include(patient => patient.Site)
            //    .ThenInclude(site => site.ExternalIds)
            //    .Include(patient => patient.Site)
            //    .ThenInclude(site => site.SiteOptions)
            //    .ThenInclude(siteOptions => siteOptions.Option)
            //    .Include(patient => patient.ExternalIds)
            //    .ThenInclude(externalIds => externalIds.Site.SiteOptions)
            //    .ThenInclude(siteOptions => siteOptions.Option)
            //    .Where(p => p.ExternalIds.InternalId == p.Id)
            //    .Where(p => p.ExternalIds.Entity == "patients")
            //    .Where(p => p.ExternalIds.Vendor == "pulsecheck")
            //    .Where(p => p.Site.ExternalIds.Entity == "sites")
            //    .Where(p => p.Site.ExternalIds.Vendor == "pulsecheck")
            //    .Where(GetWherePredicate(number, getPatientBy))
            //    .Select(c => new
            //    {
            //        root = c.Site.SiteOptions
            //            .Join(_context.Options
            //                    .Where(o => o.Name == RootType),
            //                so => so.OptionId,
            //                o => o.Id,
            //                (so, o) => new { so.OptionValue }),
            //        site = c.ExternalIds.ExternalId.Split("|")[0],
            //        extId = c.ExternalIds.ExternalId.Split("|")[1]
            //    });

            //Dictionary<string, string> e2 = new Dictionary<string, string>();
            //e2.Add("root", list.Select(c => c.root).Select(c => c).FirstOrDefault().Select(c => c.OptionValue).FirstOrDefault());
            //e2.Add("siteId", list.Select(c => c.site).Select(c => c).FirstOrDefault());
            //e2.Add("patientId", list.Select(c => c.extId).Select(c => c).FirstOrDefault());

            //Dictionary<string, string> externalRootSitePatientId = new Dictionary<string, string>
            //{
            //    { "root", list.Select(c => c.root).Select(c => c).FirstOrDefault().Select(c => c.OptionValue).FirstOrDefault() },
            //    { "siteId", list.Select(c => c.site).Select(c => c).FirstOrDefault() },
            //    { "patientId", list.Select(c => c.extId).Select(c => c).FirstOrDefault() }
            //};

            #endregion
            #region Annonymous Joins
            ////////var externalRootSitePatientId = _context.Patients
            ////////                                //.Where(wherePredicate)
            ////////                                .Join(_context.ExternalIds.Where(ep => ep.Vendor == "pulsecheck" && ep.Entity == "patients"),
            ////////                                        p => p.Id,
            ////////                                        ep => ep.InternalId,
            ////////                                        (p, ep) => new { p, ep })
            ////////                                .Join(_context.Sites,
            ////////                                        p => p.p.Id,
            ////////                                        s => s.Id,
            ////////                                        (p, s) => new { p, s })
            ////////                                .Join(_context.ExternalIds.Where(es => es.Vendor == "pulsecheck" && es.Entity == "sites"),
            ////////                                        s => s.s.Id,
            ////////                                        es => es.InternalId,
            ////////                                        (s, es) => new { s, es })
            ////////                                .Join(_context.SiteOptions,
            ////////                                        s => s.s.s.Id,
            ////////                                        so => so.SiteId,
            ////////                                        (s, so) => new { s, so })
            ////////                                .Join(_context.Options.Where(o => o.Name == "PATIENT_IMAGE_PATH"),
            ////////                                        so => so.so.OptionId,
            ////////                                        o => o.Id,
            ////////                                        (so, o) => new { so, o })
            ////////                                .Select(c => new
            ////////                                {
            ////////                                    iSiteId = c.so.s.s.s.Id,
            ////////                                    eSiteId = c.so.s.es.ExternalId,
            ////////                                    iPtId = c.so.s.s.p.p.Id,
            ////////                                    iPtAccountNumber = c.so.s.s.p.p.AccountNumber,
            ////////                                    iPtCustomNumber = c.so.s.s.p.p.CustomNumber,
            ////////                                    oPtPersonNumber = c.so.s.s.p.p.PersonNumber,
            ////////                                    oSoOptionValue = c.so.so.OptionValue,
            ////////                                    eptId = c.so.s.s.p.ep.ExternalId
            ////////                                });
            #endregion
            #region Straight SQL
            //////var externalRootSitePatientId = from p in _context.Patients
            ////////var externalRootSitePatientId = from p in ((from p in _context.Patients
            ////////                                               select p)
            ////////                                        .Where(wherePredicate))
            //////                                   join ep in _context.ExternalIds on p.Id equals ep.InternalId
            //////                                   join s in _context.Sites on p.SiteId equals s.Id
            //////                                   join es in _context.ExternalIds on s.Id equals es.InternalId
            //////                                   join so in _context.SiteOptions on s.Id equals so.SiteId
            //////                                   join o in _context.Options on so.OptionId equals o.Id
            //////                                   where ep.Vendor == "pulsecheck"
            //////                                         && ep.Entity == "patients"
            //////                                         && es.Vendor == "pulsecheck"
            //////                                         && es.Entity == "sites"
            //////                                         && o.Name == "PATIENT_IMAGE_PATH"
            //////                                   select so.OptionValue + "|" + ep.ExternalId;
            #endregion
            #region
            ////string byField;

            ////switch (getPatientBy)
            ////{
            ////    case GetPatientBy.Id:
            ////        byField = "p.id";
            ////        break;
            ////    case GetPatientBy.MedicalRecordNumber:
            ////        byField = "p.medical_record_number";
            ////        number = "'" + number + "'";
            ////        break;
            ////    case GetPatientBy.AccountNumber:
            ////        byField = "p.account_number";
            ////        number = "'" + number + "'";
            ////        break;
            ////    case GetPatientBy.CustomNumber:
            ////        byField = "p.custom_number";
            ////        number = "'" + number + "'";
            ////        break;
            ////    case GetPatientBy.PersonNumber:
            ////        byField = "p.person_number";
            ////        number = "'" + number + "'";
            ////        break;
            ////    default:
            ////        return null;
            ////}

            ////var externalRootSitePatientId = from e in _context.ExternalIds
            ////                                .FromSqlRaw("" +
            ////                                "SELECT    so.option_value + '|' + ep.external_id AS external_id " +
            ////                                "FROM      sites AS s " +
            ////                                "  JOIN    external_ids AS es ON s.id = es.internal_id " +
            ////                                "  JOIN    patients AS p ON p.site_id = s.id " +
            ////                                "  JOIN    external_ids AS ep ON p.id = ep.internal_id " +
            ////                                "  JOIN    site_options AS so ON so.site_id = s.id " +
            ////                                "  JOIN    options AS o ON so.option_id = o.id " +
            ////                                "WHERE     (es.vendor = 'pulsecheck' AND " +
            ////                                "           es.entity = 'sites') AND " +
            ////                                "          (ep.vendor = 'pulsecheck' AND " +
            ////                                "           ep.entity = 'patients') AND " +
            ////                                "          o.name = 'PATIENT_IMAGE_PATH' AND " +
            ////                                "          " + byField + " = " + number + " ")
            ////                                select e.ExternalId;
            #endregion

            //return externalRootSitePatientId;
        }

        public Patient GetPatientByNumber(string number, GetPatientBy getPatientBy, bool includeOrders)
        {
            return GetPatients(GetWherePredicate(number, getPatientBy), includeOrders)
                    .FirstOrDefault();
        }

        public int GetSiteIdForPatient(long patientId)
        {
            return _context.Patients.Where(p => p.Id == patientId)
                .Select(p => p.SiteId).FirstOrDefault();
        }

        private static Expression<Func<Patient, bool>> GetWherePredicate(string number, GetPatientBy getPatientBy)
        {
            switch (getPatientBy)
            {
                case GetPatientBy.Id:
                    return p => p.Id == long.Parse(number);
                case GetPatientBy.AccountNumber:
                    return p => p.AccountNumber == number;
                case GetPatientBy.CustomNumber:
                    return p => p.CustomNumber == number;
                case GetPatientBy.PersonNumber:
                    return p => p.PersonNumber == number;
                default:
                    return null;
            }
        }

        public IEnumerable<PatientAllergy> GetAllergiesByPatientId(long patientId, Expression<Func<PatientAllergy, bool>> wherePredicate = null)
        {
            Expression<Func<PatientAllergy, bool>> whereLambda = a => a.PatientId == patientId;

            if (wherePredicate != null)
            {
                whereLambda = whereLambda.And(wherePredicate);
            }

            var allergies = _context.PatientAllergies
                .Include(a => a.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(d => d.FdbBrandName)
                .Include(a => a.Medication)
                    .ThenInclude(m => m.MedicationDetails)
                        .ThenInclude(md => md.MedicationUnit)
                .Where(whereLambda)
                .ToList();

            return allergies.AsEnumerable();
        }

        public IEnumerable<FdbAllergyName> GetAllergyFdbAllergyNames(string name, Expression<Func<FdbAllergyName, bool>> wherePredicate = null)
        {
            Expression<Func<FdbAllergyName, bool>> whereLambda = f => f.AllergyName == name;

            if (wherePredicate != null)
            {
                whereLambda = whereLambda.And(wherePredicate);
            }

            return _context.FdbAllergyName
                .Where(whereLambda);
        }

        public IEnumerable<FdbAllergyName> GetAllergyFdbAllergyNamesByIdentifier(string identifier, Expression<Func<FdbAllergyName, bool>> wherePredicate = null)
        {
            // Handle Hicls / L-codes by default. They query PC_HICL_SEQNO.
            Expression<Func<FdbAllergyName, bool>> whereLambda = f => f.PcHiclSeqno == identifier;

            // If we received a G-code, insteadf query PC_MED_NAME_ID
            if (!string.IsNullOrWhiteSpace(identifier) && identifier.ToLowerInvariant().StartsWith("g"))
            {
                whereLambda = f => f.PcMedNameId == identifier;
            }

            if (wherePredicate != null)
            {
                whereLambda = whereLambda.And(wherePredicate);
            }

            return _context.FdbAllergyName
                .Where(whereLambda);
        }

        public int? GetPatientsAttendingDoctorIdByRole(long patientId, string role)
        {
            var user = _context.UserPatients.FirstOrDefault(u => u.PatientId == patientId && u.RoleName == role);
            return user?.UserId;
        }

        public Patient GetAdministrationsByPatientAndOrder(long patientId, long orderId)
        {
            //Call the existing GetPatients method to get the patient, the order, and the order administrations.
            //This lambda gets only the one patient and then only the one order.
            //Yay code reuse!
            var ret = GetPatients(patient => patient.Id == patientId && patient.PatientOrders.Any(order => order.Id == orderId), true)
                    .FirstOrDefault();

            //I saved the results to a var and then return the var so that
            //I can inspect the values when debugging in VS.
            //Return the Patient entity.
            return ret;

            //throw new NotImplementedException();
        } //end GetAdministrationsByPatientAndOrder

        #region IDS Methods

        public void FilePatient(Patient patientFromHost, out bool interactRecalcNeeded, string queueRecordId)
        {
            if (patientFromHost.Id > 0)
            {
                Patient existingPatient = _context.Patients
                    .Include(p => p.PatientAllergies)
                    .Include(p => p.PatientHomeMedications)
                    .FirstOrDefault(u => u.Id == patientFromHost.Id);

                if (existingPatient == null)
                {
                    _logger.LogWarning($"Made it into FilePatient() with a existingPatient.Id of {existingPatient.Id}" +
                                       $" which couldn't be retrieved from the database");
                    interactRecalcNeeded = false;
                    return;
                }

                _context.UserPatients.RemoveRange(_context.UserPatients.Where(u => u.PatientId == existingPatient.Id).ToList());
                _context.SaveChanges();

                existingPatient.UserPatients = patientFromHost.UserPatients;

                // 20220628 BRM: patientFromHost will have deactivationDateTime = null, which will be filled in 
                // with the default of (C# equivalent of) SYSDATETIMEOFFSET().  If the patient is already
                // deactivated, then we'd be overwriting it, so...
                if (!existingPatient.Active)
                    patientFromHost.DeactivationDatetime = existingPatient.DeactivationDatetime;
                _context.Entry(existingPatient).CurrentValues.SetValues(patientFromHost);

                UpdateAllergies(patientFromHost, existingPatient, out bool allergiesUpdated, queueRecordId);
                UpdateMedications(patientFromHost, existingPatient, out bool medsUpdated, queueRecordId);

                interactRecalcNeeded = allergiesUpdated || medsUpdated;

                _context.SaveChanges();
            }
            //Else, we haven't added this patient yet.
            //If it's an active patient (i.e. emar_pat is 'Y' and the patient
            //hasn't been archived in PulseCheck yet), then add them to the DB.
            //Winston Murdock, 06/24/2022.
            else if (patientFromHost.Active)
            {
                // Save this new patient to the database
                interactRecalcNeeded = true;
                SaveNewPatientToDatabase(patientFromHost);
            }
            else
            {
                //Don't recalculate if the patient isn't active.
                interactRecalcNeeded = false;
            }
            //Else, the patient has not been added yet, but this is not an emar patient
            //or the patient has already been archived.  Either way, we don't need to add the patient.
        }

        public void FilePatientIndicators(Patient patientFromHost)
        {
            if (patientFromHost.Id > 0)
            {
                Patient existingPatient = _context.Patients.FirstOrDefault(u => u.Id == patientFromHost.Id);

                if (existingPatient == null)
                {
                    _logger.LogWarning($"Made it into FilePatientIndicators() with a existingPatient.Id of {existingPatient.Id}" +
                                       $" which couldn't be retrieved from the database");
                    return;
                }

                _context.PatientIndicators.RemoveRange(_context.PatientIndicators.Where(u => u.PatientId == existingPatient.Id).ToList());
                _context.SaveChanges();

                existingPatient.PatientIndicators = patientFromHost.PatientIndicators;
                _context.SaveChanges();
            }
            else
            {
                // Save this new patient to the database
                SaveNewPatientToDatabase(patientFromHost);
            }
        }

        private void SaveNewPatientToDatabase(Patient patientFromHost)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.Add(patientFromHost);
                _context.SaveChanges();

                //We're seeing an issue where we try to add a patient into the external_ids table
                //when they already exist.
                //We had a similar issue with users back in Decmeber of 2021.
                //The fix there was to add an "if exists" check around this insert call.
                //I propose that we do something similar here.
                //Winston Murdock, 05/14/2022.  PC-27271
                if (!_context.ExternalIds.Any
                    (x =>
                        x.Entity == "patients" &&
                        x.Vendor == "pulsecheck" &&
                        x.ExternalId == patientFromHost.ExternalSiteId + "|" + patientFromHost.ExternalPatientId
                    )
                )
                {
                    var externalId = new ExternalIdEntity
                    {
                        Entity = "patients",
                        Vendor = "pulsecheck",
                        InternalId = patientFromHost.Id,
                        ExternalId = patientFromHost.ExternalSiteId + "|" + patientFromHost.ExternalPatientId
                    };

                    _context.Add(externalId);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                else
                {
                    //Log to a text file that we tried to add a patient into external_ids that
                    //already existed in there.
                    //Winston Murdock, 05/16/2022.  PC-27271
                    WriteDuplicatePatientToFile(patientFromHost.ExternalSiteId.ToString(), patientFromHost.ExternalPatientId);
                } //end if  (does this PCED patient already exist in the external_ids table?)
            }
            catch (Exception e)
            {
                transaction.Rollback();
                _logger.LogError(
                    "Exception Encountered when filing new patient from PatientRepository.FilePatient(): " +
                    $"{Utilities.ExtractExceptionMessages(e)}");
            }
        }

        private void UpdateAllergies(Patient patientFromHost, Patient existingPatient, out bool allergiesUpdated, string queueRecordId)
        {
            allergiesUpdated = false;

            try
            {
                // Delete children
                foreach (var existingChild in existingPatient.PatientAllergies
                    .Where(ec => !patientFromHost.PatientAllergies.Any(a => a.InternalKey == ec.InternalKey))
                    .ToList())
                {
                    _context.Remove(existingChild);
                    allergiesUpdated = true;
                }

                // Update and Insert children
                foreach (var childModel in patientFromHost.PatientAllergies)
                {
                    var existingChild = existingPatient.PatientAllergies
                        .FirstOrDefault(ec => ec.InternalKey == childModel.InternalKey);

                    if (existingChild != null)
                    {
                        _context.Entry(existingChild).State = EntityState.Unchanged;

                        // Update child - need to set the Id and PatientId first because it throws an error
                        // if we try to change it, even to "0"
                        childModel.Id = existingChild.Id;
                        childModel.PatientId = existingPatient.Id;
                        _context.Entry(existingChild).CurrentValues.SetValues(childModel);

                        if (_context.Entry(existingChild).State != EntityState.Unchanged)
                            allergiesUpdated = true;
                    }
                    else
                    {
                        childModel.Id = 0;
                        childModel.PatientId = 0;
                        // Insert child
                        existingPatient.PatientAllergies.Add(childModel);
                        allergiesUpdated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // 20220629 BRM: We don't want this to stop us from filling the patient
                _inboundDataRepository.LogQueueError(queueRecordId, "PatientRepository.FilePatient() - UpdateAllergies()", ex);

                // At this point, the existingPatient.HomeMedications can be jacked up, and that will
                // hose trying to call _context.SaveChanges() for all future calls.  So clean it up now.
                foreach (var entry in _context.ChangeTracker
                    .Entries()
                    .Where(x => x.Entity != null && typeof(PatientAllergy) == x.Entity.GetType()
                    && x.State != EntityState.Unchanged)
                    .ToList())
                {
                    entry.Reload();
                }
            }

        }

        private void UpdateMedications(Patient patientFromHost, Patient existingPatient, out bool medsUpdated, string queueRecordId)
        {
            medsUpdated = false;

            try
            {
                // Delete orphaned children
                foreach (var existingChild in existingPatient.PatientHomeMedications
                    .Where(ec => !patientFromHost.PatientHomeMedications.Any(h => h.InternalKey == ec.InternalKey))
                    .ToList())
                {
                    _context.Remove(existingChild);
                    medsUpdated = true;
                }

                // Update and Insert children
                foreach (var childModel in patientFromHost.PatientHomeMedications)
                {
                    var existingChild = existingPatient.PatientHomeMedications
                      .FirstOrDefault(ec => ec.InternalKey == childModel.InternalKey);

                    if (existingChild != null)
                    {
                        _context.Entry(existingChild).State = EntityState.Unchanged;

                        // Update child - need to set the Id and PatientId first because it throws an error
                        // if we try to change it, even to "0"
                        childModel.Id = existingChild.Id;
                        childModel.PatientId = existingPatient.Id;
                        _context.Entry(existingChild).CurrentValues.SetValues(childModel);

                        if (_context.Entry(existingChild).State != EntityState.Unchanged)
                            medsUpdated = true;
                    }
                    else
                    {
                        childModel.Id = 0;
                        childModel.PatientId = 0;
                        // Insert child
                        existingPatient.PatientHomeMedications.Add(childModel);
                        medsUpdated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // 20220629 BRM: We don't want this to stop us from filling the patient
                _inboundDataRepository.LogQueueError(queueRecordId, "PatientRepository.FilePatient() - UpdateMedications()", ex);
                medsUpdated = false;

                // At this point, the existingPatient.HomeMedications can be jacked up, and that will
                // hose trying to call _context.SaveChanges() for all future calls.  So clean it up now.
                foreach (var entry in _context.ChangeTracker
                    .Entries()
                    .Where(x => x.Entity != null && typeof(PatientHomeMedication) == x.Entity.GetType()
                    && x.State != EntityState.Unchanged)
                    .ToList())
                {
                    entry.Reload();
                }
            }
        }

        public void DeactivatePatient(long patientId)
        {
            Patient existingPatient = _context.Patients.FirstOrDefault(u => u.Id == patientId);
            if (existingPatient == null)
                return;

            existingPatient.Active = false;
            _context.SaveChanges();
        }

        public void WriteDuplicatePatientToFile(string siteId, string ibex)
        {
            //Write the site id, ibex number, and current time (in this webserver's time zone) into a text file.
            //This way we can go back and see the patients who ran into this in hopes of finding a pattern.
            //Winston Murdock, 05/16/2022.  PC-27271

            //Wrap the whole thing in a try/catch so that this doesn't
            //effect the operation of the IDS.
            //Should something happen, we'll silently eat it and move onwards.
            try
            {
                //Get the path to this assembly (Emar.Core.dll)
                //This assembly is at C:\inetpub\wwwroot\emarAPI\Emar.Core.Dll
                string sPath = Assembly.GetExecutingAssembly().Location;

                //Get the assembly's name (Emar.Core).
                string sAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                //Chop Emar.Core.dll off the path.
                sPath = sPath.Replace(sAssemblyName + ".dll", "");

                //append the name of the txt file.
                sPath += "duplicate_patient_log.txt";

                //If the file exists, then log.
                //If the file doesn't exist, then don't log.
                //When we want to log, we'll have the file there.
                //When we don't want to log, we'll rename the file to something else.
                //This way, we don't have to make a new setting in appsettings and somehow pass it down to here.
                if (File.Exists(sPath))
                {
                    using (StreamWriter sw = File.AppendText(sPath))
                    {
                        //Each call to WriteLine appends a line break to the end.
                        sw.WriteLine("Timestamp (in the time zone of this server) = " + DateTime.Now.ToString());
                        sw.WriteLine("Ibex Number = " + ibex);
                        sw.WriteLine("Site ID = " + siteId);
                        sw.WriteLine("");
                    } //end using
                } //end if
            }
            catch (Exception ex)
            {
                //Don't do anything with an exception here.
            } //end try/catch.
        } //end WriteDuplicatePatientToFile

        #endregion
    }
}