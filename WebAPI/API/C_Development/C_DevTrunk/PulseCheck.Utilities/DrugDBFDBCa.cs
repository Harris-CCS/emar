using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using PulseCheck.IUtilities;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle interaction with the FDB Canada drug database
    /// </summary>
    public class DrugDBFDBCa : IDrugDBUtility
    {
        public string Vendor = DrugDB.Constants.Vendors.FDB_CANADIAN;
        public string Name = "Canadian FDB";
        public string DBName = "fdb_ca";
        public string DBType = "1";

        /// <summary>
        /// Get the name of the Drug database vendor
        /// </summary>
        /// <returns>Canadian FDB (name of the vendor)</returns>
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
            var classListParams = DB.GetParamsList(classes, SqlDbType.Decimal, 4, "p");
            var hicCodes = checklistDrugs.Keys.ToList();    // Component drug codes
            var genCodes = new List<string>();
            foreach (var hic in hicCodes)
            {
                if (checklistDrugs.ContainsKey(hic))
                {
                    genCodes.AddRange(checklistDrugs[hic].Keys.ToList());   // Parent codes associated with the component
                }
            }

            var sqlParameters = new List<SqlParameter>(classListParams.Item1);
            var hicCodeParams = DB.GetParamsList(hicCodes, SqlDbType.Decimal, 6, "d");
            var genCodeParams = DB.GetParamsList(genCodes, SqlDbType.VarChar, "g");

            var drugChecklist = "";
            if (hicCodes.Count > 0)
            {
                drugChecklist += string.Format(@" AND RDAMXHC0_HIC_ALRGN_XSENSE_LINK.HIC_SEQN IN ({0})", string.Join(",", hicCodeParams.Item2));
                sqlParameters.AddRange(hicCodeParams.Item1);
            }
            if (genCodes.Count > 0)
            {
                drugChecklist += string.Format(@" AND fdb_ca_brand_name.PC_ROUTED_GEN_ID IN ({0})", string.Join(",", genCodeParams.Item2));
                sqlParameters.AddRange(genCodeParams.Item1);
            }

            var sql = string.Format(
                @"SELECT DISTINCT
                  RDAMXHC0_HIC_ALRGN_XSENSE_LINK.DAM_ALRGN_XSENSE AS class,
                  RIGHT('00000' + cast(RDAMXHC0_HIC_ALRGN_XSENSE_LINK.HIC_SEQN as varchar), 6) AS drug,
                  RHICD5_HIC_DESC.HIC_DESC AS name,
                  fdb_ca_brand_name.PC_ROUTED_GEN_ID
                FROM fdb_ca..RDAMXHC0_HIC_ALRGN_XSENSE_LINK
                LEFT JOIN fdb_ca..RHICD5_HIC_DESC ON RHICD5_HIC_DESC.HIC_SEQN = RDAMXHC0_HIC_ALRGN_XSENSE_LINK.HIC_SEQN
                LEFT JOIN fdb_ca..RHICL1_HIC_HICLSEQNO_LINK ON RHICL1_HIC_HICLSEQNO_LINK.HIC_SEQN = RDAMXHC0_HIC_ALRGN_XSENSE_LINK.HIC_SEQN
                LEFT JOIN fdb_ca..RGCNSEQ4_GCNSEQNO_MSTR ON RGCNSEQ4_GCNSEQNO_MSTR.HICL_SEQNO = RHICL1_HIC_HICLSEQNO_LINK.HICL_SEQNO
                LEFT JOIN fdb_ca..RMIID2_MED ON RMIID2_MED.GCN_SEQNO = RGCNSEQ4_GCNSEQNO_MSTR.GCN_SEQNO
                LEFT JOIN ibex..fdb_ca_brand_name ON fdb_ca_brand_name.MEDID = RMIID2_MED.MEDID
                WHERE RDAMXHC0_HIC_ALRGN_XSENSE_LINK.DAM_ALRGN_XSENSE IN ({0}){1}",
                string.Join(",", classListParams.Item2),
                drugChecklist
            );

            var info = new List<Dictionary<string, string>>();
            var DrugDone = new HashSet<string>();
            var ds = new DB.Select
            {
                Sql = sql,
                Parameters = sqlParameters.ToArray(),
            }.RunForListOfDictionaries();
            foreach (var res in ds)
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

                if (!String.IsNullOrWhiteSpace(res["PC_ROUTED_GEN_ID"]) && !DrugDone.Contains(res["PC_ROUTED_GEN_ID"]))
                {
                    info.Add(new Dictionary<string, string>
                    {
                        { "class", res["class"] },
                        { "name", res["name"] },
                        { "drug", res["PC_ROUTED_GEN_ID"] }
                    });
                    DrugDone.Add(res["PC_ROUTED_GEN_ID"]);
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
                Sql = @"SELECT RDAMGX0_ALRGN_GRP_XSENSE_LINK.DAM_ALRGN_XSENSE
                        FROM fdb_ca..RDAMGX0_ALRGN_GRP_XSENSE_LINK
                        WHERE RDAMGX0_ALRGN_GRP_XSENSE_LINK.DAM_ALRGN_GRP=@category",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@category", SqlDbType.Decimal, 6) { Value = category }
                }
            }.RunForListOfStrings("DAM_ALRGN_XSENSE");
        }

        /// <summary>
        /// Get the Allergy Class based on a dnum
        /// </summary>
        /// <param name="drugId">Drug identifier</param>
        /// <returns>List of Allergy class(es) associated with the drug id</returns>
        public List<string> GetAllergyClassByDrug(string drugId)
        {
            var components = GetComponents(drugId);
            var componentParams = DB.GetParamsList(components, SqlDbType.Decimal, 6);
            var sql = string.Format(
                @"SELECT * from fdb_ca..RDAMXHC0_HIC_ALRGN_XSENSE_LINK WHERE HIC_SEQN in ({0})",
                String.Join(",", componentParams.Item2)
            );
            return new DB.Select
            {
                Sql = sql,
                Parameters = componentParams.Item1.ToArray()
            }.RunForListOfStrings("DAM_ALRGN_XSENSE");
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
            var drugs = checklistDrugs.Select(x => x.Substring(1)).ToList();
            var drugParams = DB.GetParamsList(drugs, SqlDbType.Decimal, 8);
            var paramsList = new List<SqlParameter>();
            var checklistSel = "";
            if (drugs.Count > 0)
            {
                paramsList.AddRange(drugParams.Item1);
                checklistSel = string.Format(
                    " AND rggl.ROUTED_GEN_ID IN ({0})",
                    string.Join(",", drugParams.Item2)
                );
            }

            var componentParams = DB.GetParamsList(algDrugs, SqlDbType.Decimal, 6, "c");
            paramsList.AddRange(componentParams.Item1);

            var sql = string.Format(@"SELECT DISTINCT
                'R' + RIGHT('0000000' + cast(rggl.ROUTED_GEN_ID as varchar), 8) AS drug,
                fbn.brand_name AS brand,
                RIGHT('00000' + cast(hhl.HIC_SEQN as varchar), 6) AS cdrug,
                'N' as croot,
                RIGHT('00000' + cast(hd.HIC_ROOT as varchar), 6) AS c_root
            FROM fdb_ca..RHICL1_HIC_HICLSEQNO_LINK hhl
            LEFT JOIN fdb_ca..RHICD5_HIC_DESC hd ON hd.HIC_SEQN = hhl.HIC_SEQN
            LEFT JOIN fdb_ca..RGCNSEQ4_GCNSEQNO_MSTR gm ON gm.HICL_SEQNO = hhl.HICL_SEQNO
            LEFT JOIN fdb_ca..RMIID2_MED m ON m.GCN_SEQNO = gm.GCN_SEQNO
            LEFT JOIN fdb_ca..RRTGNGC0_RTD_GEN_GCNSEQNO_LNK rggl ON rggl.GCN_SEQNO = m.GCN_SEQNO
            LEFT JOIN ibex..fdb_ca_brand_name fbn ON fbn.MEDID = m.MEDID
            WHERE
                ( hhl.HIC_SEQN IN ({0})
                OR hd.HIC_ROOT IN ({1})
              )
              AND fbn.brand_name IS NOT NULL{2}",
              string.Join(",", componentParams.Item2),
              string.Join(",", componentParams.Item2),
              checklistSel
            );

            var info = new List<Dictionary<string, string>>();

            var ds = new DB.Select
            {
                Sql = sql,
                Parameters = paramsList.ToArray()
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

            return info;
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

            var whereParam = "fbn.brand_name=@brand";
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
                whereParam += " and fii.din in (SELECT fii.base_din AS ndc FROM ibex..frm ";
                if (exactMatch)
                {
                    whereParam += "LEFT JOIN ibex..fdb_ca_idc_info fii ON fii.din = frm.ndc " +
                        "LEFT JOIN ibex..fdb_ca_brand_name fbn ON fbn.MEDID = fii.MEDID ";
                }
                else
                {
                    whereParam += "LEFT JOIN ibex..fdb_ca_brand_name fbn ON fbn.PC_ROUTED_GEN_ID = frm.dnum " +
                        "LEFT JOIN ibex..fdb_ca_idc_info fii ON fii.medid = fbn.medid AND fii.din = fii.base_din ";
                }
                whereParam += " where fbn.brand_name=@brand_description and site=@site";
                whereParamVals.AddRange(new[] {
                    new SqlParameter("@brand_description", SqlDbType.VarChar) { Value = brand },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                });

                whereParam += " and (" + string.Join(" OR ", filters) + ")";
            }
            else
            {
                whereParam += " AND fii.din = fii.base_din ";
            }

            return GetDrugInfoWhere(whereParam, whereParamVals);
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
                        { "field", "fdb_ca_allergy_name.MED_NAME_ID" },
                        { "drug_id", "fdb_ca_allergy_name.PC_MED_NAME_ID" }
                    }
                },
                { "R", new Dictionary<string, string> {
                        { "field", "rggl.ROUTED_GEN_ID" },
                        { "drug_id", "'R' + RIGHT('0000000' + cast(rggl.ROUTED_GEN_ID AS varchar), 8)" }
                    }
                },
                { "L", new Dictionary<string, string> {
                        { "field", "hhl.HICL_SEQNO" },
                        { "drug_id", "'L' + RIGHT('00000' + cast(hhl.HICL_SEQNO AS varchar), 6)" }
                    }
                },
                { "",  new Dictionary<string, string> {
                        { "field", "hhl.HIC_SEQN" },
                        { "drug_id", "RIGHT('00000' + cast(hhl.HIC_SEQN AS varchar), 6)" }
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
                        "ibex..fdb_ca_allergy_name ON fdb_ca_allergy_name.HICL_SEQNO = RHICL1_HIC_HICLSEQNO_LINK.HICL_SEQNO"
                    }
                },
                { "R", new List<string> {
                        "fdb_ca..RGCNSEQ4_GCNSEQNO_MSTR gm on gm.HICL_SEQNO = hhl.HICL_SEQNO",
                        "fdb_ca..RRTGNGC0_RTD_GEN_GCNSEQNO_LNK rggl on rggl.GCN_SEQNO = gm.GCN_SEQNO"
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
                "fdb_ca..RHICL1_HIC_HICLSEQNO_LINK hhl",
                "fdb_ca..RHICD5_HIC_DESC hd ON hd.HIC_SEQN = hhl.HIC_SEQN"
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
                        hd.HIC_DESC AS name,
                        RIGHT('00000' + cast(hd.HIC_SEQN as varchar), 6) AS cdrug,
                        'N' AS croot,
                        RIGHT('00000' + cast(hd.HIC_ROOT as varchar), 6) AS c_root
                        FROM {1}
                        WHERE hd.HIC_ROOT != 9870
                        AND {2} IN ({3})",
                        drugId,
                        string.Join(" LEFT JOIN ", keyJoins),
                        field,
                        paramsString
                );

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
                  FROM fdb_ca..RMIID2_MED
                  LEFT JOIN fdb_ca..RRTGNGC0_RTD_GEN_GCNSEQNO_LNK ON RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.GCN_SEQNO = RMIID2_MED.GCN_SEQNO
                  LEFT JOIN fdb_ca..RMIDFID2_ROUTED_DOSE_FORM_MED ON RMIDFID2_ROUTED_DOSE_FORM_MED.ROUTED_DOSAGE_FORM_MED_ID = RMIID2_MED.ROUTED_DOSAGE_FORM_MED_ID
                  LEFT JOIN fdb_ca..RMIRMID1_ROUTED_MED ON RMIRMID1_ROUTED_MED.ROUTED_MED_ID = RMIDFID2_ROUTED_DOSE_FORM_MED.ROUTED_MED_ID
                  LEFT JOIN fdb_ca..RGCNSEQ4_GCNSEQNO_MSTR ON RGCNSEQ4_GCNSEQNO_MSTR.GCN_SEQNO = RMIID2_MED.GCN_SEQNO
                  LEFT JOIN fdb_ca..RHICL1_HIC_HICLSEQNO_LINK ON RHICL1_HIC_HICLSEQNO_LINK.HICL_SEQNO = RGCNSEQ4_GCNSEQNO_MSTR.HICL_SEQNO
                  LEFT JOIN fdb_ca..RHICD5_HIC_DESC ON RHICD5_HIC_DESC.HIC_SEQN = RHICL1_HIC_HICLSEQNO_LINK.HIC_SEQN
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
        /// Get the codes and descriptions for drugs by formulation ID, for finding equivalent medications.
        /// </summary>
        /// <param name="formulationId">Formulation ID</param>
        /// <returns>List of Dictionaries of information about formulation ID equivalents</returns>
        public List<Dictionary<string, string>> GetDrugInfoByFormulationId(string formulationId)
        {
            var whereParam = "m.GCN_SEQNO=@mmdc AND fii.din=fii.base_din";
            return GetDrugInfoWhere(whereParam, new List<SqlParameter> { new SqlParameter("@mmdc", SqlDbType.VarChar) { Value = formulationId } });
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
            var whereParam = "fii.din IN (" + string.Join(",", p.Item2) + ")";
            return GetDrugInfoWhere(whereParam, p.Item1);
        }

        /// <summary>
        /// Gather drug interaction information
        /// </summary>
        /// <param name="components">List of drug IDs</param>
        /// <returns>A List of Dictionary objects with interaction information associated with the list of drugs</returns>
        public List<Dictionary<string, string>> GetDrugInteractions(List<string> components)
        {
            var lookupComponents = components.Where(x => x.Substring(0, 1).Equals("R")).Select(y => y.Substring(1)).ToList();
            var ret = new List<Dictionary<string, string>>();
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                var res = new DB.Select
                {
                    Connection = con,
                    Sql = "[dbo].pc_fdb_ca_get_drug_interactions",
                    IsStoredProcedure = true,
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@drugs", SqlDbType.VarChar) { Value = string.Join(",", lookupComponents) }
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
			
              fii.DIN AS ndc,
              fii.MEDID,

              fbn.long_brand_name as long_brand,
              fii.GCN_SEQNO AS multum,

              fbn.brand_name as brand,
              fbn.PC_ROUTED_GEN_ID as drug,

              fan.med_name,
              fbn.PC_MED_NAME_ID med_drug_id,

              fan.allergy_name as alg_name,
              fan.PC_HICL_SEQNO as alg_drug_id,

              fbn.active,

              fii.base_din,
              fii.packaging,
              fii.strength,
              df.MED_DOSAGE_FORM_DESC AS dose_form,
              rd.GCRT_DESC AS route,

              fbn.dea_schedule,
              fbn.rx_otc,
              fbn.generic,
              days_obsolete,
              case
                when days_obsolete > 0 then 'Y'
                else 'N'
              end as obsolete,
              cast(gm.tc as varchar) AS drugcat,
			  cast(gm.gtc as varchar) as druggroup

            FROM
              ibex..fdb_ca_idc_info fii
              LEFT JOIN ibex..fdb_ca_brand_name fbn ON fbn.MEDID = fii.MEDID
              LEFT JOIN ibex..fdb_ca_allergy_name fan ON fan.MEDID = fii.MEDID
              LEFT JOIN fdb_ca..RMIID2_MED m ON m.MEDID = fii.MEDID
              LEFT JOIN fdb_ca..RGCNSEQ4_GCNSEQNO_MSTR gm ON gm.GCN_SEQNO = m.GCN_SEQNO

              LEFT JOIN fdb_ca..RMIDFID2_ROUTED_DOSE_FORM_MED rdfm ON rdfm.ROUTED_DOSAGE_FORM_MED_ID = m.ROUTED_DOSAGE_FORM_MED_ID
              LEFT JOIN fdb_ca..RMIDFD1_DOSE_FORM df ON df.MED_DOSAGE_FORM_ID = rdfm.MED_DOSAGE_FORM_ID
              LEFT JOIN fdb_ca..RROUTED3_ROUTE_DESC rd ON rd.GCRT = gm.GCRT

              LEFT JOIN fdb_ca..RHICL1_HIC_HICLSEQNO_LINK hhl ON hhl.HICL_SEQNO = gm.HICL_SEQNO
              LEFT JOIN fdb_ca..RHICD5_HIC_DESC hd ON hd.HIC_SEQN = hhl.HIC_SEQN

			  LEFT JOIN fdb_ca..rxn_din_rxcui ON rxn_din_rxcui.din = fii.DIN
			  LEFT JOIN fdb_ca..rxn_map ON rxn_map.rxcui = rxn_din_rxcui.rxcui

            WHERE
              m.MED_STATUS_CD IN (0,3)
              AND {0}
              ORDER  BY fbn.brand_name,df.MED_DOSAGE_FORM_DESC,gm.STR60,hhl.HIC_REL_NO
              ", whereParam);

            var ret = new List<Dictionary<string, string>>();
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
                @"SELECT {0} rxl.*, fcbn.dea_schedule, fcbn.rx_otc
                  FROM {1} rxl LEFT JOIN ibex..fdb_ca_idc_info fcii ON fcii.din = rxl.ndc
                  LEFT JOIN ibex..fdb_ca_brand_name fcbn ON fcbn.MEDID = fcii.medid
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
                (SELECT base_din FROM ibex..fdb_ca_idc_info WHERE din = frm.ndc) AS base_din
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
            var drugIds = ids.Where(x => !x.Equals("ft")).Select(y => y.Substring(1)).ToList();
            var drugIdSql = "";
            var sqlParameters = new List<SqlParameter>();
            var drugIdParams = DB.GetParamsList(drugIds, SqlDbType.Decimal, 8);
            if (ids.Count > 0)
            {
                drugIdSql = string.Format(
                    "where ROUTED_GEN_ID in ({0})",
                    string.Join(",", drugIdParams.Item2)
                );
                sqlParameters.AddRange(drugIdParams.Item1);
            }

            var we = new Dictionary<string, string>();
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();

                // Let's find which ones have side effects
                var res = new DB.Select
                {
                    Connection = con,
                    Sql = "select ROUTED_GEN_ID from fdb_ca..RSIDERG0_ROUTED_GEN_LINK " + drugIdSql,
                    Parameters = sqlParameters.ToArray()
                }.RunForDataReader();
                while (res.Read())
                {
                    var id = "R" + res["ROUTED_GEN_ID"].ToString().Trim();
                    if (!we.ContainsKey(id))
                        we.Add(id, "Y");
                }
                res.Close();

                // Now let's find the warnings
                res = new DB.Select
                {
                    Connection = con,
                    Sql = "select distinct ROUTED_GEN_ID from fdb_ca..RRTGNGC0_RTD_GEN_GCNSEQNO_LNK left join fdb_ca..RLBLWGC0_GCNSEQNO_LINK on RRTGNGC0_RTD_GEN_GCNSEQNO_LNK.GCN_SEQNO = RLBLWGC0_GCNSEQNO_LINK.GCN_SEQNO " + drugIdSql,
                    Parameters = sqlParameters.ToArray()
                }.RunForDataReader();
                while (res.Read())
                {
                    var id = "R" + res["ROUTED_GEN_ID"].ToString().Trim();
                    if (!we.ContainsKey(id))
                        we.Add(id, "Y");
                }
                res.Close();
                con.Close();
            }

            return we;
        }
    }
}