using Emar.Core.Medications.Model;
using System.Collections.Generic;
using Emar.Core.Medications.Repository;

namespace Emar.Core.Medications.Service
{
    public partial class MedicationService : IMedicationService
    {
        private readonly IMedicationRepository _medicationRepository;
        
        public MedicationService(IMedicationRepository MedicationRepository)
        {
            _medicationRepository = MedicationRepository;
        }

        public IEnumerable<string> GetMedsByBrandName(int siteId, string search, int userId, MedicationLookupDto.SearchType searchType)
        {
            //throw new System.NotImplementedException();
            var records = _medicationRepository.GetMedsByBrandName(siteId, search, userId, searchType);

            //Return the list of strings.
            return records;
        }
    }
}
