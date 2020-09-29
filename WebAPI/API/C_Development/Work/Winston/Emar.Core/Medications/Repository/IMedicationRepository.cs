using System.Collections.Generic;


namespace Emar.Core.Medications.Repository
{
    public interface IMedicationRepository
    {
        IEnumerable<string> GetMedsByBrandName(int siteId, string search, int userId, Model.MedicationLookupDto.SearchType searchType, string deptCode);
    }
}
