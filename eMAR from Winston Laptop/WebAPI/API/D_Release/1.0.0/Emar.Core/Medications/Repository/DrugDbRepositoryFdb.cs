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
            if ((searchType == EmarOrderType.All) || (!_inpat && !_outpat && !_pyxis))
            {
                //This is being changed from a string to a MedicationLookup
                //We have to do this to be able to have/access the IsBrandNameMatch and SearchPos fields.
                //Winston Murdock, 02/03/2021.
                var nonFormSearchResults = NonFormularySearchResults(searchType, search, userId, deptCode);
                var ret = nonFormSearchResults.Select(i => new BrandNameReturnDto
                {
                    BrandName = i.BrandName,
                    InpatientMatch = 0,
                    OutpatientMatch = 0,
                    PyxisMatch = 0,
                    MatchLevel = 0,
                    IsBrandNameMatch = i.IsBrandNameMatch,
                    SearchPos = i.SearchPos
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
                MatchLevel = CalcMaxMatch(i.InpatientMatch, i.OutpatientMatch, i.PyxisMatch)

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
                    //Also join to fdb_ndc_info to get the ids from it.
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
                             join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
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
                                 Medid = fni.Medid,
                                 GcnSeqNo = fni.GcnSeqno ?? -1,
                                 HiclSeqNo = fni.HiclSeqno ?? -1,
                                 //These two lines let me do one DB hit and still set these values appropriately.
                                 //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                 //Else, set IsBrandNameMatch to false.
                                 //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                 //Winston Murdock, 02/03/2021.
                                 IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                                 SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                             }).Distinct().ToList();

                        //Also perform the search to get all medications that match the search criteria
                        //and that are not in site_formulary_match.
                        //Winston Murdock, 11/04/2020

                        //ret.AddRange(
                        //    (from m in _context.Medications
                        //     join md in _context.MedicationDetails on m.Id equals md.MedicationId
                        //     join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                        //     join dpl in _context.DepartmentPreferredListItems
                        //         on new { MedicationId = m.Id, SiteId = _siteId }
                        //         equals new { dpl.MedicationId, dpl.SiteId }
                        //     where !string.IsNullOrWhiteSpace(md.BrandName)
                        //           && md.IsActive
                        //           && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
                        //           (
                        //               EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                        //               EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                        //               EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                        //           )
                        //     select new MedicationLookup
                        //     {
                        //         BrandName = md.BrandName,
                        //         DrugId = md.DrugId,
                        //         MedicationId = md.MedicationId,

                        //         //Don't set these to 0.
                        //         //We need them to be null, so that we know that we need to calculate the match value
                        //         //and insert them into the site_formulary_match table.
                        //         //Winston Murdock, 03/08/2021.  EMAR-
                        //         //InpatientMatch = 0,
                        //         //OutpatientMatch = 0,
                        //         //PyxisMatch = 0,

                        //         Medid = fni.Medid,
                        //         GcnSeqNo = fni.GcnSeqno ?? -1,
                        //         HiclSeqNo = fni.HiclSeqno ?? -1,
                        //         //These two lines let me do one DB hit and still set these values appropriately.
                        //         //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                        //         //Else, set IsBrandNameMatch to false.
                        //         //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                        //         //Winston Murdock, 02/03/2021.
                        //         IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                        //         SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                        //     }).Distinct().ToList()
                        //);
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
                             join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
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
                                 Medid = fni.Medid,
                                 GcnSeqNo = fni.GcnSeqno ?? -1,
                                 HiclSeqNo = fni.HiclSeqno ?? -1,
                                 //These two lines let me do one DB hit and still set these values appropriately.
                                 //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                 //Else, set IsBrandNameMatch to false.
                                 //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                 //Winston Murdock, 02/03/2021.
                                 IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                                 SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                             }).Distinct().ToList();

                        //Also perform the search to get all medications that match the search criteria
                        //and that are not in site_formulary_match.
                        //Winston Murdock, 11/04/2020

                        //ret.AddRange(
                        //    (from m in _context.Medications
                        //        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                        //        join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                        //        join dpl in _context.DepartmentPreferredListItems
                        //            on new {MedicationId = m.Id, SiteId = _siteId, DeptCode = deptCode}
                        //            equals new {dpl.MedicationId, dpl.SiteId, DeptCode = dpl.DepartmentCode}
                        //        where !string.IsNullOrWhiteSpace(md.BrandName)
                        //              && md.IsActive
                        //              && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
                        //              (
                        //                  EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                        //                  EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                        //                  EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                        //              )
                        //        select new MedicationLookup
                        //        {
                        //            BrandName = md.BrandName,
                        //            DrugId = md.DrugId,
                        //            MedicationId = md.MedicationId,

                        //            //Don't set these to 0.
                        //            //We need them to be null, so that we know that we need to calculate the match value
                        //            //and insert them into the site_formulary_match table.
                        //            //Winston Murdock, 03/08/2021.  EMAR-
                        //            //InpatientMatch = 0,
                        //            //OutpatientMatch = 0,
                        //            //PyxisMatch = 0,

                        //            Medid = fni.Medid,
                        //            GcnSeqNo = fni.GcnSeqno ?? -1,
                        //            HiclSeqNo = fni.HiclSeqno ?? -1,
                        //            //These two lines let me do one DB hit and still set these values appropriately.
                        //            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                        //            //Else, set IsBrandNameMatch to false.
                        //            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                        //            //Winston Murdock, 02/03/2021.
                        //            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                        //            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                        //        }).Distinct().ToList()
                        //);
                    } //end if (do we have a department code?)
                    break;
                case EmarOrderType.GroupRememberedOrder:
                    //Perform the "groups" search.
                    //join to site_formulary_match (to only include medications that
                    //are already in the match table for this site) and to get the "match" values from it.
                    //Also join to fdb_ndc_info to get the ids from it.
                    //EMAR-321.  Winston Murdock, 11/04/2020

                    ret =
                    (from m in _context.Medications
                        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                        join sfm in _context.SiteFormularyMatch
                            on new { MedicationId = m.Id, SiteId = _siteId }
                            equals new { sfm.MedicationId, sfm.SiteId }
                        join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                        join gli in _context.GroupListItems
                            on new { MedicationId = m.Id, SiteId = _siteId }
                            equals new { gli.MedicationId, gli.SiteId }
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
                            Medid = fni.Medid,
                            GcnSeqNo = fni.GcnSeqno ?? -1,
                            HiclSeqNo = fni.HiclSeqno ?? -1,
                            //These two lines let me do one DB hit and still set these values appropriately.
                            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                            //Else, set IsBrandNameMatch to false.
                            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                            //Winston Murdock, 02/03/2021.
                            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                        }).Distinct().ToList();

                    //Also perform the search to get all medications that match the search criteria
                    //and that are not in site_formulary_match.
                    //Winston Murdock, 11/04/2020

                    //ret.AddRange
                    //(
                    //    (from m in _context.Medications
                    //        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                    //        join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                    //        join gli in _context.GroupListItems
                    //            on new { MedicationId = m.Id, SiteId = _siteId }
                    //            equals new { gli.MedicationId, gli.SiteId }
                    //        where !string.IsNullOrWhiteSpace(md.BrandName)
                    //            && md.IsActive
                    //            && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
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

                    //            //Don't set these to 0.
                    //            //We need them to be null, so that we know that we need to calculate the match value
                    //            //and insert them into the site_formulary_match table.
                    //            //Winston Murdock, 03/08/2021.  EMAR-
                    //            //InpatientMatch = 0,
                    //            //OutpatientMatch = 0,
                    //            //PyxisMatch = 0,

                    //            Medid = fni.Medid,
                    //            GcnSeqNo = fni.GcnSeqno ?? -1,
                    //            HiclSeqNo = fni.HiclSeqno ?? -1,
                    //            //These two lines let me do one DB hit and still set these values appropriately.
                    //            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                    //            //Else, set IsBrandNameMatch to false.
                    //            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                    //            //Winston Murdock, 02/03/2021.
                    //            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                    //            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                    //        }).Distinct().ToList()
                    //);

                    break;
                case EmarOrderType.UserQuickListItem:
                    //Perform the "user quicklist" search.
                    //join to site_formulary_match (to only include medications that
                    //are already in the match table for this site) and to get the "match" values from it.
                    //Also join to fdb_ndc_info to get the ids from it.
                    //EMAR-319.  Winston Murdock, 11/04/2020

                    //Grab all items that match on the brand name.
                    ret =
                    (from m in _context.Medications
                        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                        join sfm in _context.SiteFormularyMatch
                            on new { MedicationId = m.Id, SiteId = _siteId }
                            equals new { sfm.MedicationId, sfm.SiteId }
                        join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
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
                            Medid = fni.Medid,
                            GcnSeqNo = fni.GcnSeqno ?? -1,
                            HiclSeqNo = fni.HiclSeqno ?? -1,
                            //These two lines let me do one DB hit and still set these values appropriately.
                            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                            //Else, set IsBrandNameMatch to false.
                            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                            //Winston Murdock, 02/03/2021.
                            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                        }).Distinct().ToList();

                    //Also perform the search to get all medications that match the search criteria
                    //and that are not in site_formulary_match.
                    //Winston Murdock, 11/04/2020

                    //Grab all items that match on the brand name.
                    //ret.AddRange
                    //(
                    //    (from m in _context.Medications
                    //        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                    //        join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                    //        join uqli in _context.UserQuickListItems
                    //            on new { MedicationId = m.Id, SiteId = _siteId }
                    //            equals new { uqli.MedicationId, uqli.SiteId }
                    //        where !string.IsNullOrWhiteSpace(md.BrandName)
                    //            && md.IsActive
                    //            && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
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

                    //            //Don't set these to 0.
                    //            //We need them to be null, so that we know that we need to calculate the match value
                    //            //and insert them into the site_formulary_match table.
                    //            //Winston Murdock, 03/08/2021.  EMAR-
                    //            //InpatientMatch = 0,
                    //            //OutpatientMatch = 0,
                    //            //PyxisMatch = 0,

                    //            Medid = fni.Medid,
                    //            GcnSeqNo = fni.GcnSeqno ?? -1,
                    //            HiclSeqNo = fni.HiclSeqno ?? -1,
                    //            //These two lines let me do one DB hit and still set these values appropriately.
                    //            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                    //            //Else, set IsBrandNameMatch to false.
                    //            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                    //            //Winston Murdock, 02/03/2021.
                    //            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                    //            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                    //        }).Distinct().ToList()
                    //);

                    break;
                default:
                    //Perform the "all" search.
                    //join to site_formulary_match (to only include medications that
                    //are already in the match table for this site) and to get the "match" values from it.
                    //Also join to fdb_ndc_info to get the ids from it.
                    //Winston Murdock, 11/04/2020

                    ret =
                    (from m in _context.Medications
                        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                        join sfm in _context.SiteFormularyMatch
                            on new { MedicationId = m.Id, SiteId = _siteId }
                            equals new { sfm.MedicationId, sfm.SiteId }
                        join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
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
                            Medid = fni.Medid,
                            GcnSeqNo = fni.GcnSeqno ?? -1,
                            HiclSeqNo = fni.HiclSeqno ?? -1,
                            //These two lines let me do one DB hit and still set these values appropriately.
                            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                            //Else, set IsBrandNameMatch to false.
                            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                            //Winston Murdock, 02/03/2021.
                            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                        }).Distinct().ToList();

                    //Also perform the search to get all medications that match the search criteria
                    //and that are not in site_formulary_match.
                    //Winston Murdock, 11/04/2020

                    //ret.AddRange
                    //(
                    //    (from m in _context.Medications
                    //        join md in _context.MedicationDetails on m.Id equals md.MedicationId
                    //        join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                    //        where !string.IsNullOrWhiteSpace(md.BrandName)
                    //            && md.IsActive
                    //            && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
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
                                
                    //            //Don't set these to 0.
                    //            //We need them to be null, so that we know that we need to calculate the match value
                    //            //and insert them into the site_formulary_match table.
                    //            //Winston Murdock, 03/08/2021.  EMAR-
                    //            //InpatientMatch = 0,
                    //            //OutpatientMatch = 0,
                    //            //PyxisMatch = 0,
                                
                    //            Medid = fni.Medid,
                    //            GcnSeqNo = fni.GcnSeqno ?? -1,
                    //            HiclSeqNo = fni.HiclSeqno ?? -1,
                    //            //These two lines let me do one DB hit and still set these values appropriately.
                    //            //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                    //            //Else, set IsBrandNameMatch to false.
                    //            //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                    //            //Winston Murdock, 02/03/2021.
                    //            IsBrandNameMatch = md.BrandName.ToLower().IndexOf(search.ToLower()) != -1,
                    //            SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, search)
                    //        }).Distinct().ToList()
                    //);

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
                                   Medid = -1,
                                   GcnSeqNo = -1,
                                   HiclSeqNo = -1,
                                   //These two lines let me do one DB hit and still set these values appropriately.
                                   //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                   //Else, set IsBrandNameMatch to false.
                                   //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                   //Winston Murdock, 02/03/2021.
                                   IsBrandNameMatch = md.BrandName.ToLower().IndexOf(searchString.ToLower()) != -1,
                                   SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, searchString)

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
                                   Medid = -1,
                                   GcnSeqNo = -1,
                                   HiclSeqNo = -1,
                                   //These two lines let me do one DB hit and still set these values appropriately.
                                   //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                   //Else, set IsBrandNameMatch to false.
                                   //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                   //Winston Murdock, 02/03/2021.
                                   IsBrandNameMatch = md.BrandName.ToLower().IndexOf(searchString.ToLower()) != -1,
                                   SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, searchString)
                               }).ToList();
                    } //end if (deptCode == null)

                    break;
                case EmarOrderType.GroupRememberedOrder:
                    //Perform the "groups" search.
                    //EMAR-321.  Winston Murdock, 09/24/2020
                    ret = (from m in _context.Medications
                                //Join to the Medication Details table
                            join md in _context.MedicationDetails on m.Id equals md.MedicationId
                            //Join to the group_list_items table.
                            join gli in _context.GroupListItems
                               on new { MedicationId = m.Id, SiteId = _siteId }
                               equals new { gli.MedicationId, gli.SiteId }
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
                               Medid = -1,
                               GcnSeqNo = -1,
                               HiclSeqNo = -1,
                               //These two lines let me do one DB hit and still set these values appropriately.
                               //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                               //Else, set IsBrandNameMatch to false.
                               //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                               //Winston Murdock, 02/03/2021.
                               IsBrandNameMatch = md.BrandName.ToLower().IndexOf(searchString.ToLower()) != -1,
                               SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, searchString)
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
                                Medid = -1,
                                GcnSeqNo = -1,
                                HiclSeqNo = -1,
                                //These two lines let me do one DB hit and still set these values appropriately.
                                //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                //Else, set IsBrandNameMatch to false.
                                //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                //Winston Murdock, 02/03/2021.
                                IsBrandNameMatch = md.BrandName.ToLower().IndexOf(searchString.ToLower()) != -1,
                                SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, searchString)

                            }).ToList();
                    break;
                default:
                    //Perform the "all" search.
                    //Since we aren't doing formulary filtering, we don't need
                    //anything from the site_formulary_match or fdb_ndc_info tables.
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
                                Medid = -1,
                                GcnSeqNo = -1,
                                HiclSeqNo = -1,
                                //These two lines let me do one DB hit and still set these values appropriately.
                                //If the search string is found in the brand name, then set IsBrandNameMatch to true.
                                //Else, set IsBrandNameMatch to false.
                                //Then set SearchPos based on whether the seach string is in the brand name or ingredient list.
                                //Winston Murdock, 02/03/2021.
                                IsBrandNameMatch = md.BrandName.ToLower().IndexOf(searchString.ToLower()) != -1,
                                SearchPos = CalcSearchPos(md.BrandName, md.ActiveList, searchString)
                            }).ToList();
                        
                    break;
            } //end switch (search type)

            //Retuen the list of MedicationLookup objects.
            return ret;
        } //end NonFormularySearchResults

        private List<MedicationLookup> ApplyFormularyFilterToList(List<MedicationLookup> meds)
        {
            //We don't need to attempt to do the formulary match calculation here.
            //The DB has already calculated the match level.
            //Everything here will already have 0, 1, 2, 3, or 4 .
            //Winston Murdock, 03/21/2021. EMAR-837
            //// If any of the medIds returned don't have a site_formulary_match record, then fill them out now
            //if (meds.Any(m => !m.OutpatientMatch.HasValue))
            //{
            //    //Grab the info from the fdb_ndc_info table for all medications in the site_formulary table for this site.
            //    //We'll also grab the I/O/P value for each drug from site_formulary.
            //    var fdbInfoForFormularyMeds =
            //    (
            //        from fni in _context.FdbNdcInfo
            //        join md in _context.MedicationDetails on fni.Medid.ToString() equals md.DrugId
            //        join sf in _context.SiteFormulary on new { md.MedicationId, SiteId = _siteId } equals new
            //        { sf.MedicationId, sf.SiteId }
            //        select (new { fni, sf.IsInpatient, sf.IsOutpatient, sf.IsPyxis })
            //    ).ToList();

            //    //Loop through all the medications that are not in the match table.
            //    //Compare each of them with all of the fdb IDs in fdbInfoForFormularyMeds.
            //    //Use those to determine what match value to calculate for this medication.
            //    foreach (var med in meds.Where(m => !m.OutpatientMatch.HasValue))
            //    {
            //        // Get the match level for the inpatient formulary
            //        byte matchInpt = 0;
            //        if (fdbInfoForFormularyMeds.Any(m => m.IsInpatient && m.fni.Medid == med.Medid))
            //            matchInpt = 3;
            //        else if (fdbInfoForFormularyMeds.Any(m => m.IsInpatient && m.fni.GcnSeqno == med.GcnSeqNo))
            //            matchInpt = 2;
            //        else if (fdbInfoForFormularyMeds.Any(m => m.IsInpatient && m.fni.HiclSeqno == med.HiclSeqNo))
            //            matchInpt = 1;

            //        // Get the match level for the outpatient formulary
            //        byte matchOutpt = 0;
            //        if (fdbInfoForFormularyMeds.Any(m => m.IsOutpatient && m.fni.Medid == med.Medid))
            //            matchOutpt = 3;
            //        else if (fdbInfoForFormularyMeds.Any(m => m.IsOutpatient && m.fni.GcnSeqno == med.GcnSeqNo))
            //            matchOutpt = 2;
            //        else if (fdbInfoForFormularyMeds.Any(m => m.IsOutpatient && m.fni.HiclSeqno == med.HiclSeqNo))
            //            matchOutpt = 1;

            //        // Get the match level for the Pyxis formulary
            //        byte matchPyxis = 0;
            //        if (fdbInfoForFormularyMeds.Any(m => m.IsPyxis && m.fni.Medid == med.Medid))
            //            matchPyxis = 3;
            //        else if (fdbInfoForFormularyMeds.Any(m => m.IsPyxis && m.fni.GcnSeqno == med.GcnSeqNo))
            //            matchPyxis = 2;
            //        else if (fdbInfoForFormularyMeds.Any(m => m.IsPyxis && m.fni.HiclSeqno == med.HiclSeqNo))
            //            matchPyxis = 1;

            //        //Now that we've set the inpatient, outpatient, and pyxis variables, add a new site formulary match
            //        //object to the DB context (we'll save the changes after the loop).
            //        _context.SiteFormularyMatch.Add(new SiteFormularyMatch
            //        {
            //            SiteId = _siteId,
            //            InpatientMatch = matchInpt,
            //            OutpatientMatch = matchOutpt,
            //            PyxisMatch = matchPyxis,
            //            MedicationId = med.MedicationId
            //        });

            //        med.InpatientMatch = matchInpt;
            //        med.OutpatientMatch = matchOutpt;
            //        med.PyxisMatch = matchPyxis;

            //        //Calculate the maximum match value.
            //        //Winston Murdock, 01/22/2021.  EMAR-586
            //        med.MatchLevel = Math.Max(matchInpt, Math.Max(matchOutpt, matchPyxis));
            //    } //end foreach loop

            //    //Save the changes to the database (i.e. perform all the inserts).
            //    //Doing it this way lets us only do one DB operation for all the inserts
            //    //rather than doing one operation for each time through the loop above.
            //    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            //    {
            //        try
            //        {
            //            _context.SaveChanges();
            //            transaction.Commit();
            //        }
            //        catch (Exception ex)
            //        {
            //            transaction.Rollback();
            //        } //end try/catch
            //    } //end using
            //} //end if (are there any meds who are not in the site_formulary_match table?)

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

        //TODO: Need three methods here (department preferred list, group, and user quick list).
        //1) Accept a list of those entities.
        //2) Join to site formulary match table (so two selects as aboe in FormularySearchCandidates
        //3) Pass the medicationlookup object into ApplyFormularyFilterToList
        //4) Return the filtered list to whoever called this method.

        //Parameter/return data types.
        //IEnumerable<DepartmentPreferredListItem>
        //IEnumerable<GroupListItem>
        //IEnumerable<UserQuickListItem>

        public List<UserQuickListItem> ApplyFormularyFilterToQuickList(Expression<Func<UserQuickListItem, bool>> whereExpression, Expression<Func<UserQuickListItem, bool>> whereExpressionNoMatchRow, int siteId)
        {
            //TODO: Come back and finish this.
            //For user quick list starting with "C", we were getting 1,146 medications.
            //Per SSMS, of those, five had a 1 in the "match" columns.
            //But this filtering was returning zero meds.
            //Need to figure out what's going on.

            //Per Debi, I'm holding off on this.
            //The UI isn't waiting on this, so she asked me to hold off on this for now.
            //I'll come back to it later.
            //Winston Murdock, 11/11/2020.
            //throw new NotImplementedException();

            IEnumerable<UserQuickListItem> retList = new List<UserQuickListItem>();

            List<MedicationLookup> meds = new List<MedicationLookup>();

            //Somehow get the values for fdb_ndb_info and site_formulary_match for each medication in the parameter list.
            //As with FormularySearchCandidates, this will have two queries: one for the medications that do
            //have a row in the match table and one for those that do not.
            //This is the original query from OrderRepository.GetUserQuickListTabItems()
            //The only change is that we've added in the join to fni and sfm.
            //This guy should get me all medications that are already in the site_formulary_match table.
            //Yes, we'll have a cartesian product based since one medication can be in fdb_ndc_info
            //multiple times.  But we'll handle that.
            var uqli =
                (
                    from s in
                    (
                    _context.UserQuickListItems
                    .Where(whereExpression)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(d => d.FdbBrandName)
                    .Include(i => i.Medication)
                        .ThenInclude(m => m.MedicationDetails)
                            .ThenInclude(md => md.MedicationUnit)
                    .Include(i => i.MedicationRoute)
                    .Include(i => i.MedicationUnit)
                    .Include(i => i.FrequencySchedule)
                    )
                    //These lines were added to the original query.
                    //We need the "id" fields from fdb_ndc_info.
                    //And we need the "match" fields from site_formulary_match.
                    join fni in _context.FdbNdcInfo on s.Medication.DrugId equals fni.Medid.ToString()
                    join sfm in _context.SiteFormularyMatch on new { s.MedicationId, SiteId = siteId } equals new
                    { sfm.MedicationId, sfm.SiteId }
                    //End added lines

                    //Get all of it as a MedicationLookup.
                    select new MedicationLookup
                    {
                        BrandName = "",
                        DrugId = s.Medication.DrugId,
                        MedicationId = s.MedicationId,
                        InpatientMatch = sfm.InpatientMatch,
                        OutpatientMatch = sfm.OutpatientMatch,
                        PyxisMatch = sfm.PyxisMatch,
                        Medid = fni.Medid,
                        GcnSeqNo = fni.GcnSeqno ?? -1,
                        HiclSeqNo = fni.HiclSeqno ?? -1,
                        uqliItem = s
                    }
                ).ToList();

            //Also need to write the linq to pull the ones from the list that are NOT in
            //the site_formulary_match table and call .AddRange.
            uqli.AddRange(
                            (
                                from s in
                                (
                                _context.UserQuickListItems
                                .Where(whereExpressionNoMatchRow)
                                .Include(i => i.Medication)
                                    .ThenInclude(m => m.MedicationDetails)
                                        .ThenInclude(d => d.FdbBrandName)
                                .Include(i => i.Medication)
                                    .ThenInclude(m => m.MedicationDetails)
                                        .ThenInclude(md => md.MedicationUnit)
                                .Include(i => i.MedicationRoute)
                                .Include(i => i.MedicationUnit)
                                .Include(i => i.FrequencySchedule)
                                )
                                join fni in _context.FdbNdcInfo on s.Medication.DrugId equals fni.Medid.ToString()
                                
                                select new MedicationLookup
                                {
                                    BrandName = "",
                                    DrugId = s.Medication.DrugId,
                                    MedicationId = s.Medication.Id,
                                    InpatientMatch = 0,
                                    OutpatientMatch = 0,
                                    PyxisMatch = 0,
                                    Medid = fni.Medid,
                                    GcnSeqNo = fni.GcnSeqno ?? -1,
                                    HiclSeqNo = fni.HiclSeqno ?? -1,
                                    uqliItem = s
                                }
                        ).Distinct().ToList()
                    );

            //Call the filter method.
            //TODO: Need to figure out what's going on with the filter method and this list.
            //It's filtering out every row from uqli, not sure why.
            //Need to debug that on Monday.
            var filteredMeds = ApplyFormularyFilterToList(uqli);
            return filteredMeds.Select(z => z.uqliItem).Distinct().ToList();

            //throw new NotImplementedException();
        } //end ApplyFormularyFilterToQuickList

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

            public decimal Medid { get; set; }

            public decimal GcnSeqNo { get; set; }
            
            public decimal HiclSeqNo { get; set; }

            //The objects for the "remembered" lists are below.
            //For brand name search, these weill be empty.
            //For the "remembered" lists (where we pass in ther where
            //clause, run the DB query in here, and then filter that
            //list based on the formulary criteria, one of these will
            //have data while the other two are empty.
            public UserQuickListItem? uqliItem { get; set; }

            public GroupListItem? gliItem { get; set; }

            public DepartmentPreferredListItem? dpliItem { get; set; }

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

    }
}