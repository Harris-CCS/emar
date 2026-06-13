using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Repository
{
    public interface IMedicationRepository
    {
        IEnumerable<BrandNameReturnDto> GetMedsByBrandName(int siteId, string search, int userId, EmarOrderType searchType, string deptCode);
        IEnumerable<AntimicrobialIndication> GetIndicationsBySite(int siteId);
        Dictionary<string, bool> GetSearchDropdownList(int siteId);
        Medication GetMedication(int medicationId);
    }
}