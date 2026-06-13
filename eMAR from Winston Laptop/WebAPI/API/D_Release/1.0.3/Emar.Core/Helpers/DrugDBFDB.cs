using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to handle interaction with the FDB drug database
    /// </summary>
    public class DrugDBFDB : IDrugDBUtility
    {
        public string Vendor = DrugDB.Constants.Vendors.FDB;
        public string Name = "FDB";
        public string DBName = "fdb";
        public string DBType = "F";

        /// <summary>
        /// Get the name of the Drug database vendor
        /// </summary>
        /// <returns>FDB (name of the vendor)</returns>
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
        /// Gather component drug information for the entered drug ids
        /// </summary>
        /// <param name="codes">List of drug IDs</param>
        /// <returns>A List of Dictionary objects with component information associated with the list of drugs</returns>
        public List<Dictionary<string, string>> GetComponentInfo(List<string> codes)
        {
            var codeInfo = new Dictionary<string, Dictionary<string, string>>
            {
                { "G", new Dictionary<string, string> {
                        { "field", "fdb_allergy_name.MED_NAME_ID" },
                        { "drug_id", "fdb_allergy_name.PC_MED_NAME_ID" }
                    }
                },
                { "R", new Dictionary<string, string> {
                        { "field", "RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.ROUTED_GEN_ID" },
                        { "drug_id", "'R' + RIGHT('0000000' + cast(RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.ROUTED_GEN_ID AS varchar), 8)" }
                    }
                },
                { "L", new Dictionary<string, string> {
                        { "field", "RHICL1_HIC_HICLSEQNO_LINK.HICL_SEQNO" },
                        { "drug_id", "'L' + RIGHT('00000' + cast(RHICL1_HIC_HICLSEQNO_LINK.HICL_SEQNO AS varchar), 6)" }
                    }
                },
                { "",  new Dictionary<string, string> {
                        { "field", "RHICL1_HIC_HICLSEQNO_LINK.HIC_SEQN" },
                        { "drug_id", "RIGHT('00000' + cast(RHICL1_HIC_HICLSEQNO_LINK.HIC_SEQN AS varchar), 6)" },
                        { "alt_field", "RHICD5_HIC_DESC.HIC_ROOT" }
                    }
                }
            };

            var fieldPrecision = new Dictionary<string, int>
            {
                { "G", 8 },
                { "R", 8 },
                { "L", 6 },
                { "",  6 }
            };

            var codeJoins = new Dictionary<string, List<string>>
            {
                { "G", new List<string> {
                        "ibex..fdb_allergy_name ON fdb_allergy_name.HICL_SEQNO = RHICL1_HIC_HICLSEQNO_LINK.HICL_SEQNO"
                    }
                },
                { "R", new List<string> {
                        "fdb..RGCNSEQ4_GCNSEQNO_MSTR on RHICL1_HIC_HICLSEQNO_LINK.HICL_SEQNO = RGCNSEQ4_GCNSEQNO_MSTR.HICL_SEQNO",
                        "fdb..RRTGNGC0_RTD_GEN_GCNSEQNO_LNK on RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.GCN_SEQNO = RGCNSEQ4_GCNSEQNO_MSTR.GCN_SEQNO"
                    }
                }
            };

            var codeLists = new Dictionary<string, List<string>>
            {
                { "G", new List<string>() },
                { "R", new List<string>() },
                { "L", new List<string>() },
                { "",  new List<string>() }
            };

            var info = new List<Dictionary<string, string>>();
            var joins = new List<string>
            {
                "fdb..RHICL1_HIC_HICLSEQNO_LINK",
                "fdb..RHICD5_HIC_DESC ON RHICD5_HIC_DESC.HIC_SEQN = RHICL1_HIC_HICLSEQNO_LINK.HIC_SEQN"
            };

            foreach (var code in codes)
            {
                var key = code.Substring(0, 1);
                if (key.Equals("R") || key.Equals("L") || key.Equals("G"))
                {
                    codeLists[key].Add(code.Substring(1));
                }
                else
                {
                    codeLists[""].Add(code);
                }
            }

            foreach (var key in codeInfo.Keys)
            {
                var field = codeInfo[key]["field"];
                var drugId = codeInfo[key]["drug_id"];
                var list = codeLists[key];
                var altField = codeInfo[key].ContainsKey("alt_field") ? codeInfo[key]["alt_field"] : "";
                if (!string.IsNullOrWhiteSpace(altField))
                {
                    field = "(" + field;
                }

                if (list.Count == 0)
                {
                    continue;
                }

                var listParams = DB.GetParamsList(list, SqlDbType.Decimal, fieldPrecision[key], "f");
                var sqlParameters = new List<SqlParameter>(listParams.Item1);
                var keyJoins = new List<string>();
                keyJoins.AddRange(joins);
                if (codeJoins.ContainsKey(key))
                {
                    keyJoins.AddRange(codeJoins[key]);
                }

                var paramsString = string.Join(",", listParams.Item2);
                var sql = string.Format(@"
                    SELECT DISTINCT {0} AS drug,
                        RHICD5_HIC_DESC.HIC_DESC AS name,
                        RIGHT('00000' + cast(RHICD5_HIC_DESC.HIC_SEQN as varchar), 6) AS cdrug,
                        'N' AS croot,
                        RIGHT('00000' + cast(RHICD5_HIC_DESC.HIC_ROOT as varchar), 6) AS c_root
                        FROM {1}
                        WHERE RHICD5_HIC_DESC.HIC_ROOT != 9870
                        AND {2} IN ({3})",
                        drugId,
                        string.Join(" LEFT JOIN ", keyJoins),
                        field,
                        paramsString
                );
                if (!string.IsNullOrWhiteSpace(altField))
                {
                    sql += string.Format(
                        " OR {0} IN ({1}))",
                        altField,
                        paramsString
                    );
                }

                var ds = new DB.Select
                {
                    Sql = sql,
                    Parameters = sqlParameters.ToArray()
                }.RunForDataSet();

                if (ds != null)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var dict = DB.ConvertDataRowToDictionary(dr);
                        info.Add(dict);
                        if (!dict["cdrug"].Equals(dict["c_root"]))
                        {
                            var croot = new Dictionary<string, string>(dict);
                            croot["cdrug"] = dict["c_root"];
                            croot["croot"] = "Y";
                            info.Add(croot);
                        }
                    }
                }
            }

            return info;
        }

        /// <summary>
        /// Gather component drug ids (HIC) for the entered drug id
        /// </summary>
        /// <param name="code">A single drug id</param>
        /// <returns>A list of the components associated with the drug</returns>
        public List<string> GetComponents(string code)
        {
            return GetComponents(new List<string> { code });
        }

        /// <summary>
        /// Gather component drug ids (HIC) for the entered drug ids
        /// </summary>
        /// <param name="code">A list of drug ids</param>
        /// <returns>A list of the components associated with the drugs</returns>
        public List<string> GetComponents(List<string> codes)
        {
            var codeInfo = new Dictionary<string, string>
            {
                { "G", "RMIRMID1_ROUTED_MED.MED_NAME_ID" },
                { "R", "RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.ROUTED_GEN_ID" },
                { "L", "RGCNSEQ4_GCNSEQNO_MSTR.HICL_SEQNO" }
            };

            var codeValues = new Dictionary<string, List<string>>
            {
                { "G", new List<string>() },
                { "R", new List<string>() },
                { "L", new List<string>() }
            };

            var parameterPrecision = new Dictionary<string, int>
            {
                { "G", 8 },
                { "R", 8 },
                { "L", 6 }
            };

            var info = new Dictionary<string, int>();

            int codeParse;
            foreach (var code in codes)
            {
                var key = code.Substring(0, 1);
                if (codeValues.ContainsKey(key))
                {
                    codeValues[key].Add(code.Substring(1));
                }
                else if (Int32.TryParse(code, out codeParse))
                {
                    info[code] = 1;
                }
            }

            foreach (var key in codeInfo.Keys)
            {
                var field = codeInfo[key];
                var list = codeValues[key];
                if (list.Count == 0)
                {
                    continue;
                }

                var paramList = DB.GetParamsList(list, SqlDbType.Decimal, parameterPrecision[key]);

                var sql = string.Format(@"SELECT DISTINCT
                    RIGHT('00000' + cast(RHICL1_HIC_HICLSEQNO_LINK.HIC_SEQN as varchar), 6) AS HIC_SEQN,
                    RIGHT('00000' + cast(RHICD5_HIC_DESC.HIC_ROOT as varchar), 6) AS HIC_ROOT
                  FROM fdb..RMIID1_MED
                  LEFT JOIN fdb..RRTGNGC0_RTD_GEN_GCNSEQNO_LNK ON RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.GCN_SEQNO = RMIID1_MED.GCN_SEQNO
                  LEFT JOIN fdb..RMIDFID1_ROUTED_DOSE_FORM_MED ON RMIDFID1_ROUTED_DOSE_FORM_MED.ROUTED_DOSAGE_FORM_MED_ID = RMIID1_MED.ROUTED_DOSAGE_FORM_MED_ID
                  LEFT JOIN fdb..RMIRMID1_ROUTED_MED ON RMIRMID1_ROUTED_MED.ROUTED_MED_ID = RMIDFID1_ROUTED_DOSE_FORM_MED.ROUTED_MED_ID
                  LEFT JOIN fdb..RGCNSEQ4_GCNSEQNO_MSTR ON RGCNSEQ4_GCNSEQNO_MSTR.GCN_SEQNO = RMIID1_MED.GCN_SEQNO
                  LEFT JOIN fdb..RHICL1_HIC_HICLSEQNO_LINK ON RHICL1_HIC_HICLSEQNO_LINK.HICL_SEQNO = RGCNSEQ4_GCNSEQNO_MSTR.HICL_SEQNO
                  LEFT JOIN fdb..RHICD5_HIC_DESC ON RHICD5_HIC_DESC.HIC_SEQN = RHICL1_HIC_HICLSEQNO_LINK.HIC_SEQN
                  WHERE RHICD5_HIC_DESC.HIC_ROOT != 9870
                    AND {0} IN ({1})", field, string.Join(",", paramList.Item2));

                var ds = new DB.Select
                {
                    Sql = sql,
                    Parameters = paramList.Item1.ToArray()
                }.RunForDataSet();
                if (ds != null)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        info[dr["HIC_SEQN"]?.ToString()] = 1;
                        info[dr["HIC_ROOT"]?.ToString()] = 1;
                    }
                }
            }

            return info.Keys.ToList();
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
            var whereParam = "RMINDC1_NDC_MEDID.NDC IN (" + string.Join(",", p.Item2) + ")";
            return GetDrugInfoWhere(whereParam, p.Item1);
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
			
              RMINDC1_NDC_MEDID.NDC AS ndc,
              RMIID1_MED.MEDID,

              fdb_brand_name.long_brand_name as long_brand,
              RMIID1_MED.GCN_SEQNO AS multum,

              fdb_brand_name.brand_name as brand,
              fdb_brand_name.PC_ROUTED_GEN_ID as drug,

              fdb_allergy_name.med_name,
              fdb_brand_name.PC_MED_NAME_ID med_drug_id,

              fdb_allergy_name.allergy_name as alg_name,
              fdb_allergy_name.PC_HICL_SEQNO as alg_drug_id,

              RHICD5_HIC_DESC.HIC_DESC as active,

              fdb_ndc_info.base_ndc,
              fdb_ndc_info.packaging,
              fdb_ndc_info.strength,
              RMIDFD1_DOSE_FORM.MED_DOSAGE_FORM_DESC AS dose_form,
              RROUTED3_ROUTE_DESC.GCRT_DESC AS route,

              fdb_brand_name.dea_schedule,
              fdb_brand_name.rx_otc,
              case
                when RNDC14_NDC_MSTR.GNI = 1 then 'G'
                when RNDC14_NDC_MSTR.GNI = 2 then 'N'
                else ''
              end as generic,
              days_obsolete,
              case
                when days_obsolete > 0 then 'Y'
                else 'N'
              end as obsolete,
              RETCNDC0_ETC_NDC.ETC_ID AS drugcat,
			        RETCTBL0_ETC_ID.ETC_ULTIMATE_PARENT_ETC_ID AS druggroup

            FROM
              fdb..RMINDC1_NDC_MEDID
              LEFT JOIN fdb..RNDC14_NDC_MSTR ON RNDC14_NDC_MSTR.NDC = RMINDC1_NDC_MEDID.NDC
              LEFT JOIN fdb..RETCNDC0_ETC_NDC ON RETCNDC0_ETC_NDC.NDC = RMINDC1_NDC_MEDID.NDC
              LEFT JOIN fdb..RMIID1_MED ON RMIID1_MED.MEDID = RMINDC1_NDC_MEDID.MEDID
			
			  LEFT JOIN fdb..RETCTBL0_ETC_ID ON RETCTBL0_ETC_ID.ETC_ID = RETCNDC0_ETC_NDC.ETC_ID
              LEFT JOIN fdb..RGCNSEQ4_GCNSEQNO_MSTR ON RGCNSEQ4_GCNSEQNO_MSTR.GCN_SEQNO = RMIID1_MED.GCN_SEQNO

              LEFT JOIN fdb..RMIDFID1_ROUTED_DOSE_FORM_MED ON RMIDFID1_ROUTED_DOSE_FORM_MED.ROUTED_DOSAGE_FORM_MED_ID = RMIID1_MED.ROUTED_DOSAGE_FORM_MED_ID
              LEFT JOIN fdb..RMIDFD1_DOSE_FORM ON RMIDFD1_DOSE_FORM.MED_DOSAGE_FORM_ID = RMIDFID1_ROUTED_DOSE_FORM_MED.MED_DOSAGE_FORM_ID
              LEFT JOIN fdb..RROUTED3_ROUTE_DESC ON RGCNSEQ4_GCNSEQNO_MSTR.GCRT = RROUTED3_ROUTE_DESC.GCRT

              LEFT JOIN fdb..RHICL1_HIC_HICLSEQNO_LINK ON RHICL1_HIC_HICLSEQNO_LINK.HICL_SEQNO = RGCNSEQ4_GCNSEQNO_MSTR.HICL_SEQNO
              LEFT JOIN fdb..RHICD5_HIC_DESC ON RHICD5_HIC_DESC.HIC_SEQN = RHICL1_HIC_HICLSEQNO_LINK.HIC_SEQN

              LEFT JOIN ibex..fdb_brand_name ON fdb_brand_name.MEDID = RMIID1_MED.MEDID
              LEFT JOIN ibex..fdb_allergy_name ON fdb_allergy_name.MEDID = RMIID1_MED.MEDID
              LEFT JOIN ibex..fdb_ndc_info ON fdb_ndc_info.ndc = RMINDC1_NDC_MEDID.NDC

			  LEFT JOIN fdb..rxn_ndc_rxcui ON rxn_ndc_rxcui.ndc = RMINDC1_NDC_MEDID.NDC
			  LEFT JOIN fdb..rxn_map ON rxn_map.rxcui = rxn_ndc_rxcui.rxcui

		        WHERE
              RMIID1_MED.MED_STATUS_CD IN (0,3)
              AND RETCNDC0_ETC_NDC.ETC_COMMON_USE_IND = 1
              AND {0}
              ORDER BY fdb_brand_name.brand_name,RMIDFD1_DOSE_FORM.MED_DOSAGE_FORM_DESC,RGCNSEQ4_GCNSEQNO_MSTR.STR60,RHICL1_HIC_HICLSEQNO_LINK.HIC_REL_NO
              ", whereParam);

            var denormList = new List<Dictionary<string, string>>();
            var ndcDenorm = new Dictionary<string, Dictionary<string, string>>();
            var usedActives = new Dictionary<string, Dictionary<string, int>>();

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
                    var dr = Enumerable.Range(0, res.FieldCount).ToDictionary(
                         i => res.GetName(i),
                         i => res.GetValue(i)?.ToString().Trim()
                    );

                    // If we don't have the hic_desc, it's an obsolete drug and shouldn't display
                    if (!dr.ContainsKey("active"))
                        continue;

                    var ndc = dr["ndc"];
                    var active = dr["active"];
                    if (ndcDenorm.ContainsKey(ndc))
                    {
                        if (usedActives.ContainsKey(ndc) && usedActives[ndc].ContainsKey(active))
                            continue;

                        // Build the active name list
                        ndcDenorm[ndc]["active"] += "/" + active;
                    }
                    else
                    {
                        ndcDenorm[ndc] = dr;
                        denormList.Add(dr);
                    }

                    if (!usedActives.ContainsKey(ndc))
                    {
                        usedActives[ndc] = new Dictionary<string, int>
                        {
                            { active, 1 }
                        };
                    }
                    else
                    {
                        usedActives[ndc][active] = 1;
                    }
                }
                res.Close();
                con.Close();
            }

            return denormList;
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
                (SELECT base_ndc FROM ibex..fdb_ndc_info WHERE ndc = frm.ndc) AS base_ndc
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

        public Dictionary<string, string> GetCategoryInfoById(int subcatId)
        {
            var info = new DB.Select
            {
                Sql = @"SELECT DISTINCT
                    ei1.ETC_ULTIMATE_PARENT_ETC_ID AS cat,
                    ei2.ETC_NAME AS name,
                    ei1.ETC_ID as sub_cat,
                    ei1.ETC_NAME as sub_cat_name
                FROM fdb..RETCNDC0_ETC_NDC en
                LEFT JOIN fdb..RETCTBL0_ETC_ID ei1 ON ei1.ETC_ID = en.ETC_ID
                INNER JOIN fdb..RETCTBL0_ETC_ID ei2 ON ei2.ETC_ID = ei1.ETC_ULTIMATE_PARENT_ETC_ID
                LEFT JOIN ibex..fdb_ndc_info fni ON fni.ndc = en.NDC
                LEFT JOIN ibex..fdb_brand_name fbn ON fbn.MEDID = fni.MEDID
                WHERE fni.ndc IS NOT NULL AND en.ETC_COMMON_USE_IND = 1 and ei1.ETC_ID = @subcatid",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@subcatid", SqlDbType.Int) { Value = subcatId}
                }
            }.RunForDataRow();
            if (info == null)
                return null;

            var dict = DB.ConvertDataRowToDictionary(info);

            return dict;
        }

        public string GetRxcuiByDrugId(string drugId)
        {
            var info = new DB.Select
            {
                Sql = "SELECT rxcui FROM fdb..rxn_map WHERE drug_id = @drugid",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@drugid", SqlDbType.VarChar) { Value = drugId}
                }
            }.RunForDataRow();
            if (info == null)
                return "";

            return info["rxcui"]?.ToString().Trim();
        }
    }
}
