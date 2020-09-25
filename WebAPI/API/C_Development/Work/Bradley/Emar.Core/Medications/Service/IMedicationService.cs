using Emar.Core.Medications.Model;
using System.Collections.Generic;

namespace Emar.Core.Medications.Service
{
    public interface IMedicationService
    {

        IEnumerable<string> GetMedsByBrandName(int siteId, string search, int userId, MedicationLookupDto.SearchType searchType);
    }
}
