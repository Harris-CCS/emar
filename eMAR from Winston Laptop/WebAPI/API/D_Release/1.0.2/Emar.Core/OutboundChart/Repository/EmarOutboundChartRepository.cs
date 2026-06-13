using Emar.Core.Helpers;
//using Emar.Core.Orders.Model;
using Emar.Core.OutboundChart.Model;
using Emar.Data;
//using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emar.Core.OutboundChart.Repository
{
    public class EmarOutboundChartRepository : IEmarOutboundChartRepository
    {
        private readonly EmarContext _context;
        private readonly ILogger<EmarOutboundChartRepository> _logger;

        public EmarOutboundChartRepository(EmarContext context, ILogger<EmarOutboundChartRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get an EMR.Line object that represents this medication's information
        /// </summary>
        /// <param name="patient">IPatient instance</param>
        /// <param name="med">medication</param>
        /// <param name="sysDate">System date/time for entry</param>
        /// <param name="pharmVerifStatus">pharmacy verification status</param>
        /// <returns>EMR.Line object for entry</returns>
//        public async Task<EMR.Line> ChartEntry(IPatient patient, Medication med, string sysDate, List<OverrideRationale> overrides)
        public EMR.Line ChartEntry(IPatient patient, Medication med, string sysDate, byte pharmVerifStatus, string? prnIndication)
        {
            var line = new EMR.Line();

            var medInfo = new StringBuilder();
            var NCTNAME = med.GetName();

            if (med.IsFreeText())
            {
                medInfo.Append(string.Format("Free Text order: {0} : {1} : {2}", NCTNAME, med.Notes, med.GetRouteDescription()));
            }
            else
            {
                medInfo.Append(string.Format("Order: {0}", med.GetFullNameForChart()));

                if (!string.IsNullOrWhiteSpace(med.Frequency))
                    medInfo.Append(string.Format(" <b>Frequency: </b> {0}", med.Frequency));

                if (!string.IsNullOrWhiteSpace(med.Duration))
                    medInfo.Append(string.Format(" <b>Duration: </b> {0}", med.Duration));

                // rate is Medispan only at the moment
                var rate = string.Format("{0} {1}", med.Rate, med.GetRateUnitDescription());
                if (!string.IsNullOrWhiteSpace(rate))
                    medInfo.Append(string.Format("<b>Rate: </b> {0}", rate));

                // PRN Indication
                if (!string.IsNullOrWhiteSpace(prnIndication))
                    medInfo.Append(string.Format("\nPRN Indication: {0}", prnIndication));

                foreach (var comp in med.Components)
                {
                    var brandName = "";
                    if (med.IsCombo())
                    {
                        medInfo.Append(string.Format("\n** {0}", comp.GetFullName(med)));
                        brandName = comp.GetBrandName();
                    }

                    foreach (var drugInteraction in comp.Interactions)
                    {
                        // started with mobile api code here and removed overrides block(s) including applyToAll
                        medInfo.Append(string.Format("\nPOTENTIAL {0}: {1} - ", drugInteraction["interaction"], drugInteraction["drug"]));
                        if (!string.IsNullOrWhiteSpace(drugInteraction["override_reason"]))
                            medInfo.Append(drugInteraction["override_reason"]);
                        med.WriteTrx(patient, drugInteraction["sevtxt"], drugInteraction["drug"], NCTNAME, comp, Convert.ToInt32(med.OrderUserId));

                    }

                    foreach (var allergyReaction in comp.Reactions)
                    {
                        // started with mobile api code here and removed overrides block(s) including applyToAll
                        medInfo.Append(string.Format("\nPOTENTIAL {0}: {1} - ", allergyReaction["interaction"], allergyReaction["drug"]));
                        if (!string.IsNullOrWhiteSpace(allergyReaction["override_reason"]))
                            medInfo.Append(allergyReaction["override_reason"]);
                        med.WriteTrx(patient, allergyReaction["sevtxt"], allergyReaction["drug"], NCTNAME, comp, Convert.ToInt32(med.OrderUserId));
                    }
                }
            }

            // TODO: DRC information is written here in desktop PulseCheck. Currently not present in the mobile app.

            // Not needed in eMAR
            if (!string.IsNullOrWhiteSpace(med.Repeat) || !string.IsNullOrWhiteSpace(med.Time))
            {
                var lineParts = new List<string>();
                var timeDescription = med.GetMedTimeDescription();
                var useDescription = (!string.IsNullOrWhiteSpace(timeDescription) ? timeDescription : !string.IsNullOrWhiteSpace(med.Time) ? med.Time : "");
                if (!string.IsNullOrWhiteSpace(useDescription))
                {
                    lineParts.Add(string.Format("Schedule: {0}", useDescription));
                }
                if (!string.IsNullOrWhiteSpace(med.Repeat))
                {
                    lineParts.Add(med.Repeat);
                }
                medInfo.Append(string.Format("\n{0}", string.Join(" ", lineParts)));
            }

            if (!string.IsNullOrWhiteSpace(med.Notes))
            {
                medInfo.Append(string.Format("\nNotes: {0}", med.Notes));
            }

            if (med.OrderForUserId != null)
            {
                var physicianName = GetUserFullname((int)med.OrderForUserId);
                medInfo.Append(string.Format("\nOrdered By: {0}", physicianName));
            }

            var ordererName = GetUserFullname((int)med.OrderUserId);
            var orderDate = (new Time()).LongDateTime(med.OrderDate);

            if (med.OrderUserId != null)
            {
                medInfo.Append(string.Format("\nEntered By: {0} {1}", ordererName, orderDate));
            }

            if (pharmVerifStatus == 1)
            {
                var pharmVerifText = "Pharmacist Verification Needed";
                medInfo.Append(string.Format("\n{0}: Entered By {1} {2}\n", pharmVerifText, ordererName, orderDate));
            }

//            if (!string.IsNullOrWhiteSpace(med.Authentication) && MedicationActions.Constants.AUTH_TEXT.ContainsKey(med.Authentication))
//            {
//                medInfo.Append(" ");
//                medInfo.Append(MedicationActions.Constants.AUTH_TEXT[med.Authentication]);
//            }

            line.LineHeader.sys_time = sysDate;
            line.LineHeader.user = med.OrderUserId ?? 0;
            line.LineHeader.losecs = med.Losecs.ToString();         // TODO: Make sure EF actually updates this value after a save. Never trust.
            line.LinePart.nct = EMR.Constants.NCT_MED_SVC;
            line.LinePart.section = EMR.Constants.SECT_MED_SVC;
            line.LinePart.part = NCTNAME;
            line.DataSegments = new List<EMR.Line.DataSegment>
            {
                new EMR.Line.DataSegment(EMR.Line.DataSegment.Constants.TYPE_DROPDOWN, medInfo.ToString())
            };

            return line;
        }

        public PatientDataForIbex GetPatientDataForIbex(long patientId)
        {
            var query = (from p in _context.Patients
                         where p.Id == patientId
                         select new PatientDataForIbex
                         {
                             Department = p.DepartmentCode,
                             Ward = p.WardCode,
                             Bed = p.RoomBedCode,
                             FirstName = p.FirstName,
                             MiddleName = p.MiddleName,
                             LastName = p.LastName,
                             NameSuffix = p.NameSuffix,
                             Age = p.Age,
                             AgeUnits = p.AgeUnits,
                         }).ToList();

            return query.FirstOrDefault();
        }

        public string GetRoute(string internalRouteId)
        {
            int.TryParse(internalRouteId, out var intInternalRouteId);
            var query = from m in _context.MedicationRoutes
                        where m.Id == intInternalRouteId
                        select m.Name;

            return query.FirstOrDefault();
        }

        public string GetUnit(string internalUnitId)
        {
            int.TryParse(internalUnitId, out var intInternalUnitId);
            var query = from m in _context.MedicationUnits
                        where m.Id == intInternalUnitId
                        select m.Name;

            return query.FirstOrDefault();
        }

        public string GetUserFullname(int userId)
        {
            var query = from u in _context.Users
                        join e in _context.ExternalIds on u.Id equals e.InternalId
                        where e.ExternalId == userId.ToString()
                        && e.Entity == "users"
                        && e.Vendor == "pulsecheck"
                        select u.LastName + ", " + u.FirstName;

            return query.FirstOrDefault();
        }

        public string GetAllergyDrugIdFromPatientAllergyId(long patAllergyId)
        {
            var query = from pa in _context.PatientAllergies
                        where pa.Id == patAllergyId
                        select pa.AllergyDrugId;

            return query.FirstOrDefault();
        }

        public long GetPatientIdFromPatientOrderId(long patientOrderId)
        {
            var query = from p in _context.PatientOrders
                        where p.Id == patientOrderId
                        select p.PatientId;
            return query.FirstOrDefault();
        }

        public string GetFullNameFromUserId(int userId)
        {
            var query = from u in _context.Users
                        where u.Id == userId
                        select u.LastName + ", " + u.FirstName;

            return query.FirstOrDefault();
        }

        public string GetDrugNameFromHomeMed(long homeMedId)
        {
            var query = from m in _context.PatientHomeMedications
                        where m.Id == homeMedId
                        select m.Name;

            return query.FirstOrDefault();
        }

        public string GetDrugNameFromCartOrder(long cartOrderId)
        {
            var query = from pco in _context.PatientCartOrders
                        join md in _context.MedicationDetails on pco.MedicationId equals md.MedicationId
                        where pco.Id == cartOrderId
                        select md.BrandName;

            return query.FirstOrDefault();
        }

        public string GetDrugNameFromOrder(long orderId)
        {
            var query = from po in _context.PatientOrders
                        join md in _context.MedicationDetails on po.MedicationId equals md.MedicationId
                        where po.Id == orderId
                        select md.BrandName;

            return query.FirstOrDefault();
        }

        public string GetFrequencyNameFromId(int frequencyId)
        {
            var query = from f in _context.FrequencySchedules
                        where f.Id == frequencyId
                        select f.Name;
            return query.FirstOrDefault();
        }

        public string GetDurationUnitFromId(int? unitId)
        {
            var query = from d in _context.DurationUnits
                        where d.Id == unitId
                        select d.Name;
            return query.FirstOrDefault();
        }

        public string GetOverrideReason(int? overrideReasonId)
        {
            if (overrideReasonId == null)
                return null;
            var query = from or in _context.OverrideReasons
                        where or.Id == overrideReasonId
                        select or.Description;
            return query.FirstOrDefault();
        }

        public string GetInternalUserName(int userId)
        {
            var query = from u in _context.Users
                        where u.Id == userId
                        select u.LastName + ", " + u.FirstName;

            return query.FirstOrDefault();
        }

        public int GetCodeShareSite(byte siteId, string type)
        {
            var query = from cs in _context.SiteCodeShares
                        where cs.SourceSiteId == siteId && cs.Entity == type
                        select cs.TargetSiteId;
            var firstOrDefault = query.FirstOrDefault();

            return firstOrDefault > 0 ? firstOrDefault : siteId;
        }

        public int GetMedicationIdFromPatientOrderId(long patientOrderId)
        {
            var query = from p in _context.PatientOrders
                        where p.Id == patientOrderId
                        select p.MedicationId;
            return query.FirstOrDefault();
        }

        public string GetNDCFromPatientOrderId(long patientOrderId)
        {
            var query = from p in _context.PatientOrders
                        where p.Id == patientOrderId
                        select p.Ndc;
            return query.FirstOrDefault();
        }

        // retrieve DB Vendor using the site Id and use this to get the NDCs
        public List<string> GetNDCsFromBaseNDC(string baseNdc, byte siteId)
        {
            var dbVendor = GetDBVendor(siteId);
            IQueryable<string> query;
            switch (dbVendor)
            {
                case DrugDB.Constants.Vendors.FDB:
                    query = from f in _context.FdbNdcInfo
                                where f.BaseNdc == baseNdc
                                select f.Ndc;
                    break;
                default:
                    query = from f in _context.FdbNdcInfo
                            where f.BaseNdc == baseNdc
                            select f.Ndc;
                    break;
            }
            return query.ToList();
        }

        public string GetServiceCodesFromFormulary(int medicationId, int siteId)
        {
            var query = from f in _context.SiteFormulary
                        where f.MedicationId == medicationId && f.SiteId == siteId
                        select f.ServiceCode;
            return query.FirstOrDefault();
        }

        public string GetServiceCodesFromFormulary(string ndc, int siteId)
        {
            var query = from f in _context.SiteFormulary
                        where f.Ndc == ndc && f.SiteId == siteId
                        select f.ServiceCode;
            return query.FirstOrDefault();
        }

        public List<string> GetMedicationDetailsDrugIds(int medicationId)
        {
            var query = from m in _context.MedicationDetails
                        where m.MedicationId == medicationId
                        select m.DrugId;
            return query.ToList();
        }

        public int GetMedicationMedicationIds(string drugId)
        {
            var query = from m in _context.Medications
                        where m.DrugId == drugId
                        select m.Id;
            return query.FirstOrDefault();
        }

        public bool GetPharmVerificationReqStatus(long orderId, long patientId)
        {
            var query = from po in _context.PatientOrders
                        where po.PatientId == patientId && po.Id != orderId && po.PharmacyVerificationStatus == 1
                        // Need to revisit whether order status needs to be included and if so, how
//                        && (po.OrderStatus == OrderStatus.Pending.ToString() || po.OrderStatus == OrderStatus.OnGoing.ToString())
                        select po.PharmacyVerificationStatus;

            return query.FirstOrDefault() == (byte)1;
        }

        public string GetDBVendor(byte siteId, SqlConnection con = null)
        {
                var dbVendor = new DB.Select
                {
                    Connection = con,
                    Sql = "SELECT [dbo].[fnGetOrgOption](@siteId, @optName)",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@siteId", SqlDbType.TinyInt) { Value = siteId },
                        new SqlParameter("@optName", SqlDbType.VarChar) { Value = "DRUG_DB_VENDOR" }
                    }
                }.RunForScalar().ToString();

            return dbVendor;
        }

        public string GetMedDetailsId(string ibex, byte site, int losecs, bool isEmarMedAdmin, string brandName)
        {
            // isEmarMedAdmin is used to determine which table to query
            // brandName is used for combo meds when querying the med_details table (brandName == false)
            // When a combo med (brandName != null) and in the emar_med_administrations table (isEmarMedAdmin == true),
            //    it is expected that there is only one row in the table unlike when in the med_details table.
            var xtraWhere = brandName == null ? "" : " AND brand_name=@brandname";
            var sql = isEmarMedAdmin == false ? @"SELECT id FROM med_details WHERE ibex=@ibex AND site=@site AND losecs=@losecs" + xtraWhere
                                              : @"SELECT id FROM emar_med_administrations WHERE ibex=@ibex AND site=@site AND losecs=@losecs";

            int? medDetailsId = 0;
            try
            {
                var connection = DB.GetConnectionString();
                using (var con = new SqlConnection(connection))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@ibex", SqlDbType.Char).Value = ibex;
                        cmd.Parameters.Add("@site", SqlDbType.SmallInt).Value = site;
                        cmd.Parameters.Add("@losecs", SqlDbType.Int).Value = losecs;
                        if (brandName != null)
                            cmd.Parameters.Add("@brandname", SqlDbType.VarChar).Value = brandName;

                        con.Open();
                        medDetailsId = (int?)cmd.ExecuteScalar();
                        if (con.State == System.Data.ConnectionState.Open)
                            con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: Determine how to handle exceptions
//                return string.Format(
//                    "Error in GetMedDetailsId. Sql({0}) with values({1}) created error({2}).",
//                    sql,
//                    string.Join(";", ibex, site, losecs, brandName),
//                    e.Message
//                );
            }

            return medDetailsId.ToString();
        }

        public PatientOrderDataForMeds GetPatientOrderDataForMeds(long patientOrderId)
        {
            var query = (from po in _context.PatientOrders
                         where po.Id == patientOrderId
                         select new PatientOrderDataForMeds
                         {
                             PatentOrderId = patientOrderId,
                             orderingPhysicianId = po.OrderPhysicianUser.Id,
                             medicationId = po.MedicationId,
                             medNotes = po.OrderNotes,
                             Dose = po.Dose.ToString(),
                             Route = po.MedicationRouteId.ToString(),
                             Unit = po.MedicationUnitId.ToString(),
                             FrequencyId = (int)po.FrequencyScheduleId,
                             Duration = po.Duration,
                             DurationId = po.DurationUnitId,
                             OrderDate = po.AddDatetime.ToString("yyyyMMddHHmmss"),
                             AntiMicrobialIndication = po.AntimicrobialIndication != null ? po.AntimicrobialIndication.Code : "",
                             AntiMicrobialIndicationText = po.AntimicrobialIndication != null ? po.AntimicrobialIndication.Description : ""
                         }).ToList();

            return query.FirstOrDefault();
        }

        public class Constants
        {
            // --- CODE SHARING CONSTANTS --- ///
            public const string CODE_SHARE_ANTIMICROBIAL_INDICATIONS = "antimicrobial_indications";
            public const string CODE_SHARE_FORMULARY = "formulary";
            public const string CODE_SHARE_FREQUENCY_SCHEDULES = "frequency_schedules";
            public const string CODE_SHARE_INTERACTION_OVERRIDES = "interaction_overrides";
            public const string CODE_SHARE_MEDICATION_ROUTES = "medication_routes";
            public const string CODE_SHARE_MEDICATION_UNITS = "medication_units";
            public const string CODE_SHARE_ORDER_INSTRUCTIONS = "order_instructions";
            public const string CODE_SHARE_SERVICES = "services";
            public const string CODE_SHARE_VITAL_SIGNS = "vital_signs";
        }
    }
}
