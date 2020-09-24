using Emar.Core.Options.Repository;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emar.Core.Medications.Repository
{
    public class DrugDbRepositoryFdb : IDrugDbRepository
    {
        private readonly EmarContext _context;
        private readonly IOptionRepository _optionRepository;
        private string _inpat;
        private string _outpat;
        private string _pyxis;
        private string _exactMatch;

        public DrugDbRepositoryFdb(EmarContext context, IOptionRepository optionRepository)
        {
            _context = context;
            _optionRepository = optionRepository;
        }

        public IEnumerable<string> GetMedsByBrandName(int siteId, string search, int userId, Model.MedicationLookupDto.SearchType searchType)
        {
            //*****************************************
            //Name:         GetMedsByBrandName
            //Author:       Winston Murdock
            //Date:         09/23/2020
            //Purpose:      Peform the medication search.
            //
            //Params:
            //siteId - The ID of the site that the user is logged into (sites.id)
            //search - The medication name we are searching for
            //userId - The ID of the logged in user (users.id)
            //searchType - The type of search being performed (all, formulary,
            //              group, department preferred list, or user quick list
            //
            //Note:         As of 09/23/2020, only the "all" and "formulary"
            //                  searches have been written.  Groups,
            //                  department preferred list, and user quick
            //                  list will come later.
            //Winston Murdock, 09/23/2020.
            //*****************************************

            //Return variable (list of medication names)
            IEnumerable<string> medsToReturn = null;

            //String that we add each medication name to.
            //This is split into a list, made distinct, and then sorted.
            string sMedNamesToReturn = "";

            //MedicationLookup objects.
            IEnumerable<MedicationLookup> medsListMatch;
            IEnumerable<MedicationLookup> medsListNoMatch;

            //Variables used to calculate the match value
            //when comparing a medication to the formulary.
            byte medInpatientMatch = 0;
            byte medOutpatientMatch = 0;
            byte medPyxisMatch = 0;
            byte tempMatch = 0;
            byte storedTempMatch = 0;

            //The queries for medication searches do a Like comparison.
            //By having these in local variables, I end up with "LIKE @p0", "Like @p1", and "LIKE @p2".
            //This was the easiest way to get the like clauses to be correct.
            string sLike1 = "%" + search + "%"; //anywhere in brand name
            string sLike2 = search + "%"; //at the start of active list
            string sLike3 = "%/ {" + search + "%"; //after a / and space in active list


            //Get the Y/N for I/O/P and exact match.
            _inpat = _optionRepository.GetOption(siteId, "MEDINPAT").ToUpper();
            _outpat = _optionRepository.GetOption(siteId, "MEDOUTPAT").ToUpper();
            _pyxis = _optionRepository.GetOption(siteId, "MEDPYXIS").ToUpper();
            _exactMatch = _optionRepository.GetOption(siteId, "MEDEXACTMATCH").ToUpper();
            
            //If this is an "all" search, then return all medications regardless of their formulary status.
            if (searchType == Model.MedicationLookupDto.SearchType.all)
            {
                //Perform the "all" search.
                //Since we aren't doing formulary filtering, we don't need
                //anything from the site_formulary_match or fdb_ndc_info tables.
                //We just use medications and medication_details to return values.
                var allQuery = from m in _context.Medications
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
                                   EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                   EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                                   EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                               )
                               //Order by the brand name.
                               orderby md.BrandName
                               //Select the MedicationDetails table.
                               //We'll use a mapper to grab only the columns we need and store into a DTO later on.
                               select md;

                //Run the query and return the results as a list.
                //This grouping prevents duplicate medication names from showing here.
                medsToReturn = allQuery
                    .GroupBy(i => i.BrandName)
                    .Select(i => i.Key)
                    .ToList();
           
                //Regardless of which of these four searches we did,
                //return the list from the base search.
                //No formulary filtering needed.
                return medsToReturn;
            } //end if (search type = all?)


            //If inpat, outpat, and pyxis are all "N", then this site isn't using the formulary filtering.
            //Pull the list of medications without regards to whether this medication is in the match
            //table or not, and then return that list.
            if (_inpat == "N" && _outpat == "N" && _pyxis == "N")
            {
                //All three are "N".
                //Do the search based on the type and return the results.
                //Don't go down to the formulary filtering logic below.
                if (searchType == Model.MedicationLookupDto.SearchType.deptpreferredlist)
                {
                    //Perform the "prefered list" search.
                    //medsToReturn = Query/execute here.
                    //I'll need the department code for this (which means the UI will need to pass it along in the page header)
                    //This is outside the scope of EMAR-57, so I'll revisit this whenever I get to its ticket.
                    //See Department Preferred List Query With No Filtering.sql.

                    throw new NotImplementedException();
                }
                else if (searchType == Model.MedicationLookupDto.SearchType.groups)
                {
                    //Perform the "groups" search.
                    //EMAR-321.  Winston Murdock, 09/24/2020
                    var groupQuery = from m in _context.Medications
                                   //Join to the Medication Details table
                                   join md in _context.MedicationDetails on m.Id equals md.MedicationId
                                   //Join to the group_list_items table.
                                   join gli in _context.GroupListItems on m.Id equals gli.MedicationId
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
                                       EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                                       EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                                       EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                                   )
                                   //Order by the brand name.
                                   orderby md.BrandName
                                   //Select the MedicationDetails table.
                                   //We'll use a mapper to grab only the columns we need and store into a DTO later on.
                                   select md;

                    //Run the query and return the results as a list.
                    //This grouping prevents duplicate medication names from showing here.
                    medsToReturn = groupQuery
                        .GroupBy(i => i.BrandName)
                        .Select(i => i.Key)
                        .ToList();

                    //Regardless of which of these four searches we did,
                    //return the list from the base search.
                    //No formulary filtering needed.
                    return medsToReturn;
                }
                else if (searchType == Model.MedicationLookupDto.SearchType.quicklist)
                {
                    //Perform the "user quicklist" search.
                    //medsToReturn = Query/execute here.
                    //This is outside the scope of EMAR-57, so I'll revisit it whenever I get to its ticket
                    //See User Quick List Query With No Filtering.sql.

                    throw new NotImplementedException();
                }
                else
                {
                    //Perform the "all" search as we aren't doing any fmrulary filtering.
                    var formularyQuery = from m in _context.Medications
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
                            EF.Functions.Like(md.BrandName, $"%{search}%") || //anywhere in brand name
                            EF.Functions.Like(md.ActiveList, $"{search}%") || //at the start of active list
                            EF.Functions.Like(md.ActiveList, $"%/ {search}%") //after a / and space in active list
                        )
                        //Order by the brand name.
                        orderby md.BrandName
                        //Select the MedicationDetails table.
                        //We'll use a mapper to grab only the columns we need and store into a DTO later on.
                        select md;

                    //Run the query and return the results as a list.
                    //This grouping prevents duplicate medication names from showing here.
                    medsToReturn = formularyQuery
                        .GroupBy(i => i.BrandName)
                        .Select(i => i.Key)
                        .ToList();
                } // end if
                
                //Regardless of which of these four searches we did,
                //return the list from the base search.
                //No formulary filtering needed.
                return medsToReturn;
            }
            else
            {
                //At least one of the formulary filters is turned on.
                //Thusly, we need to apply the formulary filtering logic.
                //My number one goal here is to minimize the number of trips to the DB.
                //My first pass at this resulted in 3n + 1 SQL selects.
                //My current plan results in exactly 3 SQL Selects, regardless
                //of how many medications match the search criteria.
                
                //Do the search based on the type and then filter the results below.
                if (searchType == Model.MedicationLookupDto.SearchType.deptpreferredlist)
                {
                    //Perform the "prefered list" search.
                    //medsListMatch = _context.MedicationLookups.FromSqlInterpolated($"").ToList();
                    //medsListNoMatch = _context.MedicationLookups.FromSqlInterpolated($"").ToList();
                    //I'll need the department code for this (which means the UI will need to pass it along in the page header)
                    //This is outside the scope of EMAR-57, so I'll revisit it whenever I get to its ticket.
                    //See Department Preferred List Query With No Filtering.sql.

                    throw new NotImplementedException();
                }
                else if (searchType == Model.MedicationLookupDto.SearchType.groups)
                {
                    //Perform the "groups" search.
                    //EMAR-321.  Winston Murdock, 09/24/2020
                    medsListMatch = _context.MedicationLookups.FromSqlInterpolated($"SELECT DISTINCT md.brand_name, md.drug_id, md.medication_id, sfm.inpatient_match, sfm.outpatient_match, sfm.pyxis_match, fni.medid, fni.GCN_SEQNO, fni.HICL_SEQNO FROM medication_details md INNER JOIN medications med ON md.medication_id = med.id INNER JOIN site_formulary_match sfm ON med.id = sfm.medication_id INNER JOIN fdb_ndc_info fni on med.drug_id = CONVERT(varchar(50), fni.medid) INNER JOIN group_list_items gli on med.id = gli.medication_id WHERE LEN(md.brand_name) > 0 AND md.is_active = 1 AND (md.brand_name LIKE {sLike1} OR md.active_list LIKE {sLike2} OR md.active_list LIKE {sLike3}) AND sfm.site_id = {siteId} ORDER BY md.brand_name").ToList();

                    medsListNoMatch = _context.MedicationLookups.FromSqlInterpolated($"SELECT DISTINCT md.brand_name, md.drug_id, md.medication_id, CONVERT(tinyint, 0) as inpatient_match, CONVERT(tinyint, 0) as outpatient_match, CONVERT(tinyint, 0) as pyxis_match, fni.medid, fni.GCN_SEQNO, fni.HICL_SEQNO FROM medication_details md INNER JOIN medications med ON md.medication_id = med.id LEFT JOIN site_formulary_match sfm  ON med.id = sfm.medication_id and sfm.site_id = {siteId} INNER JOIN fdb_ndc_info fni on med.drug_id = CONVERT(varchar(50), fni.medid) INNER JOIN group_list_items gli on med.id = gli.medication_id WHERE LEN(md.brand_name) > 0 AND md.is_active = 1 AND sfm.inpatient_match IS NULL AND (md.brand_name LIKE {sLike1} OR md.active_list LIKE {sLike2} OR md.active_list LIKE {sLike3}) ORDER BY md.brand_name").ToList();
                }
                else if (searchType == Model.MedicationLookupDto.SearchType.quicklist)
                {
                    //Perform the "user quicklist" search.
                    //medsListMatch = _context.MedicationLookups.FromSqlInterpolated($"").ToList();
                    //medsListNoMatch = _context.MedicationLookups.FromSqlInterpolated($"").ToList();
                    //This is outside the scope of EMAR-57, so I'll revisit it whenever I get to its ticket
                    //See User Quick List Query With No Filtering.sql.

                    throw new NotImplementedException();
                }
                else
                {
                    //Perform the "all" search.
                    //join to site_formulary_match (to only include medications that
                    //are already in the match table for this site) and to get the "match" values from it.
                    //Also join to fdb_ndc_info to get the ids from it.
                    //Winston Murdock, 09/22/2020
                    medsListMatch = _context.MedicationLookups.FromSqlInterpolated($"SELECT DISTINCT md.brand_name, md.drug_id, md.medication_id, sfm.inpatient_match, sfm.outpatient_match, sfm.pyxis_match, fni.medid, fni.GCN_SEQNO, fni.HICL_SEQNO FROM medication_details md INNER JOIN medications med ON md.medication_id = med.id INNER JOIN site_formulary_match sfm ON med.id = sfm.medication_id INNER JOIN fdb_ndc_info fni on med.drug_id = CONVERT(varchar(50), fni.medid) WHERE LEN(md.brand_name) > 0 AND md.is_active = 1 AND (md.brand_name LIKE {sLike1} OR md.active_list LIKE {sLike2} OR md.active_list LIKE {sLike3}) AND sfm.site_id = {siteId} ORDER BY md.brand_name").ToList();

                    //Also perform the search to get all medications that match the search criteria
                    //and that are not in site_formulary_match.
                    //Winston Murdock, 09/22/2020
                    medsListNoMatch = _context.MedicationLookups.FromSqlInterpolated($"SELECT DISTINCT md.brand_name, md.drug_id, md.medication_id, CONVERT(tinyint, 0) as inpatient_match, CONVERT(tinyint, 0) as outpatient_match, CONVERT(tinyint, 0) as pyxis_match, fni.medid, fni.GCN_SEQNO, fni.HICL_SEQNO FROM medication_details md INNER JOIN medications med ON md.medication_id = med.id LEFT JOIN site_formulary_match sfm  ON med.id = sfm.medication_id and sfm.site_id = {siteId} INNER JOIN fdb_ndc_info fni on med.drug_id = CONVERT(varchar(50), fni.medid) WHERE LEN(md.brand_name) > 0 AND md.is_active = 1 AND sfm.inpatient_match IS NULL AND (md.brand_name LIKE {sLike1} OR md.active_list LIKE {sLike2} OR md.active_list LIKE {sLike3}) ORDER BY md.brand_name").ToList();
                } // end if

                //For each medication that is in the "match" table, evaluate whether or not to include it.
                //If include = true, then add it to the list of medication names to return.
                foreach (MedicationLookup ml in medsListMatch)
                {
                    //Call a helper method that uses this medication's "match" values and the
                    //I/O/P/exact match settings to determine whethe or not to include this medication.
                    //If it evalues to true, then include it.
                    if (IncludeThisMed(ml.InpatientMatch, ml.OutpatientMatch, ml.PyxisMatch))
                    {
                        //Add this medicaiton to the return list.
                        sMedNamesToReturn += ml.BrandName + ",";
                    } //end if
                } //end foreach

                //Now that we've included anything already in the "match" table that should be included,
                //we need to handle the ones that are not already in the "match" table.

                //Grab the info (ids and such) for all medications in the site_formulary table for this site.
                //We'll make the SQL call once and keep in a variable in memory so that each mediation
                //in medsListNoMatch can check against it.
                //Say we searched for Acetaminophen when it isn't in the match table, but Tylenol is in the formulary.
                //We would need to compare Acetaminophen's values from fdb_ndc_info
                //to Tylenol's values from fdb_ndc_info and calculate the match values for Acetaminophen.
                //Distinct is needed because the same medid can be in fdb_ndc_info multiple times.
                //I only care about Medid, GCB)SEQNO, and HICL_SEQNO (which will be the same for all Tylenol entries).
                //For the rest of the columns, I specify default/empty values and then do as 'column_name'
                //This way C# is happy because we include all of the SQL coumns that the entity is expecting.
                //Winston Murdock, 09/19/2020.
                List<FdbNdcInfo> fdbInfoForFormularyMeds = _context.FdbNdcInfo.FromSqlInterpolated($"SELECT distinct fni.medid, fni.GCN_SEQNO, fni.HICL_SEQNO, '' as 'ndc', '' as 'base_ndc', 0 as 'repackaged', '' as 'packaging',  '' as 'strength', 999999999 as 'days_obsolete', 0 as 'ROUTED_GEN_ID' FROM site_formulary sf INNER JOIN medication_details md ON sf.medication_id = md.medication_id INNER JOIN fdb_ndc_info fni on md.drug_id = fni.medid WHERE sf.site_id = {siteId}").ToList();

                //Loop through all the rows in medsListNoMatch.
                //Compare each of the medications in medsListNoMatch
                //with all of the fdb IDs in fdbInfoForFormularyMeds.
                //Use those to determine what match value to calculate for this medication.
                foreach (MedicationLookup ml2 in medsListNoMatch)
                {
                    //Reset variables for each iteration through the loop.
                    medInpatientMatch = 0;
                    medOutpatientMatch = 0;
                    medPyxisMatch = 0;
                    storedTempMatch = 0;

                    //Loop through all of the meds that are on this site's formulary..
                    foreach (FdbNdcInfo fni in fdbInfoForFormularyMeds)
                    {
                        //Loop through all of the fni rows for each drug in the site_formulary table
                        //For each one...
                        //  If we find a match on medid, then this is a 3 (exact match).
                        //  If we don't find a match on medid but we do find a match on gcn, then this is a 2.
                        //  If we don't find a match on gcn but do find a match on hicl, then this is a 1.
                        //  Else this is a 0.
                        //  If the current iteration value is higher than the existing value, overwrite the stored value.
                        //  Else don't overwrite the stored value.
                        //  If the stored value is 3 or 4, then exit the for loop as that's the highest value we can evaluate to.
                        //End Loop
                        foreach (FdbNdcInfo infoForIndividualFormularyMed in fdbInfoForFormularyMeds)
                        {
                            //Reset variables for each iteration through the loop.
                            tempMatch = 0;

                            //Check the fields to see which fields match (if any).
                            if (infoForIndividualFormularyMed.Medid == ml2.Medid)
                            {
                                //If medid matches, then this is an exact match.
                                //Romel says I can make this a 3 or 4 (I chose 3).
                                tempMatch = 3;
                            }
                            else if (infoForIndividualFormularyMed.GcnSeqNo == ml2.GcnSeqNo)
                            {
                                //If medid doesn't match but GCN does match, then this is a 2.
                                tempMatch = 2;
                            }
                            else if (infoForIndividualFormularyMed.HiclSeqNo == ml2.HiclSeqNo)
                            {
                                //If medid and GCN don't match, but HICL does match, then this is a 1.
                                tempMatch = 1;
                            }
                            else
                            {
                                //If all of medid, GCN, and HICL do not match, then this is a zero.
                                tempMatch = 0;
                            }//end if

                            //If the value of tempMatch is larger than the stored value then update it.
                            //Also, if the value is 0, then update it regardless.
                            if (tempMatch > storedTempMatch)
                            {
                                storedTempMatch = tempMatch;
                            } //end if

                            //If we've already calculated this to be an exact match (i.e. 3 or 4),
                            //then there's no point in continuing futher.
                            //Break the loop.
                            if (storedTempMatch == 3 || storedTempMatch == 4)
                            {
                                break;
                            } //end if
                        } //end foreach

                        //Now that we've calculated the match value for this medication, see which combination of
                        //Inpatient, Outpatient, and Pyxis the match value will apply to.
                        //If it applies to that one, then set that variable to the calculated value.
                        //Else, set the variable to 0.
                        if (_inpat == "Y")
                        {
                            medInpatientMatch = storedTempMatch;
                        }
                        else
                        {
                            medInpatientMatch = 0;
                        } //end if
                        if (_outpat == "Y")
                        {
                            medOutpatientMatch = storedTempMatch;
                        }
                        else
                        {
                            medOutpatientMatch = 0;
                        } //end if
                        if (_pyxis == "Y")
                        {
                            medPyxisMatch = storedTempMatch;
                        }
                        else
                        {
                            medPyxisMatch = 0;
                        } //end if

                        //Now that we've set the inpatient, outpatient, and pyxis variables, insert into the site_formulary_match table.
                        //Create a new SiteFormularyMatch object and then insert it into the table.
                        SiteFormularyMatch sfmToInsert = new SiteFormularyMatch();
                        sfmToInsert.SiteId = siteId;
                        sfmToInsert.InpatientMatch = medInpatientMatch;
                        sfmToInsert.OutpatientMatch = medOutpatientMatch;
                        sfmToInsert.PyxisMatch = medPyxisMatch;
                        sfmToInsert.MedicationId = ml2.MedicationId;

                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            try
                            {
                                _context.SiteFormularyMatch.Add(sfmToInsert);
                                _context.SaveChanges();
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                            } //end try/catch
                        } //end using
                    } //end foreach.(through all medications that were not in the match table).

                    //Call a helper method that uses this medication's "match" values and the
                    //I/O/P/exact match settings to determine whethe or not to include this medication.
                    //If it evalues to true, then include it.
                    if (IncludeThisMed(medInpatientMatch, medOutpatientMatch, medPyxisMatch))
                    {
                        //Add this medicaiton to the return list.
                        sMedNamesToReturn += ml2.BrandName + ",";
                    } //end if
                } //end foreach loop
            } //end if (I/O/P equal "N"?)

            //We've added everything to the return string (both those medications that
            //were already in the match table and those we just added to the match table).
            //Split the string of medication names on comma, gorup by medication name
            //(so that duplicate medication names are filtered out), sort by medication
            //name, and then return the list.
            return sMedNamesToReturn.Split(",").GroupBy(i => i).Select(i => i.Key).OrderBy(i => i).ToList();
        } //end function GetMedsByBrandName

        private Boolean IncludeThisMed(byte inpatientMatch, byte outpatientMatch, byte pyxisMatch)
        {
            //Use the Y/N for inpatient, outpatient, and pyxis allong with the
            //the Y/N for exact match and the match values for this medication
            //to determine whether or not to include it in the results going to the UI.
            
            //Default the overall include boolean to false.
            Boolean bInclude = false;
            
            //Default the the yes/no for each individual 
            Boolean bInpatSaysInclude = true;
            Boolean bOutpatSaysInclude = true;
            Boolean bPyxisSaysInclude = true;

            //Use the site-specific settings for I/O/P/Exact match to determine whether or not to include this medication.
            //Inpatient
            if (_inpat == "Y")
            {
                //Checking inpat filter.
                //Check exact match.
                if (_exactMatch == "Y")
                {
                    //Doing exact match.
                    //Need a 3 or 4.
                    if (inpatientMatch >= 3)
                    {
                        bInpatSaysInclude = true;
                    }
                    else
                    {
                        bInpatSaysInclude = false;
                    } //end if
                }
                else
                {
                    //Not doing exact match.
                    //Need a 1, 2, 3, or 4.
                    if (inpatientMatch >= 1)
                    {
                        bInpatSaysInclude = true;
                    }
                    else
                    {
                        bInpatSaysInclude = false;
                    } //end if

                } //end if
            }
            else
            {
                //Set the flag to false so that the inpat value doesn't affect the calculation
                //of whether or not to include this medication.
                bInpatSaysInclude = false;
            } //end if (Inpat)

            //Outpatient
            if (_outpat == "Y")
            {
                //Checking outpat filter.
                //Check exact match.
                if (_exactMatch == "Y")
                {
                    //Doing exact match.
                    //Need a 3 or 4.
                    if (outpatientMatch >= 3)
                    {
                        bOutpatSaysInclude = true;
                    }
                    else
                    {
                        bOutpatSaysInclude = false;
                    } //end if
                }
                else
                {
                    //Not doing exact match.
                    //Need a 1, 2, 3, or 4.
                    if (outpatientMatch >= 1)
                    {
                        bOutpatSaysInclude = true;
                    }
                    else
                    {
                        bOutpatSaysInclude = false;
                    } //end if
                } //end if
            }
            else
            {
                //Set the flag to false so that the inpat value doesn't affect the calculation
                //of whether or not to include this medication.
                bOutpatSaysInclude = false;
            } //end if (Outpat)

            //Pyxis
            if (_pyxis == "Y")
            {
                //Checking pyxis filter.
                //Check exact match.
                if (_exactMatch == "Y")
                {
                    //Doing exact match.
                    //Need a 3 or 4.
                    if (pyxisMatch >= 3)
                    {
                        bPyxisSaysInclude = true;
                    }
                    else
                    {
                        bPyxisSaysInclude = false;
                    } //end if
                }
                else
                {
                    //Not doing exact match.
                    //Need a 1, 2, 3, or 4.
                    if (pyxisMatch >= 1)
                    {
                        bPyxisSaysInclude = true;
                    }
                    else
                    {
                        bPyxisSaysInclude = false;
                    } //end if
                } //end if
            }
            else
            {
                //Set the flag to false so that the inpat value doesn't affect the calculation
                //of whether or not to include this medication.
                bPyxisSaysInclude = false;
            } //end if (Pyxis)

            //Now that we've set all three flags, look at them.
            //If they're all false, then we arn't including this medication.
            //If even one of them is true, then we are including this medication.
            //Since all false also means we aren't doing nay filtering, I checked that way up at the top.
            //If I, O, and P are all "N", then we simply return the parameter list.
            if (bInpatSaysInclude || bOutpatSaysInclude || bPyxisSaysInclude)
            {
                //One, or more, of the flags says to include this medication.
                //Thus, include it.
                bInclude = true;
            } //end if

            //Return
            return bInclude;
        } //end IncludeThisMed

        //public IEnumerable<string> ApplyFormularyFiltertoList(IEnumerable<string> medications, int siteId)
        private IEnumerable<string> ApplyFormularyFiltertoList(IEnumerable<string> medications, int siteId)
        {
            //************************************
            //If there are n medications in the medications list, this will make
            //3n + 1 database calls to pull data.
            //IF n = 6, then this makes 19 database calls.
            //If n = 113, then this makes 340 database calls.
            //I rewrote this to make exactly 3 DB calls no matter how many medications
            //are in the medications list, and it is much more performant.
            //But I want to kepe this around as a monument to my stupidity.
            //I've made it private so that no other file can call this.
            //Winston Murdock, 09/23/2020.
            //************************************

            //Apply the formulary filtering logic to the list of medications and only return the
            //medications that match the filtering criteria.
            //EMAR-57.  Winston Murdock, 09/11/2020.

            var sMedNamesToReturn = "";

            //Get the site setting for inpat, outpat, pyxis, and exactmatch.
            _inpat = _optionRepository.GetOption(siteId, "MEDINPAT").ToUpper();
            _outpat = _optionRepository.GetOption(siteId, "MEDOUTPAT").ToUpper();
            _pyxis = _optionRepository.GetOption(siteId, "MEDPYXIS").ToUpper();
            _exactMatch = _optionRepository.GetOption(siteId, "MEDEXACTMATCH").ToUpper();

            //If inpat, outpat, and pyxis are all "N", then this site isn't using the formulary filtering.
            //Just return the parameter list and be done.
            if (_inpat == "N" && _outpat == "N" && _pyxis == "N")
            {
                //All three are "N".
                //Return the original list.
                return medications;
            }
            {
                //At least one of the three is "Y".
                
                //Loop through all of the medications in the list.
                foreach (string m in medications)
                {
                     //Declare booleans for Whether or not inpat, outpat, and pyxis say to keep this medication in the list or not.
                    //If all three are false, then we will not add this object to the return list.
                    //If at least one of these is true at the end of the loop, then we'll add this Medication Detail object to the return list.
                    Boolean bInpatSaysInclude = true;
                    Boolean bOutpatSaysInclude = true;
                    Boolean bPyxisSaysInclude = true;
                    byte medInpatientMatch = 0;
                    byte medOutpatientMatch = 0;
                    byte medPyxisMatch = 0;
                    byte tempMatch = 0;
                    byte storedTempMatch = 0;
                    decimal medidForSearchedMed = 0;
                    decimal gcn_SeqnoForSearchedMed = 0;
                    decimal hiclForSearchedMed = 0;

                    //Attempt to find this medication in the site_formulary_match table.
                    //If one drug happens to be in the match table multiple times, we only care about the first one.
                    //Amoxicillin seems to be in there multiple times.
                    //Need a MedicationDetails entity.
                    var formularyMatch = _context.MedicationDetails
                        .Include(md => md.Medication)
                            .ThenInclude(m => m.SiteFormularyMatchs)
                            .Where(md =>
                                md.Medication.SiteFormularyMatchs.Any(sfm => sfm.SiteId == siteId)
                                && md.IsActive == true
                                && md.BrandName == m
                            ).ToList();

                    //Going to try to write this as an iQueryable.
                    //IQueryable<MedicationDetail> formularyMatchQuery = from md in _context.MedicationDetails
                    //                                               join med in _context.Medications on md.MedicationId equals med.Id
                    //                                               join sfm in _context.SiteFormularyMatch on med.Id equals sfm.MedicationId
                    //                                               where md.IsActive == true
                    //                                               && md.BrandName == m
                    //                                               && sfm.SiteId == siteId
                    //                                               select md;

                    //var formularyMatch = formularyMatchQuery.ToList();


                    //Take the list of medication details and grab whichever one has the highest match value.

                    //If the above search found one or more rows, then this medication is already in the match table.
                    //Grab the "match" fields for this medication.
                    //And use the largest values.
                    if (formularyMatch.Count > 0)
                    {
                        //This medication was found in the match table.
                        //Could be once, could be multiple rows.
                        //For each one, grab the site_formulary_match rows for it.
                        foreach (MedicationDetail md in formularyMatch)
                        {
                            //Grab all rows in the site_formulary_match table
                            //that match on siteId and MedicationId.
                            var formularyMatchesFromSfm = _context.SiteFormularyMatch
                               .Where(sfm => sfm.SiteId == siteId && sfm.MedicationId == md.MedicationId).ToList();

                            //Loop through all of the sfm matches.
                            //Compare the "match" fields to the current "match" variables.
                            //If the fields in this one are larger, then overwrite the variables.
                            //In my testing, this is always one record.
                            //But it theoertically could be multiple records.
                            foreach (SiteFormularyMatch individualMatch in formularyMatchesFromSfm)
                            {
                                //Check the value for inpatient match.
                                //If it's larger than the local variable, then set the local variable to it.
                                if (individualMatch.InpatientMatch > medInpatientMatch)
                                {
                                    medInpatientMatch = individualMatch.InpatientMatch;
                                } //end if
                            
                                //Check the value for outpatient match.
                                //If it's larger than the local variable, then set the local variable to it.
                                if (individualMatch.OutpatientMatch > medOutpatientMatch)
                                {
                                    medOutpatientMatch = individualMatch.OutpatientMatch;
                                } //end if
                            
                                //Check the value for pyxis match.
                                //If it's larger than the local variable, then set the local variable to it.
                                if (individualMatch.InpatientMatch > medInpatientMatch)
                                {
                                    medInpatientMatch = individualMatch.InpatientMatch;
                                } //end if
                            } //end foreach
                        } //end foreach

                        //At this point, we've got the "match" values for inpatient, outpatient, and pyxis.
                        //Below the "else" section, we'll look at those to see if this medication should be returned or not.
                    }
                    else
                    {
                        //Else the medication is not in the match table.
                        //We need to pull all of the medications from fdb_ndc_info
                        //that have the same BrandName and that are for the same DrugVendor.
                        //Once we have the info from that table, we'll calculate the
                        //"match" value for this one and add it to the match table.
                        //Grab all medications from fdb_ndc_info that match on brand name.
                        //I don't need to include site_forumlary_match here because I explicitly
                        //want meds that are in the vendor's tables but not in our match table.
                        //Do that by filtering on BrandName and on DrugVendor.
                        //This is done as an iQueryable because this would be a "soft"
                        //foreign key (in the API only), and we ran into issues trying
                        //to create that in C#.  Perhaps it would be faster to do this
                        //as a .include and a .theninclude than this?  I don't know.
                        IQueryable<FdbNdcInfo> fdbInfoForMedQuery = from fni in _context.FdbNdcInfo
                                                                    join md in _context.MedicationDetails on fni.Medid.ToString() equals md.DrugId
                                                                    join med in _context.Medications on md.MedicationId equals med.Id
                                                                    where med.DrugVendor == "F" && md.BrandName == m
                                                                    select fni;
                        
                        //Actually execute the query.
                        //We do a FirstOrDefault because we only need one fdb_ndc_info record as 
                        //the drug ids I need are the same for all rows in the record set.
                        //for this medication.
                        var fdbInfoForMed = fdbInfoForMedQuery.FirstOrDefault();


                        //This is the above query written as a FromSqlInterpolated call.
                        //It works like this, but I"m not sure what is faster.
                        //var fdbInfoForMed = _context.FdbNdcInfo.FromSqlInterpolated($"select top 1 fni.* from fdb_ndc_info fni inner join medication_details md on fni.medid = md.drug_id inner join medications med on md.medication_id = med.id where med.drug_vendor = 'F' and md.brand_name = '{m}'").FirstOrDefault();
                        
                        //We have one entry.
                        //Grab the medid, gcn, and hicl values for it.
                        //Other drug vendors will likely have different fields.
                        //But that's part of why we're creating vendor-specific implementations of this.
                        medidForSearchedMed = fdbInfoForMed.Medid;
                        gcn_SeqnoForSearchedMed = fdbInfoForMed.GcnSeqNo;
                        hiclForSearchedMed = fdbInfoForMed.HiclSeqNo;


                        //We also need to get the values from fdb_ndc_info for all of the medications in the site_formulary table for this site.
                        //Say we searched for Acetaminophen when it isn't in the match table, but Tylenol is in the formulary.
                        //We would need to compare Acetaminophen's values from fdb_ndc_info
                        //to Tylenol's values from fdb_ndc_info and calculate the match values for Acetaminophen.
                        //Distinct is needed because the same medid can be in fdb_ndc_info multiple times.
                        //I only care about Medid, GCB)SEQNO, and HICL_SEQNO.
                        //For the rest, I specify default/empty values and then do as 'column_name'
                        //This way C# is happy because we include all of the SQL coumns that the entity is expecting.
                        //And I'm happy because we nnly return 25 rows instead of 919 (when pulling the formulary for site 16).
                        //There might be a way to make this happen with the IQueryable stuff that is commented out below.
                        //But if this works, I say roll with it.
                        //Winston Murdock, 09/19/2020.
                        var fdbInfoForFormularyMeds = _context.FdbNdcInfo.FromSqlInterpolated($"SELECT distinct fni.medid, fni.GCN_SEQNO, fni.HICL_SEQNO, '' as 'ndc', '' as 'base_ndc', 0 as 'repackaged', '' as 'packaging',  '' as 'strength', 999999999 as 'days_obsolete', 0 as 'ROUTED_GEN_ID' FROM site_formulary sf INNER JOIN medication_details md ON sf.medication_id = md.medication_id INNER JOIN fdb_ndc_info fni on md.drug_id = fni.medid WHERE sf.site_id = { siteId}").ToList();


                        //Loop through all of the fni rows for each drug in the site_formulary table
                        //For each one...
                        //  If we find a match on medid, then this is a 3 or a 4 (Romel says it doesn't amtter which one).
                        //  If we don't find a match on medid but we do find a match on gcn, then 2
                        //  If we don't find a match on gcn but do find a match on hicl, then 1.
                        //  Else 0.
                        //  If the current iteration value is higher than the existing value, then overwrite the existing value.
                        //  Else don't overwrite the existing value.
                        //End Loop
                        foreach (FdbNdcInfo infoForIndividualFormularyMed in fdbInfoForFormularyMeds)
                        {
                            tempMatch = 0;

                            //Check the fields to see which fields match (if any).
                            if (infoForIndividualFormularyMed.Medid == medidForSearchedMed)
                            {
                                //If medid matches, then this is an exact match.
                                //Romel says I can make this a 3 or 4 (I chose 3).
                                tempMatch = 3;
                            }
                            else if (infoForIndividualFormularyMed.GcnSeqNo == gcn_SeqnoForSearchedMed)
                            {
                                //If medid doesn't match but GCN does match, then this is a 2.
                                tempMatch = 2;
                            }
                            else if (infoForIndividualFormularyMed.HiclSeqNo == hiclForSearchedMed)
                            {
                                //If medid and GCN don't match, but HICL does match, then this is a 1.
                                tempMatch = 1;
                            }
                            else
                            {
                                //If all of medid, GCN, and HICL do not match, then this is a zero.
                                tempMatch = 0;
                            }//end if

                            //If the value of tempMatch is larger than the stored value then update it.
                            //Also, if the value is 0, then update it regardless.
                            if (tempMatch > storedTempMatch || storedTempMatch == 0)
                            {
                                storedTempMatch = tempMatch;
                            } //end if

                            //If we've already calculated this to be an exact match (i.e. 3 or 4),
                            //then there's no point in continuing futher.
                            //Break the loop.
                            if (storedTempMatch == 3 || storedTempMatch == 4)
                            {
                                break;
                            } //end if
                        } //end foreach

                        //Now that we've calculated the match number for this medication, see which combination of
                        //Inpatient, Outpatient, and Pyxis it needs to apply to.
                        //If it applies to that one, then set that variable to the calculated value.
                        //Else, set the variable to 0.
                        if (_inpat == "Y")
                        {
                            medInpatientMatch = storedTempMatch;
                        }
                        else
                        {
                            medInpatientMatch = 0;
                        } //end if
                        if (_outpat == "Y")
                        {
                            medOutpatientMatch = storedTempMatch;
                        }
                        else
                        {
                            medOutpatientMatch = 0;
                        } //end if
                        if (_pyxis == "Y")
                        {
                            medPyxisMatch = storedTempMatch;
                        }
                        else
                        {
                            medPyxisMatch = 0;
                        } //end if

                        //Now that we've set the inpatient, outpatient, and pyxis variables, insert into the site_formulary_match table.


                        //I currently have medications.drug_id, but I need medications.id.
                        //Make a call out to the DB to get the medication and then grab the Id from it
                        //Winston Murdock, 09/21/2020.
                        var medFromDrugId = _context.Medications
                               .Where(med => med.DrugId == medidForSearchedMed.ToString())
                               .Where(med => med.DrugVendor == "F").FirstOrDefault();

                        //Create a new SiteFormularyMatch object and then insert it into the table.
                        //SiteFormularyMatch sfmToInsert = new SiteFormularyMatch();
                        //sfmToInsert.SiteId = siteId;
                        //sfmToInsert.InpatientMatch = medInpatientMatch;
                        //sfmToInsert.OutpatientMatch = medOutpatientMatch;
                        //sfmToInsert.PyxisMatch = medPyxisMatch;
                        //sfmToInsert.MedicationId = medFromDrugId.Id;
                        //
                        //using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        //{
                        //    try
                        //    {
                        //        _context.SiteFormularyMatch.Add(sfmToInsert);
                        //        _context.SaveChanges();
                        //        transaction.Commit();
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        transaction.Rollback();
                        //    } //end try/catch
                        //} //end using
                    } //end if

                    //Now that we have the match numbers for this medication,
                    //(either we pulled them directly from the match table
                    //or we compared this medication to the drugs already on the formulary,
                    //calculated the match value from the fdb tables, and then returned that), 
                    //Use the site-specific settings for I/O/P/Exact match to determine whether or not to include this medication.

                    //Inpatient
                    if (_inpat == "Y")
                    {
                        //Checking inpat filter.
                        //Check exact match.
                        if (_exactMatch == "Y")
                        {
                            //Doing exact match.
                            //Need a 3 or 4.
                            if (medInpatientMatch >= 3)
                            {
                                bInpatSaysInclude = true;
                            }
                            else
                            {
                                bInpatSaysInclude = false;
                            } //end if
                        }
                        else
                        {
                            //Not doing exact match.
                            //Need a 1, 2, 3, or 4.
                            if (medInpatientMatch >= 1)
                            {
                                bInpatSaysInclude = true;
                            }
                            else
                            {
                                bInpatSaysInclude = false;
                            } //end if

                        } //end if
                    }
                    else
                    {
                        //Set the flag to false so that the inpat value doesn't affect the calculation
                        //of whether or not to include this medication.
                        bInpatSaysInclude = false;
                    } //end if (Inpat)
                    
                    //Outpatient
                    if (_outpat == "Y")
                    {
                        //Checking outpat filter.
                        //Check exact match.
                        if (_exactMatch == "Y")
                        {
                            //Doing exact match.
                            //Need a 3 or 4.
                            if (medOutpatientMatch >= 3)
                            {
                                bOutpatSaysInclude = true;
                            }
                            else
                            {
                                bOutpatSaysInclude = false;
                            } //end if
                        }
                        else
                        {
                            //Not doing exact match.
                            //Need a 1, 2, 3, or 4.
                            if (medOutpatientMatch >= 1)
                            {
                                bOutpatSaysInclude = true;
                            }
                            else
                            {
                                bOutpatSaysInclude = false;
                            } //end if
                        } //end if
                    }
                    else
                    {
                        //Set the flag to false so that the inpat value doesn't affect the calculation
                        //of whether or not to include this medication.
                        bOutpatSaysInclude = false;
                    } //end if (Outpat)

                    //Pyxis
                    if (_pyxis == "Y")
                    {
                        //Checking pyxis filter.
                        //Check exact match.
                        if (_exactMatch == "Y")
                        {
                            //Doing exact match.
                            //Need a 3 or 4.
                            if (medPyxisMatch >= 3)
                            {
                                bPyxisSaysInclude = true;
                            }
                            else
                            {
                                bPyxisSaysInclude = false;
                            } //end if
                        }
                        else
                        {
                            //Not doing exact match.
                            //Need a 1, 2, 3, or 4.
                            if (medPyxisMatch >= 1)
                            {
                                bPyxisSaysInclude = true;
                            }
                            else
                            {
                                bPyxisSaysInclude = false;
                            } //end if
                        } //end if
                    }
                    else
                    {
                        //Set the flag to false so that the inpat value doesn't affect the calculation
                        //of whether or not to include this medication.
                        bPyxisSaysInclude = false;
                    } //end if (Pyxis)

                    //Now that we've set all three flags, look at them.
                    //If they're all false, then we arn't including this medication.
                    //If even one of them is true, then we are including this medication.
                    //Since all false also means we aren't doing nay filtering, I checked that way up at the top.
                    //If I, O, and P are all "N", then we simply return the parameter list.
                    if (bInpatSaysInclude || bOutpatSaysInclude || bPyxisSaysInclude)
                    {
                        //One, or more, of the flags says to include this medication.
                        //Thus, include it.
                        sMedNamesToReturn += m + ",";
                    } //end if
                } //end loop through each medication name in the parameter list
                
                //Split the medication names string to an array and then convert it to a list.
                return sMedNamesToReturn.Split(",").ToList();
            } //end if (Are all three site settings set to "N"?)
        } //end ApplyFormularyFilterToList
    }
}
