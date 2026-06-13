using Emar.Core.Medications.Model;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace Emar.Core.Medications.Repository
{
    public class DrugDbRepositoryFdb : IDrugDbRepository
    {
        //As of 04/19/2021, there is no FDB-specific logic in this file.
        //This was achived by moving the formulary match calculations out to the DB.
        //I still want to keep this vendor-specific repository in case we ever need
        //to have vendor-specific repository 
        //Winston Murdock, 04/19/2021.

        private readonly EmarContext _context;
        private readonly IOptionRepository _optionRepository;
        private bool _inpat;
        private bool _outpat;
        private bool _pyxis;
        private bool _exactMatch;
        private int _siteId;

        public DrugDbRepositoryFdb(EmarContext context, IOptionRepository optionRepository)
        {
            _context = context;
            _optionRepository = optionRepository;
        }
        
        public IEnumerable<BrandNameReturnDto> GetMedsByBrandName(int siteId, string search, int userId, EmarOrderType searchType, string deptCode)
        {
            //*****************************************
            //Name:         GetMedsByBrandName
            //Author:       Winston Murdock
            //Date:         09/23/2020 - 09/29/2020
            //Purpose:      Peform the medication search.
            //
            //Params:
            //siteId - The ID of the site that the user is logged into (sites.id)
            //search - The medication name we are searching for
            //userId - The ID of the logged in user (users.id)
            //searchType - The type of search being performed (all, formulary,
            //              group, department preferred list, or user quick list
            //deptCode - the department that we're pulling the preferred list for
            //              Main ED, Fast Track, etc...
            //Notes:
            //          The formulary filtering logic has been blessed by Romel
            //              and is documented in Confluence.
            //https://edpulsecheck.atlassian.net/wiki/spaces/EDPC/pages/1365475342/PulseCheck+ED+Formulary+Calculation+Explanation
            //*****************************************

            //Copy the siteId into a module-level variable
            //so that we don't have to pass it into the helper methods.
            _siteId = siteId;

            //Get the Y/N for I/O/P and exact match.
            _inpat = _optionRepository.GetOptionBool(siteId, OptionNames.MEDINPAT, false);
            _outpat = _optionRepository.GetOptionBool(siteId, OptionNames.MEDOUTPAT, false);
            _pyxis = _optionRepository.GetOptionBool(siteId, OptionNames.MEDPYXIS, false);
            _exactMatch = _optionRepository.GetOptionBool(siteId, OptionNames.MEDEXACTMATCH, false);

            //If this is an "all" search, or if all of the formulary filters are off,
            //then return all medications regardless of their formulary status.
            //Return a BrandNameReturnDTO so that the match level numbers can be returned as well.
            //Since this search did not look in the formulary match table, use 0's for those values.
            //Call .Distinct to remove any duplicates.
            //Then call .OrderBy to sort the list alphabetically.
            //If this is a group search, then we aren't doing any formulary filtering.
            //We're accepting their group items as they have set them up.
            //Winston Murdock, 09/25/2022.  PC-27536
            //if ((searchType == EmarOrderType.All) || (!_inpat && !_outpat && !_pyxis))
            if
                (
                    (searchType == EmarOrderType.All) ||
                    (!_inpat && !_outpat && !_pyxis) ||
                    (searchType == EmarOrderType.GroupRememberedOrder)
                )
            {
                //This is being changed from a string to a MedicationLookup
                //We have to do this to be able to have/access the IsBrandNameMatch and SearchPos fields.
                //Winston Murdock, 02/03/2021.
                
                //Adding IsGroupItem and GroupItemId (which has been stored in MedicationId in the MedicationLookup object).
                //Winston Murdock, 09/25/2022.  PC-27536
                var nonFormSearchResults = NonFormularySearchResults(searchType, search, userId, deptCode);
                var ret = nonFormSearchResults.Select(i => new BrandNameReturnDto
                {
                    BrandName = i.BrandName,
                    InpatientMatch = 0,
                    OutpatientMatch = 0,
                    PyxisMatch = 0,
                    MatchLevel = 0,
                    IsBrandNameMatch = i.IsBrandNameMatch,
                    SearchPos = i.SearchPos,
                    IsGroupItem = i.IsGroupItem,
                    GroupItemId = ShouldWeReturnTheMedicationId(i.IsGroupItem, i.MedicationId)
                    //Order by IsBrandNameMatch descending (since true is 1 and false is 0)
                    //and then by the position of the search string within the brand name (or ingredient list).
                    //Winston Murdock, 01/22/2021.  EMAR-586
                    //The .GroupBy(BrandName).Select(FirstOrFDefault) lets me basically do a
                    //SELECT DISTINCT(BrandName).  This removes any duplicates from the results.
                    //https://stackoverflow.com/a/14321048
                    //Winston Murdock, 02/03/2021.  EMAR-586
                    //}).Distinct().OrderByDescending(i => i.IsBrandNameMatch).ThenBy(i => i.SearchPos);
                    //}).GroupBy(x => x.BrandName).Select(x => x.FirstOrDefault()).OrderByDescending(i => i.IsBrandNameMatch).ThenBy(i => i.SearchPos).OrderBy(s => s.BrandName);
                    //Moving the ordering logic up to the service (where it should have been anyways).
                    //Winston Murdock, 03/11/2021.  EMAR-828.
                }).GroupBy(x => x.BrandName).Select(x => x.FirstOrDefault());

                return ret;

            } //end if

            //If we haven't returned yet, then this needs to be a formulary search.

            //Do the specified search and store the results into a local variable.
            List<MedicationLookup> meds = FormularySearchCandidates(searchType, search, deptCode);

            //Apply the formulary filtering to the list.
            //Also calculcate the match level for any medications
            //in the list that we haven't calculated it for yet.
            //For each med in the list, see if it's "match" levels qualify it to show
            //up in the search results or not.
            //We aren't calculating match levels in this method as the DB has already done that.
            //Winston Murdock, 03/21/2021.  EMAR-837.
            var brandNameCandidates = ApplyFormularyFilterToList(meds);

            //Return the brand name from the list of medications that passed the formulary filtering logic.
            //Return a BrandNameReturnDTO so that the match level numbers can be returned as well.
            //Call .Distinct to remove any duplicates.
            //Then call .OrderBy to sort the list alphabetically.
            return brandNameCandidates.Select(i => new BrandNameReturnDto
            {
                BrandName = i.BrandName,
                InpatientMatch = i.InpatientMatch,
                OutpatientMatch = i.OutpatientMatch,
                PyxisMatch = i.PyxisMatch,
                IsBrandNameMatch = i.IsBrandNameMatch,
                SearchPos = i.SearchPos,

                //Calculate the maximum match level value between the individual values.
                //Winston Murdock, 01/22/2021.  EMAR-586
                //We aren't using this for the sort anymore (sorting by brand name match
                //and then position within the string).
                //But it's nice to have for testing purposes.
                //Winston Murdock, 02/03/2021.
                MatchLevel = CalcMaxMatch(i.InpatientMatch, i.OutpatientMatch, i.PyxisMatch),

                IsGroupItem = i.IsGroupItem

                //Order by IsBrandNameMatch descending (since true is 1 and false is 0)
                //and then by the position of the search string within the brand name (or ingredient list).
                //Winston Murdock, 01/22/2021.  EMAR-586
                //The .GroupBy(BrandName).Select(FirstOrFDefault) lets me basically do a
                //SELECT DISTINCT(BrandName).  This removes any duplicates from the results.
                //https://stackoverflow.com/a/14321048
                //Winston Murdock, 02/03/2021.  EMAR-586
                //Add match level and brand name to the order.
                //Winston Murdock, 03/10/2021.  EMAR-828
                //Moving the sorting logic up to the service (where it should have been anyways).
                //Winston Murdock, 03/11/2021.  EMAR-828.
                //}).Distinct().OrderByDescending(i => i.IsBrandNameMatch).ThenBy(i => i.SearchPos);
            }).GroupBy(x => x.BrandName).Select(x => x.FirstOrDefault());
    } //end function GetMedsByBrandName

        private List<MedicationLookup> FormularySearchCandidates(EmarOrderType searchType, string search, string deptCode)
        {
            //We've moved the formulary match calculation into the DB layer.
            //Thus, we don't need to do it in the API.
            //I'm commenting out the second call in each of the cases in the switch
            //so that we don't pull in medications that are not in the match table.
            //Winston Murdock, 03/21/2021. EMAR-837
            List<MedicationLookup> ret = null;

            //Perform the specified search based on the type.
            //We'll apply the formulary filter later on.
            switch (searchType)
            {
                case EmarOrderType.DepartmentPreferredListItem:
                    //Perform the "prefered list" search.
                    //join to site_formulary_match (to only include medications that
                    //are already in the match table for this site) and to get the "match" values from it.
                    //EMAR-320.  Winston Murdock, 11/04/2020

                    if (deptCode == null)
                    {
                        //Do not filter by department code.
                        ret =
                            (from m in _context.Medications
                             join md in _context.MedicationDetails on m.Id equals md.MedicationId
                             join sfm in _context.SiteFormularyMatch
                                 on new { MedicationId = m.Id, SiteId = _siteId }
                                 equals new { sfm.MedicationId, sfm.SiteId }
                             join dpl in _context.DepartmentPreferredListItems
                                 on new { MedicationId = m.Id, SiteId = _siteId }
                                 equals new { dpl.MedicationId, dpl.SiteId }
                             where md.IsActive
                                   && sfm.SiteId == _siteId
                                   &&
                                   (
                                       EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                       EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                                       EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                                   )
                             select new MedicationLookup
                             {
                                 BrandName = md.BrandName,
                                 DrugId = md.DrugId,
                                 MedicationId = md.MedicationId,
                                 InpatientMatch = sfm.InpatientMatch,
                                 OutpatientMatch = sfm.OutpatientMatch,
                                 PyxisMatch = sfm.PyxisMatch,
                                 //These two lines let me do one DB hit and still set these values appropriately.
                                 //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                 //Else, set IsBrandNameMatch to false.
                                 //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                 //Winston Murdock, 02/03/2021.
                                 IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                                 SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search),
                                 IsGroupItem = false
                             }).Distinct().ToList();
                    }
                    else // you do have a department code
                    {
                        //Filter on department code.
                        ret =
                            (from m in _context.Medications
                             join md in _context.MedicationDetails on m.Id equals md.MedicationId
                             join sfm in _context.SiteFormularyMatch 
                                 on new {MedicationId = m.Id, SiteId = _siteId} 
                                 equals new {sfm.MedicationId, sfm.SiteId}
                             join dpl in _context.DepartmentPreferredListItems 
                                 on new { MedicationId = m.Id , SiteId = _siteId,  DeptCode = deptCode}
                                 equals new {  dpl.MedicationId, dpl.SiteId, DeptCode = dpl.DepartmentCode}
                             where md.IsActive
                                   && sfm.SiteId == _siteId
                                   &&
                                   (
                                       EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                       EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                                       EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                                   )
                             select new MedicationLookup
                             {
                                 BrandName = md.BrandName,
                                 DrugId = md.DrugId,
                                 MedicationId = md.MedicationId,
                                 InpatientMatch = sfm.InpatientMatch,
                                 OutpatientMatch = sfm.OutpatientMatch,
                                 PyxisMatch = sfm.PyxisMatch,
                                 //These two lines let me do one DB hit and still set these values appropriately.
                                 //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                 //Else, set IsBrandNameMatch to false.
                                 //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                 //Winston Murdock, 02/03/2021.
                                 IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                                 SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search),
                                 IsGroupItem = false
                             }).Distinct().ToList();
                    } //end if (do we have a department code?)
                    break;

                //Group searches won't need formulary filtering.  We should never get here, so comment this out.
                //case EmarOrderType.GroupRememberedOrder:
                //    //Perform the "groups" search.
                //    //join to site_formulary_match (to only include medications that
                //    //are already in the match table for this site) and to get the "match" values from it.
                //    //EMAR-321.  Winston Murdock, 11/04/2020

                //    ret =
                //    (from m in _context.Medications
                //        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                //        join sfm in _context.SiteFormularyMatch
                //            on new { MedicationId = m.Id, SiteId = _siteId }
                //            equals new { sfm.MedicationId, sfm.SiteId }
                //        join gli in _context.GroupListItems
                //            on new { MedicationId = m.Id, SiteId = _siteId }
                //            equals new { gli.MedicationId, gli.SiteId }
                //        where md.IsActive
                //            && sfm.SiteId == _siteId
                //            &&
                //            (
                //                EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                //                EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                //                EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                //            )
                //        select new MedicationLookup
                //        {
                //            BrandName = md.BrandName,
                //            DrugId = md.DrugId,
                //            MedicationId = md.MedicationId,
                //            InpatientMatch = sfm.InpatientMatch,
                //            OutpatientMatch = sfm.OutpatientMatch,
                //            PyxisMatch = sfm.PyxisMatch,
                //            //These two lines let me do one DB hit and still set these values appropriately.
                //            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                //            //Else, set IsBrandNameMatch to false.
                //            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                //            //Winston Murdock, 02/03/2021.
                //            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                //            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                //        }).Distinct().ToList();
                //    break;


                case EmarOrderType.UserQuickListItem:
                    //Perform the "user quicklist" search.
                    //join to site_formulary_match (to only include medications that
                    //are already in the match table for this site) and to get the "match" values from it.
                    //EMAR-319.  Winston Murdock, 11/04/2020

                    //Grab all items that match on the brand name.
                    ret =
                    (from m in _context.Medications
                        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                        join sfm in _context.SiteFormularyMatch
                            on new { MedicationId = m.Id, SiteId = _siteId }
                            equals new { sfm.MedicationId, sfm.SiteId }
                        join uqli in _context.UserQuickListItems
                            on new { MedicationId = m.Id, SiteId = _siteId }
                            equals new { uqli.MedicationId, uqli.SiteId }
                        where md.IsActive
                            && sfm.SiteId == _siteId
                            &&
                            (
                                EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                                EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                            )
                        select new MedicationLookup
                        {
                            BrandName = md.BrandName,
                            DrugId = md.DrugId,
                            MedicationId = md.MedicationId,
                            InpatientMatch = sfm.InpatientMatch,
                            OutpatientMatch = sfm.OutpatientMatch,
                            PyxisMatch = sfm.PyxisMatch,
                            //These two lines let me do one DB hit and still set these values appropriately.
                            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                            //Else, set IsBrandNameMatch to false.
                            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                            //Winston Murdock, 02/03/2021.
                            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search),
                            IsGroupItem = false
                        }).Distinct().ToList();
                    break;
                default:
                    //Perform the "all" search.
                    //join to site_formulary_match (to only include medications that
                    //are already in the match table for this site) and to get the "match" values from it.
                    //Winston Murdock, 11/04/2020

                    ret =
                    (from m in _context.Medications
                        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                        join sfm in _context.SiteFormularyMatch
                            on new { MedicationId = m.Id, SiteId = _siteId }
                            equals new { sfm.MedicationId, sfm.SiteId }
                        where md.IsActive
                            && sfm.SiteId == _siteId
                            &&
                            (
                                EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                                EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                            )
                        select new MedicationLookup
                        {
                            BrandName = md.BrandName,
                            DrugId = md.DrugId,
                            MedicationId = md.MedicationId,
                            InpatientMatch = sfm.InpatientMatch,
                            OutpatientMatch = sfm.OutpatientMatch,
                            PyxisMatch = sfm.PyxisMatch,
                            //These two lines let me do one DB hit and still set these values appropriately.
                            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                            //Else, set IsBrandNameMatch to false.
                            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                            //Winston Murdock, 02/03/2021.
                            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search),
                            IsGroupItem = false
                        }).Distinct().ToList();
                    break;
            } //end switch (search type)

            return ret;
        } //end FormularySearchCandidates

        //private IEnumerable<string> NonFormularySearchResults(EmarOrderType searchType, string searchString, int userId,
        //    string deptCode)
        private IEnumerable<MedicationLookup> NonFormularySearchResults(EmarOrderType searchType, string searchString, int userId,
            string deptCode)
        {
            //Changed this to return a list of MedicationLookup objects rather than a list of strings.
            //This was done so that we could calculate/return IsBrandNameMatch and SearchPos.
            //Winston Murdock, 02/03/2021.
            //Return variable.
            List<MedicationLookup> ret = null;

            switch (searchType)
            {
                //case EmarOrderType.All:
                    // The "All" Query is the same as the "else" catch-all query, so putting it in the default case

                case EmarOrderType.DepartmentPreferredListItem:
                    //Perform the "prefered list" search.
                    //EMAR-320.  Winston Murdock, 09/29/2020
                    //If we have a department code, then include it in the filter.
                    //Else, don't include it in the filter.
                    if (deptCode == null)
                    {
                        //We don't have a department code.
                        //Do not filter based on department code.
                        ret = (from m in _context.Medications
                                   //Join to the Medication Details table
                               join md in _context.MedicationDetails on m.Id equals md.MedicationId
                               //Join to the group_list_items table.
                               join dpli in _context.DepartmentPreferredListItems
                                  on new { MedicationId = m.Id, SiteId = _siteId }
                                  equals new { dpli.MedicationId, dpli.SiteId }
                                         //Where the drug vendor matches
                               where m.DrugVendor == "F"
                                                //and the length of the brand name is greater than zero.
                                                && md.BrandName.Length > 0
                                                //and where the SiteId = -1 (i.e. this is not a combo med)
                                                && m.SiteId == -1
                                                //and where the medication is active
                                                && md.IsActive
                                                //and one, or more, of these is true.
                                                &&
                                                (
                                                    EF.Functions.Like(md.BrandName, $"%{searchString}%") || //anywhere in brand name
                                                    EF.Functions.Like(md.ActiveList, $"{searchString}%") || //at the start of active list
                                                    EF.Functions.Like(md.ActiveList, $"%/ {searchString}%") //after a / and space in active list
                                                )
                               //Order by the brand name.
                               orderby md.BrandName
                               //Select the MedicationDetails table.
                               //We'll use a mapper to grab only the columns we need and store into a DTO later on.
                               select new MedicationLookup
                               {
                                   BrandName = md.BrandName,
                                   DrugId = md.DrugId,
                                   MedicationId = md.MedicationId,
                                   InpatientMatch = 0,
                                   OutpatientMatch = 0,
                                   PyxisMatch = 0,
                                   //These two lines let me do one DB hit and still set these values appropriately.
                                   //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                   //Else, set IsBrandNameMatch to false.
                                   //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                   //Winston Murdock, 02/03/2021.
                                   IsBrandNameMatch = md.BrandName.ToLower().IndexOf(searchString.ToLower()) != -1,
                                   SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, searchString),
                                   IsGroupItem = false
                               }).ToList();
                    }
                    else
                    {
                        //We do have a department code.
                        //Use it in the filter.

                        //Grab all items that match on the brand name.
                        ret = (from m in _context.Medications
                                    //Join to the Medication Details table
                                join md in _context.MedicationDetails on m.Id equals md.MedicationId
                                //Join to the group_list_items table.
                                join dpli in _context.DepartmentPreferredListItems
                                   on new { MedicationId = m.Id, SiteId = _siteId }
                                   equals new { dpli.MedicationId, dpli.SiteId }
                                   //Where the drug vendor matches
                                where m.DrugVendor == "F"
                                                 //and the length of the brand name is greater than zero.
                                                 && md.BrandName.Length > 0
                                                 //and where the department code matches.
                                                 && dpli.DepartmentCode == deptCode
                                                 //and where the SiteId = -1 (i.e. this is not a combo med)
                                                 && m.SiteId == -1
                                                 //and where the medication is active
                                                 && md.IsActive
                                                 //and one, or more, of these is true.
                                                 &&
                                                 (
                                                     EF.Functions.Like(md.BrandName, $"%{searchString}%") || //anywhere in brand name
                                                     EF.Functions.Like(md.ActiveList, $"{searchString}%") || //at the start of active list
                                                     EF.Functions.Like(md.ActiveList, $"%/ {searchString}%") //after a / and space in active list
                                                 )
                                //Order by the brand name.
                                orderby md.BrandName
                               //Select the MedicationDetails table.
                               //We'll use a mapper to grab only the columns we need and store into a DTO later on.
                               select new MedicationLookup
                               {
                                   BrandName = md.BrandName,
                                   DrugId = md.DrugId,
                                   MedicationId = md.MedicationId,
                                   InpatientMatch = 0,
                                   OutpatientMatch = 0,
                                   PyxisMatch = 0,
                                   //These two lines let me do one DB hit and still set these values appropriately.
                                   //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                   //Else, set IsBrandNameMatch to false.
                                   //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                   //Winston Murdock, 02/03/2021.
                                   IsBrandNameMatch = md.BrandName.ToLower().IndexOf(searchString.ToLower()) != -1,
                                   SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, searchString),
                                   IsGroupItem = false
                               }).ToList();
                    } //end if (deptCode == null)

                    break;
                case EmarOrderType.GroupRememberedOrder:
                    //Prior to this ticket, the group search was looking that drug inside any groups
                    //for this site.  If I do a group search for "coumadin" then this would only
                    //return coumadin if it were in a group.
                    //This was our best understanding as of fall, 2020.
                    //Further conversations with Emerus have led us to a better understanding.
                    //If I search for "coumadin" then I want to return the name and pathway
                    //of any groups for this site that have coumadin in the name of the group
                    //(i.e. medications.display_name).  This will return group name - pathway name
                    //(medications.display_name - group_list_items.group_name).  This will also
                    //return a flag letting the UI know that this search result is a group item
                    //and not a medication detail name.  Lastly, this will return the ID of the
                    //group item. The UI will need to load the scheduler options page as if the
                    //user had just clicked the group name from Med Svc.
                    //Winston Murdock, 09/25/2022.  PC-27536
                    ret = (from m in _context.Medications
                            //Join to the group_list_items table.
                           join gli in _context.GroupListItems on  m.Id equals gli.MedicationId
                               //Where the drug vendor matches
                           where m.DrugVendor == "F"
                                    //and the length of the brand name is greater than zero.
                                    && m.DisplayName.Length > 0
                                    //Only pull the group items for the current site.
                                    //This will pull both combo meds (medications.site_id = 31)
                                    //and regular meds (medications.site_id = -1)
                                    && gli.SiteId == _siteId
                                    //and one, or more, of these is true.
                                    && EF.Functions.Like(m.DisplayName, $"%{searchString}%") //anywhere in display name.

                                        
                           //Order by the display name.
                           orderby m.DisplayName
                           //We'll use a mapper to grab only the columns we need and store into a DTO later on.
                           select new MedicationLookup
                           {
                               BrandName = m.DisplayName + " - " + gli.GroupName,
                               DrugId = "0",
                               MedicationId = gli.Id,
                               InpatientMatch = 0,
                               OutpatientMatch = 0,
                               PyxisMatch = 0,
                               //These two lines let me do one DB hit and still set these values appropriately.
                               //If the search string is found in the display name, then set IsBrandNameMatch to true.
                               //Else, set IsBrandNameMatch to false.
                               //Then set SearchPos based on where the seach string is in the display name.
                               //Winston Murdock, 02/03/2021.
                               IsBrandNameMatch = m.DisplayName.ToLower().IndexOf(searchString.ToLower()) != -1,
                               SearchPos = CalcSearchPos(m.DisplayName, "", searchString),
                               IsGroupItem = true
                           }).ToList();
                    break;
                case EmarOrderType.UserQuickListItem:
                    //Perform the "user quicklist" search.
                    //EMAR-321.  Winston Murdock, 09/24/2020
                    ret = (from m in _context.Medications
                                //Join to the Medication Details table
                            join md in _context.MedicationDetails on m.Id equals md.MedicationId
                            //Join to the group_list_items table.
                            join uqli in _context.UserQuickListItems //on m.Id equals uqli.MedicationId
                               on new { MedicationId = m.Id, SiteId = _siteId }
                               equals new { uqli.MedicationId, uqli.SiteId }
                               //Where the drug vendor matches
                            where m.DrugVendor == "F"
                                             //and the user id matches.
                                             && uqli.UserId == userId
                                             //and the length of the brand name is greater than zero.
                                             && md.BrandName.Length > 0
                                             //and where the SiteId = -1 (i.e. this is not a combo med)
                                             && m.SiteId == -1
                                             //and where the medication is active
                                             && md.IsActive
                                             //and one, or more, of these is true.
                                             &&
                                             (
                                                 EF.Functions.Like(md.BrandName, $"%{searchString}%") || //anywhere in brand name
                                                 EF.Functions.Like(md.ActiveList, $"{searchString}%") || //at the start of active list
                                                 EF.Functions.Like(md.ActiveList, $"%/ {searchString}%") //after a / and space in active list
                                             )
                            //Order by the brand name.
                            orderby md.BrandName
                            //Select the MedicationDetails table.
                            //We'll use a mapper to grab only the columns we need and store into a DTO later on.
                            select new MedicationLookup
                            {
                                BrandName = md.BrandName,
                                DrugId = md.DrugId,
                                MedicationId = md.MedicationId,
                                InpatientMatch = 0,
                                OutpatientMatch = 0,
                                PyxisMatch = 0,
                                //These two lines let me do one DB hit and still set these values appropriately.
                                //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                //Else, set IsBrandNameMatch to false.
                                //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                //Winston Murdock, 02/03/2021.
                                IsBrandNameMatch = md.BrandName.ToLower().IndexOf(searchString.ToLower()) != -1,
                                SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, searchString),
                                IsGroupItem = false
                            }).ToList();
                    break;
                default:
                    //Perform the "all" search.
                    //Since we aren't doing formulary filtering, we don't need
                    //anything from the site_formulary_match table.
                    //We just use medications and medication_details to return values.
                    ret = (from m in _context.Medications
                                //Join to the Medication Details table
                            join md in _context.MedicationDetails on m.Id equals md.MedicationId
                            //Where the drug vendor matches
                            where m.DrugVendor == "F"
                            //and the length of the brand name is greater than zero.
                            && md.BrandName.Length > 0
                            //and where the SiteId = -1 (i.e. this is not a combo med)
                            && m.SiteId == -1
                            //and where the medication is active
                            && md.IsActive
                            //and one, or more, of these is true.
                            &&
                            (
                                EF.Functions.Like(md.BrandName, $"%{searchString}%") || //anywhere in brand name
                                EF.Functions.Like(md.ActiveList, $"{searchString}%") || //at the start of active list
                                EF.Functions.Like(md.ActiveList, $"%/ {searchString}%") //after a / and space in active list
                            )
                           //Order by the brand name.
                           orderby md.BrandName
                            //Select the MedicationDetails table.
                            //We'll use a mapper to grab only the columns we need and store into a DTO later on.
                            select new MedicationLookup
                            {
                                BrandName = md.BrandName,
                                DrugId = md.DrugId,
                                MedicationId = md.MedicationId,
                                InpatientMatch = 0,
                                OutpatientMatch = 0,
                                PyxisMatch = 0,
                                //These two lines let me do one DB hit and still set these values appropriately.
                                //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                //Else, set IsBrandNameMatch to false.
                                //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                //Winston Murdock, 02/03/2021.
                                IsBrandNameMatch = md.BrandName.ToLower().IndexOf(searchString.ToLower()) != -1,
                                SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, searchString),
                                IsGroupItem = false
                            }).ToList();
                        
                    break;
            } //end switch (search type)

            //Retuen the list of MedicationLookup objects.
            return ret;
        } //end NonFormularySearchResults

        private List<MedicationLookup> ApplyFormularyFilterToList(List<MedicationLookup> meds)
        {
            // The parameter list only has meds in the match table here (since we moved the match calculation into the DB).

            // Figure out if we're doing exact match or not and set matchLevel accordingly.
            int matchLevel = _exactMatch ? 3 : 1;

            //Return variable.
            var retList = new List<MedicationLookup>();

            //If the site is doing inpatient filtering, then add the medications
            //that match on inpatient to the return list.
            if (_inpat)
            {
                retList.AddRange
                (
                    meds
                        .Where(m => m.InpatientMatch >= matchLevel)
                );
            } //end if

            //If the site is doing outpatient filtering, then add the medications
            //that match on outpatient to the return list.
            if (_outpat)
            {
                retList.AddRange
                (
                    meds
                        .Where(m => m.OutpatientMatch >= matchLevel)
                );
            } //end if

            //If the site is doing pyxis filtering, then add the medications
            //that match on pyxis to the return list.
            if (_pyxis)
            {
                retList.AddRange
                (
                    meds
                        .Where(m => m.PyxisMatch >= matchLevel)
                );
            } //end if


            // Return the list.
            return retList;
        } //end ApplyFormularyFilterToList

        private class MedicationLookup
        {
            public int MedicationId { get; set; }

            public string BrandName { get; set; }

            public string DrugId { get; set; }

            public byte? InpatientMatch { get; set; }

            public byte? OutpatientMatch { get; set; }

            public byte? PyxisMatch { get; set; }

            //We need to sort the return list by match level and then brand name.
            //Thusly, I need to calculate the maximum between inpat, outpat, and pyxis.
            //Winston Murdock, 01/22/2021.EMAR-586.
            public byte? MatchLevel { get; set; }

            //Whether this medication was found via matching on the brand name
            //or it was found in the ingredient list.
            //If true, then brand name search.
            //If false, then ingredients list.
            //Winston Murdock, 02/01/2021.
            public bool IsBrandNameMatch { get; set; }

            //The position within the brand name (or ingredient list) that the match starts at.
            //If we searched for "Tylenol", then "Tylenol" would have a value of 1 for this.
            //And "Children's Tylenol" would have a value of 12.
            //We need to sort the return by this field.
            //Winston Murdock, 02/01/2021.
            public int SearchPos { get; set; }

            //Adding the fields for group items.
            //The group list item's id will be stored in the MedicationId field.
            //The Dto will save it in a GroupListItemId field.
            //Winston Murdock, 09/25/2022.  PC-27536
            public bool? IsGroupItem { get; set; }
        } //end class MedicationLookup

        private static byte CalcMaxMatch(byte? inpat, byte? outpat, byte? pyxis)
        {
            //Calculate the highest value between inpat match, outpat match, and pyxis.
            //The UI wants us to sort by this.
            //Since all three of these are nullable bytes in the DTO,
            //copy them into a non-nullable byte and use 0 if they are null.
            //Math.Max needs me to pass in a non-nullable byte.
            //Winston Murdock, 01/22/2021.  EMAR-586
            byte tempInpat = inpat ?? 0;
            byte tempOutpat = outpat ?? 0;
            byte tempPyxis = pyxis ?? 0;

            //Return the highest value between inpat, outpat, and pyxis.
            return Math.Max(tempInpat, Math.Max(tempOutpat, tempPyxis));
        } //end CalcMaxMatch

        private static int CalcSearchPos(string brandName, string activeList, string searchString)
        {
            //Return the position of search string in either the brand name or active list.
            //Convert all values to Lower case so that this is case insensitive.

            //Return variable
            int ret;

            //The position of the search string in the brand name.
            int brandNamePos = brandName.ToLower().IndexOf(searchString.ToLower());
            
            //If it was found in brand name, then use that.
            //Else, use its position in the ingredient list.
            if (brandNamePos != -1)
            {
                //We found it in brand name.
                //Use the position in the brand name,
                ret = brandNamePos;
            }
            else
            {
                //Not found in the brand name.
                //Use the position in the ingredient list.
                ret = activeList.ToLower().IndexOf(searchString.ToLower());
            } //end if

            //Return.
            return ret;
        } //end function CalcSearchPos

        private static int? ShouldWeReturnTheMedicationId(bool? isGroupItem, int? medicationId)
        {
            //If isGroupitem is null, then we return null instead of the medicationId.
            //I couldn't get this working with a ternary expression in the mapping up above
            //so I decided to write a small function to do it.
            //Winston Murdock, 09/26/2022.  PC-27536
            int? ret;

            if (!isGroupItem.HasValue)
            {
                ret = null;
            }
            else
            {
                if ((bool)isGroupItem)
                {
                    ret = medicationId;
                }
                else
                {
                    ret = null;
                } //end if
            } //end if

            return ret;
        } //end ShouldWeReturnTheMedicationId

    }
}