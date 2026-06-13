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
                var nonFormSearchResults = NonFormularySearchResults(searchType, search, userId, deptCode);
                return nonFormSearchResults.Select(i => new BrandNameReturnDto
                {
                    BrandName = i,
                    InpatientMatch = 0,
                    OutpatientMatch = 0,
                    PyxisMatch = 0
                }).Distinct().OrderBy(i => i.BrandName);

            } //end if

            //Do the specified search and store the results into a local variable.
            List<MedicationLookup> meds = FormularySearchCandidates(searchType, search, deptCode);

            //Apply the formulary filtering to the list.
            //Also calculcate the match level for any medications
            //in the list that we haven't calculated it for yet.
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
                PyxisMatch = i.PyxisMatch
            }).Distinct().OrderBy(i => i.BrandName);
        } //end function GetMedsByBrandName

        private List<MedicationLookup> FormularySearchCandidates(EmarOrderType searchType, string search, string deptCode)
        {
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
                             select (new MedicationLookup
                             {
                                 BrandName = md.BrandName,
                                 DrugId = md.DrugId,
                                 MedicationId = md.MedicationId,
                                 InpatientMatch = sfm.InpatientMatch,
                                 OutpatientMatch = sfm.OutpatientMatch,
                                 PyxisMatch = sfm.PyxisMatch,
                                 Medid = fni.Medid,
                                 GcnSeqNo = fni.GcnSeqno ?? -1,
                                 HiclSeqNo = fni.HiclSeqno ?? -1
                             })).Distinct().ToList();

                        //Also perform the search to get all medications that match the search criteria
                        //and that are not in site_formulary_match.
                        //Winston Murdock, 11/04/2020
                        ret.AddRange(
                            (from m in _context.Medications
                             join md in _context.MedicationDetails on m.Id equals md.MedicationId
                             join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                             join dpl in _context.DepartmentPreferredListItems
                                 on new { MedicationId = m.Id, SiteId = _siteId }
                                 equals new { dpl.MedicationId, dpl.SiteId }
                             where !string.IsNullOrWhiteSpace(md.BrandName)
                                   && md.IsActive
                                   && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
                                   (
                                       EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                       EF.Functions.Like(md.ActiveList,
                                           $"{search}%") || //at the start of active list
                                       EF.Functions.Like(md.ActiveList,
                                           $"%/ {search}%") //after a / and space in active list
                                   )
                             select new MedicationLookup
                             {
                                 BrandName = md.BrandName,
                                 DrugId = md.DrugId,
                                 MedicationId = md.MedicationId,
                                 InpatientMatch = 0,
                                 OutpatientMatch = 0,
                                 PyxisMatch = 0,
                                 Medid = fni.Medid,
                                 GcnSeqNo = fni.GcnSeqno ?? -1,
                                 HiclSeqNo = fni.HiclSeqno ?? -1
                             }).Distinct().ToList()
                        );
                    }
                    else // you do have a department code
                    {
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
                             select (new MedicationLookup
                             {
                                 BrandName = md.BrandName,
                                 DrugId = md.DrugId,
                                 MedicationId = md.MedicationId,
                                 InpatientMatch = sfm.InpatientMatch,
                                 OutpatientMatch = sfm.OutpatientMatch,
                                 PyxisMatch = sfm.PyxisMatch,
                                 Medid = fni.Medid,
                                 GcnSeqNo = fni.GcnSeqno ?? -1,
                                 HiclSeqNo = fni.HiclSeqno ?? -1
                             })).Distinct().ToList();

                        //Also perform the search to get all medications that match the search criteria
                        //and that are not in site_formulary_match.
                        //Winston Murdock, 11/04/2020
                        ret.AddRange(
                            (from m in _context.Medications
                                join md in _context.MedicationDetails on m.Id equals md.MedicationId
                                join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                                join dpl in _context.DepartmentPreferredListItems
                                    on new {MedicationId = m.Id, SiteId = _siteId, DeptCode = deptCode}
                                    equals new {dpl.MedicationId, dpl.SiteId, DeptCode = dpl.DepartmentCode}
                                where !string.IsNullOrWhiteSpace(md.BrandName)
                                      && md.IsActive
                                      && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
                                      (
                                          EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                          EF.Functions.Like(md.ActiveList,
                                              $"{search}%") || //at the start of active list
                                          EF.Functions.Like(md.ActiveList,
                                              $"%/ {search}%") //after a / and space in active list
                                      )
                                select new MedicationLookup
                                {
                                    BrandName = md.BrandName,
                                    DrugId = md.DrugId,
                                    MedicationId = md.MedicationId,
                                    InpatientMatch = 0,
                                    OutpatientMatch = 0,
                                    PyxisMatch = 0,
                                    Medid = fni.Medid,
                                    GcnSeqNo = fni.GcnSeqno ?? -1,
                                    HiclSeqNo = fni.HiclSeqno ?? -1
                                }).Distinct().ToList()
                        );
                    }
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
                        select (new MedicationLookup
                        {
                            BrandName = md.BrandName,
                            DrugId = md.DrugId,
                            MedicationId = md.MedicationId,
                            InpatientMatch = sfm.InpatientMatch,
                            OutpatientMatch = sfm.OutpatientMatch,
                            PyxisMatch = sfm.PyxisMatch,
                            Medid = fni.Medid,
                            GcnSeqNo = fni.GcnSeqno ?? -1,
                            HiclSeqNo = fni.HiclSeqno ?? -1
                        })).Distinct().ToList();

                    //Also perform the search to get all medications that match the search criteria
                    //and that are not in site_formulary_match.
                    //Winston Murdock, 11/04/2020
                    ret.AddRange
                    (
                        (from m in _context.Medications
                            join md in _context.MedicationDetails on m.Id equals md.MedicationId
                            join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                            join gli in _context.GroupListItems
                                on new { MedicationId = m.Id, SiteId = _siteId }
                                equals new { gli.MedicationId, gli.SiteId }
                            where !string.IsNullOrWhiteSpace(md.BrandName)
                                && md.IsActive
                                && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
                                (
                                    EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                    EF.Functions.Like(md.ActiveList,
                                        $"{search}%") || //at the start of active list
                                    EF.Functions.Like(md.ActiveList,
                                        $"%/ {search}%") //after a / and space in active list
                                )
                            select new MedicationLookup
                            {
                                BrandName = md.BrandName,
                                DrugId = md.DrugId,
                                MedicationId = md.MedicationId,
                                InpatientMatch = 0,
                                OutpatientMatch = 0,
                                PyxisMatch = 0,
                                Medid = fni.Medid,
                                GcnSeqNo = fni.GcnSeqno ?? -1,
                                HiclSeqNo = fni.HiclSeqno ?? -1
                            }).Distinct().ToList()
                    );

                    break;
                case EmarOrderType.UserQuickListItem:
                    //Perform the "user quicklist" search.
                    //join to site_formulary_match (to only include medications that
                    //are already in the match table for this site) and to get the "match" values from it.
                    //Also join to fdb_ndc_info to get the ids from it.
                    //EMAR-319.  Winston Murdock, 11/04/2020
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
                        select (new MedicationLookup
                        {
                            BrandName = md.BrandName,
                            DrugId = md.DrugId,
                            MedicationId = md.MedicationId,
                            InpatientMatch = sfm.InpatientMatch,
                            OutpatientMatch = sfm.OutpatientMatch,
                            PyxisMatch = sfm.PyxisMatch,
                            Medid = fni.Medid,
                            GcnSeqNo = fni.GcnSeqno ?? -1,
                            HiclSeqNo = fni.HiclSeqno ?? -1
                        })).Distinct().ToList();

                    //Also perform the search to get all medications that match the search criteria
                    //and that are not in site_formulary_match.
                    //Winston Murdock, 11/04/2020
                    ret.AddRange
                    (
                        (from m in _context.Medications
                            join md in _context.MedicationDetails on m.Id equals md.MedicationId
                            join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                            join uqli in _context.UserQuickListItems
                                on new { MedicationId = m.Id, SiteId = _siteId }
                                equals new { uqli.MedicationId, uqli.SiteId }
                            where !string.IsNullOrWhiteSpace(md.BrandName)
                                && md.IsActive
                                && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
                                (
                                    EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                    EF.Functions.Like(md.ActiveList,
                                        $"{search}%") || //at the start of active list
                                    EF.Functions.Like(md.ActiveList,
                                        $"%/ {search}%") //after a / and space in active list
                                )
                            select new MedicationLookup
                            {
                                BrandName = md.BrandName,
                                DrugId = md.DrugId,
                                MedicationId = md.MedicationId,
                                InpatientMatch = 0,
                                OutpatientMatch = 0,
                                PyxisMatch = 0,
                                Medid = fni.Medid,
                                GcnSeqNo = fni.GcnSeqno ?? -1,
                                HiclSeqNo = fni.HiclSeqno ?? -1
                            }).Distinct().ToList()
                    );

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
                        select (new MedicationLookup
                        {
                            BrandName = md.BrandName,
                            DrugId = md.DrugId,
                            MedicationId = md.MedicationId,
                            InpatientMatch = sfm.InpatientMatch,
                            OutpatientMatch = sfm.OutpatientMatch,
                            PyxisMatch = sfm.PyxisMatch,
                            Medid = fni.Medid,
                            GcnSeqNo = fni.GcnSeqno ?? -1,
                            HiclSeqNo = fni.HiclSeqno ?? -1
                        })).Distinct().ToList();

                    //Also perform the search to get all medications that match the search criteria
                    //and that are not in site_formulary_match.
                    //Winston Murdock, 11/04/2020
                    ret.AddRange
                    (
                        (from m in _context.Medications
                            join md in _context.MedicationDetails on m.Id equals md.MedicationId
                            join fni in _context.FdbNdcInfo on m.DrugId equals fni.Medid.ToString()
                            where !string.IsNullOrWhiteSpace(md.BrandName)
                                && md.IsActive
                                && m.SiteFormularyMatchs.All(a => a.SiteId != _siteId) &&
                                (
                                    EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                    EF.Functions.Like(md.ActiveList,
                                        $"{search}%") || //at the start of active list
                                    EF.Functions.Like(md.ActiveList,
                                        $"%/ {search}%") //after a / and space in active list
                                )
                            select new MedicationLookup
                            {
                                BrandName = md.BrandName,
                                DrugId = md.DrugId,
                                MedicationId = md.MedicationId,
                                InpatientMatch = 0,
                                OutpatientMatch = 0,
                                PyxisMatch = 0,
                                Medid = fni.Medid,
                                GcnSeqNo = fni.GcnSeqno ?? -1,
                                HiclSeqNo = fni.HiclSeqno ?? -1
                            }).Distinct().ToList()
                    );

                    break;
            } //end switch (search type)

            return ret;
        } //end FormularySearchCandidates

        private IEnumerable<string> NonFormularySearchResults(EmarOrderType searchType, string searchString, int userId,
            string deptCode)
        {
            switch (searchType)
            {
                //case EmarOrderType.All:
                    // The "All" Query is the same as the "else" catch-all query, so putting it in the default case

                case EmarOrderType.DepartmentPreferredListItem:
                    //Perform the "prefered list" search.
                    //EMAR-320.  Winston Murdock, 09/29/2020
                    // Test is already written to query the database correctly. If this test doesn't pass, the code below is wrong.
                    if (deptCode == null)
                    {
                        //We don't have a department code.
                        //Do not filter based on department code.
                        return (from m in _context.Medications
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
                                select md)
                        .GroupBy(i => i.BrandName)
                        .Select(i => i.Key)
                        .ToList(); ;
                    }
                    else
                    {
                        //We do have a department code.
                        //Use it in the filter.
                        return (from m in _context.Medications
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
                                select md)
                        .GroupBy(i => i.BrandName)
                        .Select(i => i.Key)
                        .ToList(); ;
                    } //end if (deptCode == null)

                        

                case EmarOrderType.GroupRememberedOrder:
                    //Perform the "groups" search.
                    //EMAR-321.  Winston Murdock, 09/24/2020
                    // Test is already written to query the database correctly. (had to write two tests because the
                    // test for site 16 returns 0, so went for site 28, but that returns 7, which current code also returns
                    // In short, once both tests work, the code is correct(er)
                    return (from m in _context.Medications
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
                                     select md)
                        .GroupBy(i => i.BrandName)
                        .Select(i => i.Key)
                        .ToList();

                case EmarOrderType.UserQuickListItem:
                    //Perform the "user quicklist" search.
                    //EMAR-321.  Winston Murdock, 09/24/2020
                    // Test is already written to query the database correctly. If this test doesn't pass, the code below is wrong.
                    return (from m in _context.Medications
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
                                             select md)
                        .GroupBy(i => i.BrandName)
                        .Select(i => i.Key)
                        .ToList();

                default:
                    //Perform the "all" search.
                    //Since we aren't doing formulary filtering, we don't need
                    //anything from the site_formulary_match or fdb_ndc_info tables.
                    //We just use medications and medication_details to return values.
                    return (from m in _context.Medications
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
                                   select md)
                        .GroupBy(i => i.BrandName)
                        .Select(i => i.Key)
                        .ToList(); ;
            } //end switch (search type)
        } //end NonFormularySearchResults

        private List<MedicationLookup> ApplyFormularyFilterToList(List<MedicationLookup> meds)
        {
            // If any of the medIds returned don't have a site_formulary_match record, then fill them out now
            if (meds.Any(m => !m.OutpatientMatch.HasValue))
            {
                //Grab the info from the fdb_ndc_info table for all medications in the site_formulary table for this site.
                //We'll also grab the I/O/P value for each drug from site_formulary.
                var fdbInfoForFormularyMeds =
                (
                    from fni in _context.FdbNdcInfo
                    join md in _context.MedicationDetails on fni.Medid.ToString() equals md.DrugId
                    join sf in _context.SiteFormulary on new { md.MedicationId, SiteId = _siteId } equals new
                    { sf.MedicationId, sf.SiteId }
                    select (new { fni, sf.IsInpatient, sf.IsOutpatient, sf.IsPyxis })
                ).ToList();

                //Loop through all the medications that are not in the match table.
                //Compare each of them with all of the fdb IDs in fdbInfoForFormularyMeds.
                //Use those to determine what match value to calculate for this medication.
                foreach (var med in meds.Where(m => !m.OutpatientMatch.HasValue))
                {
                    // Get the match level for the inpatient formulary
                    byte matchInpt = 0;
                    if (fdbInfoForFormularyMeds.Any(m => m.IsInpatient && m.fni.Medid == med.Medid))
                        matchInpt = 3;
                    else if (fdbInfoForFormularyMeds.Any(m => m.IsInpatient && m.fni.GcnSeqno == med.GcnSeqNo))
                        matchInpt = 2;
                    else if (fdbInfoForFormularyMeds.Any(m => m.IsInpatient && m.fni.HiclSeqno == med.HiclSeqNo))
                        matchInpt = 1;

                    // Get the match level for the outpatient formulary
                    byte matchOutpt = 0;
                    if (fdbInfoForFormularyMeds.Any(m => m.IsOutpatient && m.fni.Medid == med.Medid))
                        matchOutpt = 3;
                    else if (fdbInfoForFormularyMeds.Any(m => m.IsOutpatient && m.fni.GcnSeqno == med.GcnSeqNo))
                        matchOutpt = 2;
                    else if (fdbInfoForFormularyMeds.Any(m => m.IsOutpatient && m.fni.HiclSeqno == med.HiclSeqNo))
                        matchOutpt = 1;

                    // Get the match level for the Pyxis formulary
                    byte matchPyxis = 0;
                    if (fdbInfoForFormularyMeds.Any(m => m.IsPyxis && m.fni.Medid == med.Medid))
                        matchPyxis = 3;
                    else if (fdbInfoForFormularyMeds.Any(m => m.IsPyxis && m.fni.GcnSeqno == med.GcnSeqNo))
                        matchPyxis = 2;
                    else if (fdbInfoForFormularyMeds.Any(m => m.IsPyxis && m.fni.HiclSeqno == med.HiclSeqNo))
                        matchPyxis = 1;

                    //Now that we've set the inpatient, outpatient, and pyxis variables, add a new site formulary match
                    //object to the DB context (we'll save the changes after the loop).
                    _context.SiteFormularyMatch.Add(new SiteFormularyMatch
                    {
                        SiteId = _siteId,
                        InpatientMatch = matchInpt,
                        OutpatientMatch = matchOutpt,
                        PyxisMatch = matchPyxis,
                        MedicationId = med.MedicationId
                    });

                    med.InpatientMatch = matchInpt;
                    med.OutpatientMatch = matchOutpt;
                    med.PyxisMatch = matchPyxis;
                } //end foreach loop

                //Save the changes to the database (i.e. perform all the inserts).
                //Doing it this way lets us only do one DB operation for all the inserts
                //rather than doing one operation for each time through the loop above.
                using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    } //end try/catch
                } //end using
            } //end if (are there any meds who are not in the site_formulary_match table?)

            // now every med has is in the site_formulary_match table.
            int matchLevel = _exactMatch ? 3 : 1;

            //Return variable.
            var brandNameCandidates = new List<string>();
            var retList = new List<MedicationLookup>();

            //If the site is doing inpatient filtering, then filter the list of
            //medications to only include those who match on inpatient.
            if (_inpat)
            {
                retList.AddRange
                (
                    meds
                        .Where(m => m.InpatientMatch >= matchLevel)
                );
            } //end if

            //If the site is doing outpatient filtering, then filter the list of
            //medications to only include those who match on outpatient.
            if (_outpat)
            {
                retList.AddRange
                (
                    meds
                        .Where(m => m.OutpatientMatch >= matchLevel)
                );
            } //end if

            //If the site is doing pyxis filtering, then filter the list of
            //medications to only include those who match on pyxis.
            if (_pyxis)
            {
                retList.AddRange
                (
                    meds
                        .Where(m => m.PyxisMatch >= matchLevel)
                );
            } //end if


            // Now that all the records have their appropriate match values, return the list.
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

        public List<UserQuickListItem> ApplyFormularyFilterToQuickList(Expression<Func<UserQuickListItem, bool>> whereExpression, int siteId)
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
            throw new NotImplementedException();

            //IEnumerable<UserQuickListItem> retList = new List<UserQuickListItem>();

            //List<MedicationLookup> meds = new List<MedicationLookup>();

            ////Somehow get the values for fdb_ndb_info and site_formulary_match for each medication in the parameter list.
            ////As with FormularySearchCandidates, this will have two queries: one for the medications that do
            ////have a row in the match table and one for those that do not.
            ////This is the original query from OrderRepository.GetUserQuickListTabItems()
            ////The only change is that we've added in the join to fni and sfm.
            //var uqli =
            //    (
            //        from s in
            //        (
            //        _context.UserQuickListItems
            //        .Where(whereExpression)
            //        .Include(i => i.Medication)
            //            .ThenInclude(m => m.MedicationDetails)
            //                .ThenInclude(d => d.FdbBrandName)
            //        .Include(i => i.Medication)
            //            .ThenInclude(m => m.MedicationDetails)
            //                .ThenInclude(md => md.MedicationUnit)
            //        .Include(i => i.MedicationRoute)
            //        .Include(i => i.MedicationUnit)
            //        .Include(i => i.FrequencySchedule)
            //        )
            //        join fni in _context.FdbNdcInfo on s.Medication.DrugId equals fni.Medid.ToString()
            //        join sfm in _context.SiteFormularyMatch on new { s.MedicationId, SiteId = siteId } equals new
            //        { sfm.MedicationId, sfm.SiteId }
            //        select new MedicationLookup
            //        {
            //            BrandName = "",
            //            DrugId = s.Medication.DrugId,
            //            MedicationId = s.MedicationId,
            //            InpatientMatch = sfm.InpatientMatch,
            //            OutpatientMatch = sfm.OutpatientMatch,
            //            PyxisMatch = sfm.PyxisMatch,
            //            Medid = fni.Medid,
            //            GcnSeqNo = fni.GcnSeqno ?? -1,
            //            HiclSeqNo = fni.HiclSeqno ?? -1,
            //            uqliItem = s
            //        }
            //    ).ToList();

            ////Also need to write the linq to pull the ones from the list that are NOT in
            ////the site_formulary_match table and call .AddRange.


            ////Call the filter method.
            ////TODO: Need to figure out what's going on with the filter method and this list.
            ////It's filtering out every row from uqli, not sure why.
            ////Need to debug that on Monday.
            //var filteredMeds = ApplyFormularyFilterToList(uqli);
            //return filteredMeds.Select(z => z.uqliItem).Distinct().ToList();

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

            public decimal Medid { get; set; }

            public decimal GcnSeqNo { get; set; }
            
            public decimal HiclSeqNo { get; set; }

            public UserQuickListItem uqliItem { get; set; }

            public GroupListItem gliItem { get; set; }

            public DepartmentPreferredListItem dpliItem { get; set; }
        } //end class MedicationLookup
    }
}