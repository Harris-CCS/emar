using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Emar.Core.Helpers;
using Emar.Core.Patients.Model;
using Emar.Core.ResourceParameters;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using static Emar.Core.Patients.Model.Constants;

namespace Emar.Core.Patients.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly EmarContext _context;
        private readonly IPropertyMappingService _propertyMappingService;

        public PatientRepository()
        {
        }

        public PatientRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public PagedList<Patient> GetPatients(PatientsResourceParameters resourceParameters, bool includeOrders)
        {
            Expression<Func<Patient, bool>> whereLambda = null;

            if (!resourceParameters.IncludeInactive)
            {
                whereLambda = //whereLambda.And(pt => pt.Active == true);
                    pt => pt.Active;
            }

            if (resourceParameters.SiteId != null)
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

            IEnumerable<Patient> patients = GetPatients(whereLambda, ((resourceParameters != null) && resourceParameters.IncludeOrders) || includeOrders);

            if (resourceParameters.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<PatientDto, Patient>();

                patients = patients
                    .AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }

            return PagedList<Patient>.Create(patients.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public Patient GetPatient(long? patientId, PatientsResourceParameters resourceParameters, bool includeOrders)
        {
            return GetPatients(patient => patient.Id == patientId, ((resourceParameters != null) && resourceParameters.IncludeOrders) || includeOrders)
                    .FirstOrDefault();
        }


        IEnumerable<Patient> GetPatients(Expression<Func<Patient, bool>> wherePredicate, bool includeOrders = true)
        {
            if (includeOrders)
            {
                return _context.Patients
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.OrderAdministrations)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.MedicationRoute)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.MedicationUnit)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.FrequencySchedule)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.AddUser)
                    .Include(patient => patient.PatientOrders)
                        .ThenInclude(order => order.OrderPhysicianUser)
                    //.Include(patient => patient.Site)
                    //    .ThenInclude(site => site.SiteOptions)
                    //        .ThenInclude(siteOptions => siteOptions.Option)
                    //.Include(patient => patient.ExternalIds)
                    .Include(patient => patient.PatientIndicators)
                    .Include(p => p.PatientAllergies)
                    .Include(p => p.PatientHomeMedications)
                        .ThenInclude(h => h.MedicationUnit)
                    .Include(p => p.PatientHomeMedications)
                        .ThenInclude(h => h.MedicationRoute)
                    .Where(wherePredicate)
                    .ToList();
            }

            return _context.Patients
                //.Include(patient => patient.Site)
                //    .ThenInclude(site => site.SiteOptions)
                //        .ThenInclude(siteOptions => siteOptions.Option)
                //.Include(patient => patient.ExternalIds)
                .Include(patient => patient.PatientIndicators)
                .Include(p => p.PatientAllergies)
                .Include(p=>p.PatientHomeMedications)
                    .ThenInclude(h => h.MedicationUnit)
                .Include(p => p.PatientHomeMedications)
                    .ThenInclude(h => h.MedicationRoute)
                .Where(wherePredicate)
                .ToList();
        }
        //IEnumerable<Patient> GetPatients(Func<Patient, bool> wherePredicate, bool includeOrders = true)
        //{
        //    if (includeOrders)
        //    {
        //        return _context.Patients
        //                .Include(patient => patient.PatientOrders)
        //                    .ThenInclude(order => order.OrderAdministrations)
        //                .Include(patient => patient.PatientOrders)
        //                    .ThenInclude(order => order.MedicationRoute)
        //                .Include(patient => patient.PatientOrders)
        //                    .ThenInclude(order => order.MedicationUnit)
        //                .Include(patient => patient.PatientOrders)
        //                    .ThenInclude(order => order.AddUser)
        //                .Include(patient => patient.PatientOrders)
        //                    .ThenInclude(order => order.OrderPhysicianUser)
        //                .Include(patient => patient.Site)
        //                    .ThenInclude(site => site.SiteOptions)
        //                        .ThenInclude(siteOptions => siteOptions.Option)
        //                .Include(patient => patient.ExternalIds)
        //                .Where(wherePredicate)
        //                .ToList();
        //    }

        //    return _context.Patients
        //            .Include(patient => patient.Site)
        //                .ThenInclude(site => site.SiteOptions)
        //                    .ThenInclude(siteOptions => siteOptions.Option)
        //                .Include(patient => patient.ExternalIds)
        //                .Where(wherePredicate)
        //                .ToList();
        //}

        public long? GetPatientId(long? patientId, PatientsResourceParameters resourceParameters)
        {
            if ((resourceParameters != null) &&
                (resourceParameters.ExtId1 != null) &&
                (resourceParameters.ExtId2 != null))
            {
                patientId = _context.ExternalIds
                    .FirstOrDefault(xId => xId.ExternalId.Equals(resourceParameters.ExtId1 + "|" + resourceParameters.ExtId2) &&
                                             xId.Entity.ToLower().Equals(@"patients") &&
                                             xId.Vendor.ToLower().Equals(@"pulsecheck"))
                    .InternalId;
            }

            return patientId;
        }

        public long GetInternalPatientId(short extId1, string extId2)
        {
            var ptId = from e in _context.ExternalIds
                       where e.ExternalId == extId1 + "|" + extId2
                             && e.Entity == "patients"
                             && e.Vendor == "pulsecheck"
                       select e.InternalId;

            return ptId.FirstOrDefault();
        }

        public Dictionary<string, string> GetExternalRootSitePatientId(string number, GetPatientBy getPatientBy, string RootType)
        {
            Expression<Func<Patient, bool>> predicate = GetWherePredicate(number, getPatientBy);

            var pathQuery =
                from p in (from p in _context.Patients select p).Where(predicate)
                join s in _context.Sites on p.SiteId equals s.Id
                join so in _context.SiteOptions on s.Id equals so.SiteId
                join o in _context.Options on so.OptionId equals o.Id
                where o.Name == RootType
                select so.OptionValue;

            var extQuery =
                from p in (from p in _context.Patients select p).Where(predicate)
                join e in _context.ExternalIds on p.Id equals e.InternalId
                where e.Entity == "patients"
                      && e.Vendor == "pulsecheck"
                select e.ExternalId;

            var path = pathQuery.FirstOrDefault();
            var extId = extQuery.FirstOrDefault();

            var extIdParts = extId.Split('|');

            return new Dictionary<string, string>
            {
                {"root", path},
                {"siteId", extIdParts[0]},
                {"patientId", extIdParts[1]}
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

        Expression<Func<Patient, bool>> GetWherePredicate(string number, GetPatientBy getPatientBy)
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

            return _context.PatientAllergies
                .Where(whereLambda);
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

        public IEnumerable<FdbAllergyName> GetAllergyFdbAllergyNamesByPcHiclSeqno(string pcHiclSeqno, Expression<Func<FdbAllergyName, bool>> wherePredicate = null)
        {
            Expression<Func<FdbAllergyName, bool>> whereLambda = f => f.PcHiclSeqno == pcHiclSeqno;

            if (wherePredicate != null)
            {
                whereLambda = whereLambda.And(wherePredicate);
            }

            return _context.FdbAllergyName
                .Where(whereLambda);
        }
    }
}