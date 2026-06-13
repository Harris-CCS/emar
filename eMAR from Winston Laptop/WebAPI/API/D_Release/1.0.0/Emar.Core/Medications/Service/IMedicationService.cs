using System.Collections.Generic;
using Emar.Core.Medications.Model;

namespace Emar.Core.Medications.Service
{
    public interface IMedicationService
    {
        IEnumerable<BrandNameSearchDto> GetMedsByBrandName(int siteId, string search, int userId, EmarOrderType searchType,
            string deptCode, string schedulerDataRetrieveBase);
        IEnumerable<AntimicrobialIndicationDto> GetIndicationsBySite(int siteId);
        Dictionary<string, bool> GetSearchDropdownList(int siteId);
        MedicationDto GetMedication(int medicationId);
        IEnumerable<MedicationInteractionReaction> GetInteractionsReactions(int userId, long patientId, EmarOrderType itemType, int itemId);
    }
}