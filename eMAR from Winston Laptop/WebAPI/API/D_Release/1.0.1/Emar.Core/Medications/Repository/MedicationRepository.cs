using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Medications.Model;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

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

        public IEnumerable<BrandNameReturnDto> GetMedsByBrandName(int siteId, string search, int userId, EmarOrderType searchType, string deptCode)
        {
            //Figure out which vendor we're using.
            GetVendorRepository(siteId);
            IEnumerable<BrandNameReturnDto> medsToReturn;

            //Go to the vendor-specific repository to actually do the search and apply the formulary filtering.
            medsToReturn = _vendorRepository.GetMedsByBrandName(siteId, search, userId, searchType, deptCode);

            //Return the list of strings.
            return medsToReturn;
        } //end GetMedsByBrandName

        public IEnumerable<AntimicrobialIndication> GetIndicationsBySite(int siteId)
        {
            //*****************************************
            //Name:         GetIndicationsBySite
            //Author:       Winston Murdock
            //Date:         09/30/2020
            //Purpose:      Return all indications for the current site.
            //
            //Params:
            //siteId - The ID of the site that the user is logged into (sites.id)
            //*****************************************

            //Return the list, sorting by ordinal position.
            return _context.AntimicrobialIndications.Where(i => i.SiteId == siteId).OrderBy(i => i.OrdinalPosition).ToList();
        } //end GetIndicationsBySite

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
            _vendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);

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

        public Dictionary<string, bool> GetSearchDropdownList(int siteId)
        {
            //*****************************************
            //Name:         GetSearchDropdownList
            //Author:       Winston Murdock
            //Date:         10/01/2020
            //Purpose:      Return true/false for each of the options on the search type dropdown.
            //
            //Params:
            //siteId - The ID of the site that the user is logged into.
            //*****************************************

            //The return variable.
            //Default deptpreferred, groups, and userquicklist to true since they will always be present.
            //Default all and formulary to false, since those are the ones we're calculating here.
            //If the logic ever needs to change (i.e. groups is not always true), then we've got all
            //the logic here.
            var searchDropdownList = new Dictionary<string, bool>
            {
                {"all", false},
                {"deptpreferred", true},
                {"formulary", false},
                {"groups", true},
                {"userquicklist", true}
            };

            //Get the Y/N for I/O/P.
            bool inpat = _optionRepository.GetOptionBool(siteId, OptionNames.MEDINPAT);
            bool outpat = _optionRepository.GetOptionBool(siteId, OptionNames.MEDOUTPAT);
            bool pyxis = _optionRepository.GetOptionBool(siteId, OptionNames.MEDPYXIS);

            //If any of the three, formulary flags are "Y", then show the formulary link.
            if (inpat || outpat || pyxis)
            {
                //Formulary search only.
                searchDropdownList["formulary"] = true;
            } //end if
            else
            {
                //If there are rows in the site_formulary table, set formulary to true.
                //Else, set formulary to false.
                searchDropdownList["formulary"] = _context.SiteFormulary.Where(i => i.SiteId == siteId).Any();

                //Set all to true.
                searchDropdownList["all"] = true;
            } //end if

            return searchDropdownList;
        } //end GetSearchDropdownList

        public Medication GetMedication(int medicationId)
        {
            return _context.Medications
                .Include(m => m.MedicationDetails)
                    .ThenInclude(md => md.MedicationUnit)
                .Include(m => m.MedicationDetails)
                    .ThenInclude(md => md.FdbBrandName)
                .FirstOrDefault(m => m.Id == medicationId);
        }
    }
}