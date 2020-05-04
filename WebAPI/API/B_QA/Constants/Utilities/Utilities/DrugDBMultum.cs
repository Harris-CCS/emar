using Interfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle interaction with the Multum drug database
    /// </summary>
    public class DrugDBMultum : IDrugDBUtility
    {
        public string Vendor = DrugDB.Constants.Vendors.MULTUM;
        public string Name = "Multum";
        public string DBName = "vantagerx";
        public string DBType = "M";

        /// <summary>
        /// Get the name of the Drug database vendor
        /// </summary>
        /// <returns>Multum (name of the vendor)</returns>
        public string GetDBType()
        {
            return DBType;
        }

        /// <summary>
        /// Whether this vendor's drug information should have obsoletes checked
        /// </summary>
        /// <returns></returns>
        public bool CheckObsoletes()
        {
            return true;
        }

        /// <summary>
        /// Identify which drugs in the checklist are associated with the Known Allergy being checked
        /// </summary>
        /// <param name="classes">List of classifications</param>
        /// <param name="checklistDrugs">Dictionary of drug Dictionaries for drugs to be presented for ordering</param>
        /// <returns>A List of allergy information Dictionary objects</returns>
        public List<Dictionary<string, string>> GetAllergies(List<string> classes, Dictionary<string, Dictionary<string, string>> checklistDrugs)
        {
            List<Dictionary<string, string>> info = new List<Dictionary<string, string>>();
            var p = DB.GetParamsList(classes, SqlDbType.Int, "p");
            var drugCodes = new List<string>(checklistDrugs.Keys);    // Component drug codes
            var parentCodes = new List<string>();
            foreach(var c in checklistDrugs.Keys)
            {
                parentCodes.AddRange(checklistDrugs[c].Keys);
            }

            var sqlParams = new List<SqlParameter>(p.Item1);
            var drugChecklistClause = new List<string>();
            var drugP = DB.GetParamsList(drugCodes, SqlDbType.NChar, "dp");
            var parentP = DB.GetParamsList(parentCodes, SqlDbType.NChar, "pp");

            if (drugCodes.Count > 0)
            {
                drugChecklistClause.Add("alr_category_drug_map.drug_id IN (" + String.Join(",", drugP.Item2) + ")");
                sqlParams.AddRange(drugP.Item1);
            }
            if (parentCodes.Count > 0)
            {
                drugChecklistClause.Add("multum_combination_drug.drug_id IN (" + String.Join(",", parentP.Item2) + ")");
                sqlParams.AddRange(parentP.Item1);
            }

            var sql = @"SELECT DISTINCT
                          alr_category_class_map.class_id AS class,
                          multum_drug_id.drug_id AS drug,
                          multum_drug_name.drug_name AS name,
                          multum_combination_drug.drug_id AS parent_id
                        FROM vantagerx..multum_drug_id
                        LEFT JOIN vantagerx..multum_drug_name ON multum_drug_name.drug_synonym_id = multum_drug_id.drug_synonym_id
                        LEFT JOIN vantagerx..alr_category_drug_map ON alr_category_drug_map.drug_id = multum_drug_id.drug_id
                        LEFT JOIN vantagerx..multum_combination_drug ON multum_combination_drug.member_drug_id = multum_drug_id.drug_id
                        LEFT JOIN vantagerx..alr_category_class_map ON alr_category_class_map.alr_category_id = alr_category_drug_map.alr_category_id
                        WHERE alr_category_class_map.class_id IN (" + String.Join(",", p.Item2) + ")";
            if (drugChecklistClause.Count > 0)
            {
                sql += " AND (" + String.Join(" OR ", drugChecklistClause) + ")";
            }

            var DrugDone = new HashSet<string>();
            var ds = new DB.Select
            {
                Sql = sql,
                Parameters = sqlParams.ToArray(),
            }.RunForListOfDictionaries();
            foreach(var res in ds)
            {
                if (!DrugDone.Contains(res["drug"]))
                {
                    info.Add(new Dictionary<string, string>
                    {
                        { "class", res["class"] },
                        { "name", res["name"] },
                        { "drug", res["drug"] }
                    });
                    DrugDone.Add(res["drug"]);
                }
                if (!String.IsNullOrWhiteSpace(res["parent_id"]) && !DrugDone.Contains(res["parent_id"]))
                {
                    info.Add(new Dictionary<string, string>
                    {
                        { "class", res["class"] },
                        { "name", res["name"] },
                        { "drug", res["parent_id"] }
                    });
                    DrugDone.Add(res["parent_id"]);
                }
            }

            return info;
        }

        /// <summary>
        /// Get the Allergy Class based on a category
        /// </summary>
        /// <param name="category">Category identifier</param>
        /// <returns>List of Allergy class(es) associated with the category</returns>
        public List<string> GetAllergyClassByCategory(string category)
        {
            return new DB.Select
            {
                Sql = @"SELECT DISTINCT class_id
                        FROM vantagerx..alr_category_class_map
                        WHERE alr_category_class_map.alr_category_id=@category",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@category", SqlDbType.Int) { Value = category }
                }
            }.RunForListOfStrings("class_id");
        }

        /// <summary>
        /// Get the Allergy Class based on a dnum
        /// </summary>
        /// <param name="drugId">Drug identifier</param>
        /// <returns>List of Allergy class(es) associated with the drug id</returns>
        public List<string> GetAllergyClassByDrug(string drugId)
        {
            return new DB.Select
            {
                Sql = @"SELECT DISTINCT class_id
                        FROM vantagerx..alr_category_class_map
                          LEFT JOIN vantagerx..alr_category_drug_map ON alr_category_class_map.alr_category_id = alr_category_drug_map.alr_category_id
                        WHERE alr_category_drug_map.drug_id=@drug_id",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@drug_id", SqlDbType.NChar) { Value = drugId }
                }
            }.RunForListOfStrings("class_id");
        }

        /// <summary>
        /// Gather the orderable drug ids based on the components of the drug in the Known Allergy info that are 
        /// also in the ordering checklist
        /// </summary>
        /// <param name="algDrugs">List of alg drug ids</param>
        /// <param name="checklistDrugs">List of checklist drug ids</param>
        /// <returns>List of Dictionary objects that contain intolerance info</returns>
        public List<Dictionary<string, string>> GetAllergyIntolerances(List<string> algDrugs, List<string> checklistDrugs)
        {
            var algDrugsP = DB.GetParamsList(algDrugs, SqlDbType.NChar, "ap");
            var sqlParams = new List<SqlParameter>(algDrugsP.Item1);
            var checklistSel = "";
            if (checklistDrugs.Count > 0)
            {
                var checkListDrugsP = DB.GetParamsList(checklistDrugs, SqlDbType.NChar, "cp");
                checklistSel = " AND ndc_denorm.drug_id IN (" + String.Join(",", checkListDrugsP.Item2) + ")";
                sqlParams.AddRange(checkListDrugsP.Item1);
            }
            var sql = @"SELECT DISTINCT
                          CASE WHEN multum_combination_drug.member_drug_id IS NULL THEN ndc_denorm.drug_id ELSE multum_combination_drug.member_drug_id END AS cdrug,
                          ndc_denorm.brand_description AS brand,
                          ndc_denorm.drug_id AS drug
                        FROM vantagerx..ndc_denorm
                        LEFT JOIN vantagerx..multum_combination_drug ON multum_combination_drug.drug_id = ndc_denorm.drug_id
                        WHERE
		                      ( multum_combination_drug.member_drug_id IN (" + String.Join(",", algDrugsP.Item2) + ") OR ndc_denorm.drug_id IN (" + String.Join(",", algDrugsP.Item2) + ") )" + checklistSel;

            return new DB.Select
            {
                Sql = sql,
                Parameters = sqlParams.ToArray()
            }.RunForListOfDictionaries();                      
        }

        /// <summary>
        /// Gather component drug information for the entered drug ids
        /// </summary>
        /// <param name="components">List of drug IDs</param>
        /// <returns>A List of Dictionary objects with component information associated with the list of drugs</returns>
        public List<Dictionary<string, string>> GetComponentInfo(List<string> components)
        {
            return DB.ConvertDataSetToListOfDictionaries(new DB.Select
            {
                Sql = "[dbo].pc_vantagerx_get_component_info",
                IsStoredProcedure = true,
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@drugs", SqlDbType.VarChar) { Value = string.Join(",", components) }
                }
            }.RunForDataSet());
        }

        /// <summary>
        /// Get the codes and descriptions for drugs by formulation ID, for finding equivalent medications.
        /// </summary>
        /// <param name="formulationId">Formulation ID</param>
        /// <returns>List of Dictionaries of information about formulation ID equivalents</returns>
        public List<Dictionary<string, string>> GetDrugInfoByFormulationId(string formulationId)
        {
            var whereParam = "main_multum_drug_code=@mmdc AND ni.ndc=ni.base_ndc";
            return GetDrugInfoWhere(whereParam, new List<SqlParameter> { new SqlParameter("@mmdc", SqlDbType.VarChar) { Value = formulationId } });
        }

        /// <summary>
        /// Get a list of drugs based off a brand
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="brand">"brand" name being searched (generic names count here)</param>
        /// <param name="type">'M'edication or 'P'rescription</param>
        /// <returns>A dictionary of information about the drugs</returns>
        public List<Dictionary<string, string>> GetDrugInfoByBrand(byte siteId, string brand, string type = "M")
        {
            var org = new DB.Select
            {
                Sql = "SELECT medinpat, medoutpat, medpyxis, medexactmatch, rxinpat, rxoutpat, rxpyxis, rxexactmatch FROM org WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                }
            }.RunForDataRow();

            var whereParam = "brand_description=@brand";
            var whereParamVals = new List<SqlParameter>
            {
                new SqlParameter("@brand", SqlDbType.VarChar) { Value = brand },
            };

            var prefix = type == "M" ? "med" : "rx";
            var exactMatch = org[prefix + "exactmatch"].ToString().Equals("Y");

            var filters = new[] { "inpat", "outpat", "pyxis" }
                .Where(x => org[prefix + x].ToString().Equals("Y"))
                .Select(x => "frm." + x + "='Y'");
            if (filters.Any())
            {
                whereParam += " and ndc_denorm.ndc_code in (";
                if (exactMatch)
                {
                    whereParam += "select nd.ndc_code from frm left join vantagerx..ndc_denorm nd on nd.ndc_code=frm.ndc";
                }
                else
                {
                    whereParam += "select distinct ni.base_ndc from frm left join vantagerx..ndc_denorm on nd.drug_id=frm.dnum " +
                        "inner join vantagerx_ndc_info ni on ni.ndc=nd.ndc_code";
                }
                whereParam += " where nd.brand_description=@brand_description and site=@site";
                whereParamVals.AddRange(new[] {
                    new SqlParameter("@brand_description", SqlDbType.VarChar) { Value = brand },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                });

                whereParam += " and (" + string.Join(" OR ", filters) + ")";
            }
            else
            {
                whereParam += " AND ni.ndc = ni.base_ndc";
            }

            if (type == "P")
            {
                whereParam += " and days_obsolete=0";
            }
            
            return GetDrugInfoWhere(whereParam, whereParamVals);
        }

        /// <summary>
        /// Get the code and description for a drug by NDC
        /// </summary>
        /// <param name="ndc">Drug NDC</param>
        /// <returns>Dictionary of information in the basic denorm table mapping</returns>
        public Dictionary<string, string> GetDrugInfoByNDC(string ndc)
        {
            return GetDrugInfoByNDCs(new List<string> { ndc })[0];
        }

        /// <summary>
        /// Get the codes and descriptions for drugs by NDC
        /// </summary>
        /// <param name="ndcs">List of drug NDCs</param>
        /// <returns>List of Dictionaries of information in the basic denorm table mapping</returns>
        public List<Dictionary<string, string>> GetDrugInfoByNDCs(List<string> ndcs)
        {
            var p = DB.GetParamsList(ndcs, SqlDbType.VarChar);
            var whereParam = "ndc_code IN (" + string.Join(",", p.Item2) + ")";
            return GetDrugInfoWhere(whereParam, p.Item1);
        }

        /// <summary>
        /// Gather drug interaction information
        /// </summary>
        /// <param name="components">List of drug IDs</param>
        /// <returns>A List of Dictionary objects with interaction information associated with the list of drugs</returns>
        public List<Dictionary<string, string>> GetDrugInteractions(List<string> components)
        {
            var ret = new List<Dictionary<string, string>>();
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                var res = new DB.Select
                {
                    Connection = con,
                    Sql = "[dbo].pc_vantagerx_get_drug_interactions",
                    IsStoredProcedure = true,
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@drugs", SqlDbType.VarChar) { Value = string.Join(",", components) }
                    }
                }.RunForDataReader();
                while (res.Read())
                {
                    ret.Add(Enumerable.Range(0, res.FieldCount).ToDictionary(
                         i => res.GetName(i),
                         i => res.GetValue(i)?.ToString().Trim()
                    ));
                }
                res.Close();
                con.Close();
            }

            return ret;
        }

        private List<Dictionary<string, string>> GetDrugInfoWhere(string whereParam, List<SqlParameter> paramValues)
        {
            if (string.IsNullOrWhiteSpace(whereParam) || paramValues.Count == 0)
            {
                return new List<Dictionary<string, string>>();
            }
            
            var sql = string.Format(@"
                SELECT
		          rxn_map.rxcui,
			      rxn_map.tty,
			
                  ndc_code AS ndc,

                  bi.long_brand,
                  main_multum_drug_code AS multum,

                  brand_description AS brand,
                  ndc_denorm.drug_id AS drug,

                  brand_description AS med_name,
                  ndc_denorm.drug_id AS med_drug_id,

                  brand_description AS alg_name,
                  ndc_denorm.drug_id AS alg_drug_id,

                  active_ingredient AS active,

                  ni.base_ndc,
                  ni.packaging,
                  ni.strength,
                  dose_form_description AS dose_form,
                  route_description AS route,

                  csa_schedule AS dea_schedule,
                  bi.rx_otc,
                  ni.days_obsolete,
                  case
                    when ni.days_obsolete > 0 then 'Y'
                    else 'N'
                  end as obsolete,
                  gbo as generic,
			
                  bi.drugcat,
                  bi.druggroup
				
                FROM vantagerx..ndc_denorm
                LEFT JOIN ibex..vantagerx_ndc_info AS ni ON ni.ndc = ndc_denorm.ndc_code
                LEFT JOIN ibex..vantagerx_brand_info AS bi ON bi.base_ndc = ni.base_ndc
                LEFT JOIN vantagerx..rxn_ndc_rxcui ON rxn_ndc_rxcui.ndc = ndc_denorm.ndc_code
                LEFT JOIN vantagerx..rxn_map ON rxn_map.rxcui = rxn_ndc_rxcui.rxcui
                WHERE {0}
                ORDER BY brand_description,dose_form_description,product_strength_description,days_obsolete
            ", whereParam);

            var ret = new List<Dictionary<string, string>>();
            var foundDrugs = new Dictionary<string, bool>();
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                var res = new DB.Select
                {
                    Connection = con,
                    Sql = sql,
                    Parameters = paramValues.ToArray()
                }.RunForDataReader();

                while (res.Read())
                {
                    var ndc = res["ndc"].ToString();
                    if (!foundDrugs.ContainsKey(ndc))
                    {
                        foundDrugs.Add(ndc, true);
                        ret.Add(Enumerable.Range(0, res.FieldCount).ToDictionary(
                             i => res.GetName(i),
                             i => res.GetValue(i)?.ToString().Trim()
                        ));
                    }
                }
                res.Close();
                con.Close();
            }

            return ret;
        }

        /// <summary>
        /// Get information needed for the quick list
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="top">Top quantity used for getting data for most used tab</param>
        /// <param name="formularyMatchClause">A formulary table clause</param>
        /// <param name="categoryClause">A category clause</param>
        /// <param name="clauseParameters">A list of SqlParameters used by the clauses</param>
        /// <param name="type">(M)edication or (P)rescription</param>
        /// <param name="sqlFormulary">Empty, or id of drug category to limit output</param>
        /// <param name="userId">The user whose quick list we're getting</param>
        /// <returns>List of Dictionary objects with quick list data</returns>
        public List<Dictionary<string, string>> GetFilteredQuickListData(byte siteId, string top, string formularyMatchClause, string categoryClause, List<SqlParameter> clauseParameters, string type, string sqlFormulary, int userId)
        {
            List<SqlParameter> parameters = new List<SqlParameter>(clauseParameters);
            parameters.Add(new SqlParameter("@user", SqlDbType.Int) { Value = userId });
            parameters.Add(new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId });
            parameters.Add(new SqlParameter("@type", SqlDbType.Char) { Value = type });

            var sql = string.Format(
                @"SELECT distinct {0} rxl.*, csa_schedule AS dea_schedule, md.gbo as generic, case when otc_status = 'T' then 'O' else 'R' end as rx_otc
                  FROM {1} rxl LEFT JOIN vantagerx..ndc_denorm AS md ON md.ndc_code = rxl.ndc
                  WHERE rxl.usr = @user {2} AND rxl.ndc IS NOT NULL AND rxl.site = @site AND rxl.type = @type {3}", 
                top, formularyMatchClause, categoryClause, sqlFormulary
            );
            if (!string.IsNullOrWhiteSpace(top))
            {
                sql += " AND rxl.usage > 0";
            }
            sql += " ORDER BY rxl.usage DESC";

            var RXL_INFO = new DB.Select
            {
                Sql = sql,
                Parameters = parameters.ToArray()
            }.RunForListOfDictionaries();

            // Grab the free text meds if type is 'M' and limit is empty so free text meds
            // are always listed in the Med SVC quick list even when restrictions are set.
            if (type.Equals("M") && string.IsNullOrWhiteSpace(top) && !string.IsNullOrWhiteSpace(sqlFormulary))
            {
                RXL_INFO.AddRange(
                    new DB.Select
                    {
                        Sql = "SELECT * FROM rxl WHERE site=@site AND usr=@user AND type=@type AND ndc=@ndc",
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                            new SqlParameter("@user", SqlDbType.Int) { Value = userId },
                            new SqlParameter("@type", SqlDbType.Char) { Value = type },
                            new SqlParameter("@ndc", SqlDbType.VarChar) { Value = "ft" }
                        }
                    }.RunForListOfDictionaries()
                );
            }

            return RXL_INFO;
        }

        /// <summary>
        /// Get formulary match information for sets of drug identifiers
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="ndcs">List of drug NDCs</param>
        /// <param name="form_ids">List of formulation IDs</param>
        /// <param name="drug_ids">List of drug IDs</param>
        /// <returns>List of Dictionary objects containing formulary match information</returns>
        public List<Dictionary<string, string>> GetFormularyMatchInfo(byte siteId, List<string> ndcs, List<string> form_ids, List<string> drug_ids)
        {
            var ndcParams = DB.GetParamsList(ndcs, SqlDbType.NVarChar, "n");
            var multumParams = DB.GetParamsList(form_ids, SqlDbType.NVarChar, "m");
            var dnumParams = DB.GetParamsList(drug_ids, SqlDbType.NVarChar, "d");
            var sqlParams = new List<SqlParameter>(ndcParams.Item1);

            var sql = @"SELECT
                ibex..frm.*,
                (SELECT base_ndc FROM ibex..vantagerx_ndc_info WHERE ndc = frm.ndc) AS base_ndc
                FROM frm
                WHERE site=@site AND (ndc IN (" + String.Join(",", ndcParams.Item2) + ")";
            if (form_ids.Count > 0)
            {
                sqlParams.AddRange(multumParams.Item1);
                sql += " OR multum IN (" + String.Join(",", multumParams.Item2) + ")";
            }
            if (drug_ids.Count > 0)
            {
                sqlParams.AddRange(dnumParams.Item1);
                sql += " OR dnum IN (" + String.Join(",", dnumParams.Item2) + ")";
            }
            sql += ")";

            return new DB.Select
            {
                Sql = sql,
                Parameters = sqlParams.ToArray()
            }.RunForListOfDictionaries();
        }

        /// <summary>
        /// Determine if there are warnings and effects for a drug list
        /// </summary>
        /// <param name="ids">A list of drugs to get the warnings/effects for</param>
        /// <returns>Dictionary of drug_id => 1 for drugs that have warnings/effects</returns>
        public Dictionary<string, string> HasWarningsAndEffects(List<string> ids)
        {
            var sqlParameters = new List<SqlParameter>();
            var sql = "select distinct drug_id from vantagerx..xp_clinical_text_xref";
            if (ids.Count > 0)
            {
                var p = DB.GetParamsList(ids, SqlDbType.NChar, "p");
                sql += " where drug_id in (" + String.Join(",", p.Item2) + ")";
                sqlParameters.AddRange(p.Item1);
            }
            var we = new Dictionary<string, string>();
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                var res = new DB.Select
                {
                    Connection = con,
                    Sql = sql,
                    Parameters = sqlParameters.ToArray()
                }.RunForDataReader();
                while (res.Read())
                {
                    var id = res["drug_id"].ToString().Trim();
                    if (we.ContainsKey(id))
                        continue;

                    we.Add(id, "Y");
                }
                res.Close();
                con.Close();
            }

            return we;
        }
    }
}