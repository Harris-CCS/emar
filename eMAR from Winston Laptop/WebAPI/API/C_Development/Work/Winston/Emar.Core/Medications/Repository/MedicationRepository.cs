using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Emar.Core.Medications.Repository
{
    public class MedicationRepository : IMedicationRepository
    {
        private readonly EmarContext _context;
        private readonly IOptionRepository _optionRepository;
        private string _vendor;
        private IDrugDbRepository _vendorRepository;
        private readonly MemoryCache _cache;

        public MedicationRepository(EmarContext context, IOptionRepository optionRepository, EmarMemoryCache cache)
        {
            _context = context;
            _optionRepository = optionRepository;
            _cache = cache.Cache;
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
            //
            //Updated By:   Winston Murdock
            //Update Date:  06/02/2021
            //Update:       Always show the "all" option.
            //*****************************************

            //The return variable.
            //Default deptpreferred, groups, and userquicklist to true since they will always be present.
            //Default all and formulary to false, since those are the ones we're calculating here.
            //If the logic ever needs to change (i.e. groups is not always true), then we've got all
            //the logic here.

            //We always want to show the "all" option per Romel at the 06/01/2021 standup.
            //Clients want the ability to search "all" even as they have a formulary and use that 99% of the time.
            //Thusly, set it to "true" here.
            //Winston Murdock, 06/02/2021.  EMAR-1043
            var searchDropdownList = new Dictionary<string, bool>
            {
                {"all", true},
                {"deptpreferred", true},
                {"formulary", false},
                {"groups", true},
                {"userquicklist", true}
            };

            //Get the Y/N for I/O/P.
            bool inpat = _optionRepository.GetOptionBool(siteId, OptionNames.MEDINPAT);
            bool outpat = _optionRepository.GetOptionBool(siteId, OptionNames.MEDOUTPAT);
            bool pyxis = _optionRepository.GetOptionBool(siteId, OptionNames.MEDPYXIS);

            //Determine whether or not to show the formulary option.
            //If any of the three, formulary flags are "Y", then show the formulary link.
            if (inpat || outpat || pyxis)
            {
                //At least one formulary option is true.
                //Show the formulary flag.
                searchDropdownList["formulary"] = true;
            } //end if
            else
            {
                //All three formulary flags are false.
                //However, if there are any rows in the site_formulary table, set formulary to true.
                //Else, leave formulary at its default value of false.
                searchDropdownList["formulary"] = _context.SiteFormulary.Where(i => i.SiteId == siteId).Any();
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

        public Medication GetMedicationByDrugId(string drugId)
        {
            //Get the medication for site -1 that matches on the drugId value
            //and that is not a combo med.
            //return _context.Medications.Where(m => m.DrugId == drugId && m.DrugId != "COMBO" && m.SiteId == -1).FirstOrDefault();


            //We need the MedicationDetails or else the interaction/reaction checking logic won't work.
            //I copied the .includes from the GetMedication method above.
            //Winston Murdock, 05/25/2022.  PC-27238
            var ret = _context.Medications
                .Include(med => med.MedicationDetails)
                    .ThenInclude(md => md.MedicationUnit)
                .Include(m => m.MedicationDetails)
                    .ThenInclude(md => md.FdbBrandName)
                .FirstOrDefault(m => m.DrugId == drugId && m.DrugId != "COMBO" && m.SiteId == -1);

            return ret;
        } //end GetMedicationByDrugId

        public List<PrnIndication> GetPrnIndicationsBySiteId(int siteId)
        {
            //Might filter by site id at some point.
            //For now, we'll just retrn all of them.
            //Although the forien key to sites is setup in the
            //Data project, we don't need the site info here.
            //Thus, we don't join to it here.
            //var ret = _context.PrnIndications.ToList();

            //Use a memory cache for this.
            //Make it site specific, even though we have one list in the table for all sites.
            //If they want site-specific lists in the future, we can implement code sharing, etc... at that point.
            var ret = _cache.GetOrCreate(siteId == 0 ? "All" : siteId + CacheKeys.PrnIndications, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var retInner = _context.PrnIndications.ToList();
                entry.Size = retInner.Count;
                return retInner;
            });

            return ret;
        } //end GetPrnIndicationBySiteId
    }
}