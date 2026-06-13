using Emar.Core.OutboundData.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emar.Core.OutboundData.Repository
{
    public class EmarOutboundDataRepository : IEmarOutboundDataRepository
    {
        private readonly EmarContext _context;
        private readonly ILogger<EmarOutboundDataRepository> _logger;

        public EmarOutboundDataRepository(EmarContext context, ILogger<EmarOutboundDataRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string GetExternalPatientId(long internalPatientId)
        {
            var ptId = from e in _context.ExternalIds
                where e.InternalId == internalPatientId
                      && e.Entity == "patients"
                      && e.Vendor == "pulsecheck"
                select e.ExternalId;

            var retVal = ptId.FirstOrDefault();
            return (!string.IsNullOrWhiteSpace(retVal) && retVal.Contains("|")) ? retVal.Split("|")[1] : "";
        }

        public int GetExternalUserId(int internalUserId)
        {
            var ptId = from e in _context.ExternalIds
                where e.InternalId == internalUserId
                      && e.Entity == "users"
                      && e.Vendor == "pulsecheck"
                select e.ExternalId;

            return int.TryParse(ptId.FirstOrDefault(), out int number) ? number : 0;
        }

        public int GetExternalSiteId(int internalSiteId)
        {
            var ptId = from e in _context.ExternalIds
                where e.InternalId == internalSiteId
                      && e.Entity == "sites"
                      && e.Vendor == "pulsecheck"
                select e.ExternalId;

            return int.TryParse(ptId.FirstOrDefault(), out int number) ? number : 0;
        }

        public string GetFdbBrandName(int internalMedId)
        {
            var query = from m in _context.Medications
                        join bn in _context.FdbBrandName on m.DrugId equals bn.MedidString
                        where m.Id == internalMedId
                        select bn.BrandName;

            return query.FirstOrDefault();
        }

        public string GetAmIndication(string internalAmId)
        {
            int.TryParse(internalAmId, out var intInternalAmId);
            var query = from m in _context.AntimicrobialIndications
                        where m.Id == intInternalAmId
                        select m.Code;

            return query.FirstOrDefault();
        }
        public string GetRoute(string internalRouteId)
        {
            int.TryParse(internalRouteId, out var intInternalRouteId);
            var query = from m in _context.MedicationRoutes
                    where m.Id == intInternalRouteId
                    select m.Code;

            return query.FirstOrDefault();
        }
        public string GetUnit(string internalUnitId)
        {
            int.TryParse(internalUnitId, out var intInternalUnitId);
            var query = from m in _context.MedicationUnits
                        where m.Id == intInternalUnitId
                        select m.Code;

            return query.FirstOrDefault();
        }

        // TODO: Rewrite due to FDB specific references
        public OdsMedicationDetails GetMedicationDetails(int internalMedId)
        {
            // may want to clean up query or its results handling since it can return more than one row. However, don't fret,
            // the rows are identical except for the NDC related info (ndc, repackaged, and days_obsolete) which is not being
            // captured in the OdsMedicationDetails object.
            // also, the create_FDB_search.sql file needs to be updated to include the missing attributes below.
            var query = (from m in _context.Medications
                         join bn in _context.FdbBrandName on m.DrugId equals bn.MedidString
                         join ndc in _context.FdbNdcInfo on m.DrugId equals ndc.MedidString
                         where m.Id == internalMedId
                         select new OdsMedicationDetails
                         {
                             BrandName = bn.BrandName,
                             ActiveName = bn.Active,
                             DrugRoute = ndc.Route,
                             DrugForm = ndc.DoseForm,
                             DrugStrength = ndc.Strength,
                             ActiveId = bn.PcRoutedGenId,
                             DrugId = ndc.GcnSeqno.ToString(),
                             PackagingId = ndc.BaseNdc,
                             DrugCategoryId = ndc.DrugCat.ToString(),
                         }).ToList();

            return query.FirstOrDefault();
        }

        // TODO: Rewrite due to FDB specific references
        public OdsMedicationDetails GetMedicationDetailsFromMedDetailsId(int detailsId)
        {
            // may want to clean up query or its results handling since it can return more than one row. However, don't fret,
            // the rows are identical except for the NDC related info (ndc, repackaged, and days_obsolete) which is not being
            // captured in the OdsMedicationDetails object.
            // also, the create_FDB_search.sql file needs to be updated to include the missing attributes below.
            var query = (from md in _context.MedicationDetails
                         join bn in _context.FdbBrandName on md.DrugId equals bn.MedidString
                         join ndc in _context.FdbNdcInfo on md.DrugId equals ndc.MedidString
                         where md.Id == detailsId
                         select new OdsMedicationDetails
                         {
                             BrandName = bn.BrandName,
                             ActiveName = bn.Active,
                             DrugRoute = ndc.Route,
                             DrugForm = ndc.DoseForm,
                             DrugStrength = ndc.Strength,
                             ActiveId = bn.PcRoutedGenId,
                             DrugId = ndc.GcnSeqno.ToString(),
                             PackagingId = ndc.BaseNdc,
                             DrugCategoryId = ndc.DrugCat.ToString(),
                             EnteredDose = md.Dose.ToString(),
                             EnteredUnit = md.MedicationUnit.Code
                         }).ToList();

            return query.FirstOrDefault();
        }

        public string GetComboName(int medicationId)
        {
            var query = from m in _context.Medications
                        where m.Id == medicationId
                        && m.DrugId.Equals("COMBO")
                        select m.DisplayName;

            return query.FirstOrDefault() ?? "";
        }

        public List<int> GetMedicationDetailsIds(int medicationId)
        {
            var query = from m in _context.MedicationDetails
                        where m.MedicationId == medicationId
                        select m.Id;
            return query.ToList(); // Select(s => int.Parse(s))
        }

        public string GetServiceByNdc(string ndc)
        {
            var query = from f in _context.SiteFormulary
                        where f.Ndc == ndc
                        select f.ServiceCode;

            return query.FirstOrDefault();
        }

        //        public int ConvertDatetimeToLosecs (DateTimeOffset losecs, string ibex)
        //        {
        //            return _time.DiffSeconds(patient.Ibex) + rand.Next(1, 50000);
        //        }

    }
}
