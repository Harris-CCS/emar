using Emar.Core.Options.Repository;
using Emar.Data;
using System;
using System.Collections.Generic;

namespace Emar.Core.Medications.Repository
{
    public class MedicationRepository : IMedicationRepository
    {
        private readonly EmarContext _context;
        private readonly IOptionRepository _optionRepository;
        private string _vendor;
        private IDrugDbRepository _vendorRepository;

        public MedicationRepository(EmarContext context, IOptionRepository optionRepository)
        {
            _context = context;
            _optionRepository = optionRepository;
        }

        public IEnumerable<string> GetMedsByBrandName(int siteId, string search, int userId, Model.MedicationLookupDto.SearchType searchType)
        {
            //Figure out which vendor we're using.
            GetVendorRepository(siteId);
            IEnumerable<string> medsToReturn;

            //Go to the vendor-specific repository to actually do the search and apply the formulary filtering.
            medsToReturn = _vendorRepository.GetMedsByBrandName(siteId, search, userId, searchType);
            
            //Return the list of strings.
            return medsToReturn;
        } //end GetMedsByBrandName
        
        private void GetVendorRepository(int siteId)
        {
            //Figure out which drug vendor we're using.
            //Multum = "M"
            //FDB-US = "F"
            //FDB-CA = "1"
            //Medispan = "2"
            //Save the vendor code in a string variable.
            //And create an instance of the appropriate, vendor-specific repository.
            //As of now, we're only doing FDB.
            //We'll figure out the others in Phase 2.
            //Winston Murdock, 09/10/2020.
            _vendor = _optionRepository.GetOption(siteId, "DRUG_DB_VENDOR");

            if (_vendor == "M")
            {
                //Multum
                throw new NotImplementedException();
            }
            else if (_vendor == "F")
            {
                //FDB
                _vendorRepository = new DrugDbRepositoryFdb(_context, _optionRepository);
            }
            else if (_vendor == "1")
            {
                //FDB-CA
                throw new NotImplementedException();
            }
            else if (_vendor == "2")
            {
                //Medispan
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException("The drug vendor was not recognized.");
            } //end if
        } //end getVendorRepository
    }
}
