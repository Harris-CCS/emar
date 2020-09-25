using Emar.Data.Entities;
using System.Collections.Generic;


namespace Emar.Core.Medications.Repository
{
    public interface IDrugDbRepository
    {
        IEnumerable<string> GetMedsByBrandName(int siteId, string search, int UserId, Model.MedicationLookupDto.SearchType searchType);
    }
}
