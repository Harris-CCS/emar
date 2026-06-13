using Emar.Core.Helpers;
using Emar.Core.OutboundChart.Model;
//using Emar.Data.Entities;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emar.Core.OutboundChart.Repository
{
    public interface IEmarOutboundChartRepository
    {
//        Task<EMR.Line> ChartEntry(IPatient patient, Medication med, string sysDate, List<OverrideRationale> overrides);
        EMR.Line ChartEntry(IPatient patient, Medication med, string sysDate, byte pharmVerifStatus, string? prnIndication);
        string GetRoute(string internalRouteId);
        string GetUnit(string internalUnitId);
        string GetAllergyDrugIdFromPatientAllergyId(long patAllergyId);
        long GetPatientIdFromPatientOrderId(long patientOrderId);
        string GetFullNameFromUserId(int userId);
        string GetFrequencyNameFromId(int frequencyId);
        string GetDurationUnitFromId(int? unitId);
        string GetOverrideReason(int? overrideReasonId);
        string GetInternalUserName(int userId);
        int GetCodeShareSite(byte siteId, string type);
        int GetMedicationIdFromPatientOrderId(long patientOrderId);
        string GetNDCFromPatientOrderId(long patientOrderId);
        List<string> GetNDCsFromBaseNDC(string baseNdc, byte siteId);
        string GetServiceCodesFromFormulary(int medicationId, int siteId);
        string GetServiceCodesFromFormulary(string ndc, int siteId);
        List<string> GetMedicationDetailsDrugIds(int medicationId);
        int GetMedicationMedicationIds(string drugId);
        PatientDataForIbex GetPatientDataForIbex(long patientId);
        bool GetPharmVerificationReqStatus(long orderId, long patientId);
        string GetDBVendor(byte siteId, SqlConnection con = null);
        string GetMedDetailsId(string ibex, byte site, int losecs, bool isEmarMedAdmin, string brandName);
        PatientOrderDataForMeds GetPatientOrderDataForMeds(long patientOrderId);
        string GetDispoTypeCodeFromPatient(long patientId);
        List<EMR.Line> ChartVitalsEntries(int userId, string losecs, string enteredTime, OcsVitals vitals);
    }
}

