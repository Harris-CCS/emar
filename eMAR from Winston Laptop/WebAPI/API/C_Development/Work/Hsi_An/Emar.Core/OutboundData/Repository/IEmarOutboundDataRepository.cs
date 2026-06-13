using Emar.Core.OutboundData.Model;
using Emar.Data.Entities;
using System.Collections.Generic;

namespace Emar.Core.OutboundData.Repository
{
    public interface IEmarOutboundDataRepository
    {
        string GetExternalPatientId(long internalPatientId);
        int GetExternalUserId(int internalUserId);
        int GetExternalSiteId(int internalSiteId);
        string GetFdbBrandName(int internalMedId);
        string GetAmIndication(string internalAmId);
        string GetRoute(string internalRouteId);
        string GetUnit(string internalUnitId);
        OdsMedicationDetails GetMedicationDetails(int internalMedId);
        string GetComboName(int medicationId);
        List<int> GetMedicationDetailsIds(int medicationId);
        OdsMedicationDetails GetMedicationDetailsFromMedDetailsId(int detailsId);
    }
}
