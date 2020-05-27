using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle order entry stuff
    /// </summary>
    public static class OrderEntry
    {
        /// <summary>
        /// Get the restricted codeset list
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="mnemlist">List of dictionary objects containing mnemonic, svc code, and gender information</param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, List<string>>> GetRestrictedCodesetList(byte siteId, List<Dictionary<string, string>> mnemlist)
        {
            var codesets = new Dictionary<string, Dictionary<string, List<string>>>();

            if (siteId <= 0 || mnemlist == null || mnemlist.Count == 0)
                return codesets;

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
            };

            var svcmnemwhere = new List<string>();

            var mnemonicList = mnemlist.Select(x => x["mnemonic"]).ToList();
            var mnemParams = DB.GetParamsList(mnemonicList, SqlDbType.Char, "m");

            var svcList = mnemlist.Select(x => x["svccode"]).ToList();
            var svcParams = DB.GetParamsList(svcList, SqlDbType.Char, "s");

            var genderList = mnemlist.Select(x => x["gender"]).ToList();
            var genderParams = DB.GetParamsList(genderList, SqlDbType.Char, "g");

            sqlParams.AddRange(mnemParams.Item1);
            sqlParams.AddRange(svcParams.Item1);
            sqlParams.AddRange(genderParams.Item1);

            for(var i = 0; i < mnemonicList.Count(); i++)
            {
                svcmnemwhere.Add(string.Format(
                    "(mnemonic = {0} AND svccode = {1} AND gender = {2})",
                    mnemParams.Item1[i].ParameterName,
                    svcParams.Item1[i].ParameterName,
                    genderParams.Item1[i].ParameterName
                ));
            }

            var sql = "SELECT svccode,mnemonic,codevalue FROM query_restricted_codeset qrc inner join org on qrc.site=org.svccs WHERE org.site = @site AND (" + string.Join(" OR ", svcmnemwhere) + ") ORDER BY codevalue";
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                var results = new DB.Select
                {
                    Connection = con,
                    Sql = sql,
                    Parameters = sqlParams.ToArray()
                }.RunForDataReader();

                while (results.Read())
                {
                    var svccode = results["svccode"].ToString().TrimEnd();
                    var mnemonic = results["mnemonic"].ToString().TrimEnd();
                    if (!codesets.ContainsKey(svccode))
                        codesets[svccode] = new Dictionary<string, List<string>>();

                    if (!codesets[svccode].ContainsKey(mnemonic))
                        codesets[svccode][mnemonic] = new List<string>();

                    codesets[svccode][mnemonic].Add(results["codevalue"]?.ToString());
                }

                results.Close();
                con.Close();
            }

            return codesets;
        }

        /// <summary>
        /// Create a queue file so the interface knows to send things for the patient
        /// </summary>
        /// <param name="siteId">Patient's site</param>
        /// <param name="patientId">Patient's ID (ibex) number</param>
        public static void CreateQueueFile(byte siteId, string patientId)
        {
            var orgInfo = new DB.Select
            {
                Sql = "SELECT root FROM org WHERE site = @site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
                }
            }.RunForDataRow();

            var root = orgInfo["root"].ToString().Trim();
            var filePath = root + "link\\snd\\" + patientId;
            FileWriter.Write(filePath, "");
        }

        /// <summary>
        /// Update the indicators that show the status of orders for a patient 
        /// </summary>
        /// <param name="siteId">Patient's site</param>
        /// <param name="patientId">Patient's ID (ibex) number</param>
        /// <param name="indicators">Dictionary of indicators</param>
        public static void UpdateIndicators(byte siteId, string patientId, Dictionary<string, DepartmentIndicator> indicators)
        {
            if (!indicators.Any())
                return;

            var updates = new Dictionary<string, string>();
            foreach (var indicator in indicators)
            {
                var interfaceId = indicator.Key;
                var statusLetter = Constants.ORDER_STATUS[indicators[interfaceId].Status];
                updates.Add(indicators[interfaceId].PatientColumn, statusLetter);
            }

            if (!updates.Any())
                return;

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
            };

            sqlParams.AddRange(updates.Select(x => new SqlParameter("@" + x.Key, x.Value)).ToArray());

            var sql = "Update pat set " + string.Join(", ", updates.Select(x => x.Key + "=@" + x.Key).ToArray()) + " where ibex=@ibex and site=@site";

            (new DB.Update
            {
                Sql = sql,
                Parameters = sqlParams.ToArray(),
            }).Run();
        }

        /// <summary>
        /// Load the custom departments for a site
        /// </summary>
        /// <param name="siteId">Site ID used for the service code sharing</param>
        /// <returns>A Dictionary of {{department ID}} => DepartmentIndicator </returns>
        public static Dictionary<string, DepartmentIndicator> LoadCustomDepartmentIndicators(byte siteId)
        {
            var depts = new Dictionary<string, DepartmentIndicator>();
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();

                // Let's find which ones have side effects
                var res = new DB.Select
                {
                    Connection = con,
                    Sql = "SELECT id,misc,misc2 FROM idx WHERE site=@site AND type='8'",
                    Parameters = new[] { new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId } },
                }.RunForDataReader();
                while (res.Read())
                {
                    if (!string.IsNullOrWhiteSpace(res["misc"].ToString()))
                    {
                        depts.Add(res["id"].ToString(), new DepartmentIndicator
                        {
                            DepartmentNumber = System.Convert.ToInt32(res["misc"]),
                            DepartmentLetter = res["misc2"].ToString(),
                        });
                    }
                }
                res.Close();
            }

            return depts;
        }

        /// <summary>
        /// Load the order settings for given svc codes
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="svcKeys">Optional list of svc codes to get the settings for</param>
        /// <returns>A multi-level dictionary, which first uses the svc code for the key, then the svc columns for the next level's key</returns>
        public static Dictionary<string, Dictionary<string, string>> LoadOrderSettings(byte siteId, List<string> svcKeys = null)
        {
            var ordInfo = new Dictionary<string, Dictionary<string, string>>();

            if (siteId <= 0)
            {
                return ordInfo;
            }
            
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
            };            

            var sql = "SELECT " + string.Join(",", Constants.ORDER_SETTINGS_COLS.Select(c => "svc." + c)) + " FROM svc inner join org on org.svccs = svc.site WHERE org.site = @site";
            if (svcKeys != null && svcKeys.Count > 0)
            {
                var svcParams = DB.GetParamsList(svcKeys, SqlDbType.Char);
                sql += " AND code IN (" + string.Join(",", svcParams.Item2) + ")";
                sqlParams.AddRange(svcParams.Item1);
            }

            ordInfo = new DB.Select
            {
                Sql = sql,
                Parameters = sqlParams.ToArray()
            }.RunForDictionary("code");

            return ordInfo;
        }

        /// <summary>
        /// Load the query defaults for given svc codes
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="svcKeys">Optional list of svc codes to get the defaults for</param>
        /// <returns>A multi-level dictionary, which first uses the svc code for the key, then the mnemonic for the next level's key</returns>
        public static Dictionary<string, Dictionary<string, Dictionary<string, string>>> LoadQueryDefaults(byte siteId, List<string> svcKeys)
        {
            var ordDefaults = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            if (siteId <= 0 || svcKeys.Count == 0)
                return ordDefaults;

            var svcParams = DB.GetParamsList(svcKeys, SqlDbType.Char);
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
            };

            var inclause = " AND svccode IN (" + string.Join(",", svcParams.Item2) + ")";
            sqlParams.AddRange(svcParams.Item1);

            var sql = "SELECT qd.* FROM query_defaults qd inner join org on org.svccs = qd.site WHERE org.site = @site AND req <> 'A'" + inclause;
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                var results = new DB.Select
                {
                    Connection = con,
                    Sql = sql,
                    Parameters = sqlParams.ToArray()
                }.RunForDataReader();

                while (results.Read())
                {
                    // Each service (svc code) can have multiple mnemonics (which means multiple queries attached),
                    // but the combination of svc code + mnemonic (per site) is unique.
                    var svccode = results["svccode"].ToString().Trim();
                    var mnemonic = results["mnemonic"].ToString().Trim();
                    if (!ordDefaults.ContainsKey(svccode))
                    {
                        ordDefaults[svccode] = new Dictionary<string, Dictionary<string, string>>();
                    }
                    ordDefaults[svccode][mnemonic] = new Dictionary<string, string>
                    {
                        { "default_value", results["defvalue"]?.ToString() },
                        { "required", results["req"]?.ToString() },
                        { "default_value_male", results["maledefault"]?.ToString() },
                        { "default_value_female", results["femaledefault"]?.ToString() },
                        { "default_display_on_chart", results["display_on_chart"]?.ToString() },
                    };
                }
                results.Close();
                con.Close();
            }

            return ordDefaults;
        }

        /// <summary>
        /// Load the basic information for queries
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="mnemlist">Optional list of mnemonics to retrieve</param>
        /// <param name="ordersOnly">Optional flag to specify whether only queries displayed on the orders screen should be retrieved</param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> LoadQueryInfo(byte siteId, List<string> mnemlist = null, bool ordersOnly = false)
        {
            var orderDefaults = new Dictionary<string, Dictionary<string, string>>();

            if (siteId <= 0)
            {
                return orderDefaults;
            }

            var mnemonicParams = DB.GetParamsList(mnemlist, SqlDbType.Char);
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
            };

            var inclause = "";
            if (mnemlist != null && mnemlist.Count > 0)
            {
                inclause = " AND mnemonic IN(" + string.Join(",", mnemonicParams.Item2) + ")";
                sqlParams.AddRange(mnemonicParams.Item1);
            }
            
            // Orders should only get active queries
            if (ordersOnly)
            {
                inclause += " AND orderspage = 'Y' AND qi.status = 'A'";
            }

            var sql = "SELECT qi.* FROM query_info qi inner join org on org.svccs=qi.site WHERE org.site=@site AND ord_or_adreq='O'" + inclause + " order by position, qi.description";
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                var results = new DB.Select
                {
                    Connection = con,
                    Sql = sql,
                    Parameters = sqlParams.ToArray()
                }.RunForDataReader();

                while (results.Read())
                {
                    var mnemonic = results["mnemonic"].ToString().Trim();
                    orderDefaults[mnemonic] = new Dictionary<string, string>
                    {
                        { "req", results["req"]?.ToString() },
                        { "name", results["description"]?.ToString() },
                        { "max_length", results["maxlength"]?.ToString() },
                        { "type", results["displaytype"]?.ToString().Trim() },
                        { "type_options", results["codeset"]?.ToString() },
                        { "sequence", results["position"]?.ToString() },
                        { "orderspage", results["orderspage"]?.ToString() },
                        { "display_once", results["ordersonce"]?.ToString() },
                        { "gender_defaults", results["genderdefaults"]?.ToString() },
                        { "status", results["status"]?.ToString() },
                        { "display_on_chart", results["display_on_chart"]?.ToString() }
                    };
                }
                results.Close();
                con.Close();
            }

            return orderDefaults;
        }

        /// <summary>
        /// Load the instructions for a given query (or queries)
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="mnemlist">Optional list of mnemonics to retrieve</param>
        /// <returns>Dictionary linking instructions to mnemonics</returns>
        public static Dictionary<string, string> LoadQueryInstructions(byte siteId, List<string> mnemlist = null)
        {
            var instructions = new Dictionary<string, string>();

            if (siteId <= 0)
                return instructions;

            var mnemParams = DB.GetParamsList(mnemlist, SqlDbType.Char);
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId }
            };

            var inclause = "";
            if (mnemlist != null && mnemlist.Count > 0)
            {
                inclause = " AND mnemonic IN (" + string.Join(",", mnemParams.Item2) + ")";
                sqlParams.AddRange(mnemParams.Item1);
            }

            var sql = "SELECT instruction,mnemonic FROM query_info qi inner join org on org.svccs=qi.site WHERE org.site=@site AND displaytype='instruction'" + inclause;
            using (var con = new SqlConnection(DB.GetConnectionString()))
            {
                con.Open();
                var results = new DB.Select
                {
                    Connection = con,
                    Sql = sql,
                    Parameters = sqlParams.ToArray()
                }.RunForDataReader();

                while (results.Read())
                {
                    instructions[results["mnemonic"].ToString()] = results["instruction"]?.ToString();
                }
                results.Close();
                con.Close();
            }

            return instructions;
        }       

        /// <summary>
        /// Order Entry codeset handling
        /// </summary>
        public static class Codeset
        {
            /// <summary>
            /// Stores codeset values by site and name
            /// </summary>
            private static Dictionary<byte, Dictionary<string, Dictionary<string, Dictionary<string, string>>>> values = new Dictionary<byte, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>();

            /// <summary>
            /// Add new values. Pre-populates the object's cache. While it is not necessary to call this method,
            /// calling it saves time as the number of codes grows
            /// </summary>
            /// <param name="siteId">Site identifier</param>
            /// <param name="valsList">List of Dictionary objects with information to add</param>
            public static void AddNewValues(byte siteId, List<Dictionary<string, string>> valsList)
            {
                var valsArr = new List<Dictionary<string, string>>();
                if (values.ContainsKey(siteId))
                {
                    foreach (var value in valsList)
                    {
                        // Don't need to add values that already exist
                        if (value != null || !(values.ContainsKey(siteId) && values[siteId].ContainsKey(value["value"])))
                        {
                            valsArr.Add(value);
                        }
                    }
                }

                if (valsArr.Count == 0)
                {
                    return;
                }

                var codes = valsArr.Select(x => x["codeset"]).ToList();
                var codeParams = DB.GetParamsList(codes, SqlDbType.Char, "c");
                var vals = valsArr.Select(x => x["value"]).ToList();
                var valParams = DB.GetParamsList(vals, SqlDbType.VarChar, "v");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                    new SqlParameter("@flag", SqlDbType.Char) { Value = Constants.ORDER_QUERY_ID }
                };
                sqlParams.AddRange(valParams.Item1);

                var sql = string.Format(@"SELECT
                            ord_codes.id, ord_codes.name, ord_code_set.name AS ocsname
                           FROM
                            ord_codes, ord_code_set
                           WHERE
                            ord_codes.type = ord_code_set.typeid
                            AND ord_code_set.name IN ({0})
                            AND ord_codes.site=@site
                            AND ord_codes.site = ord_code_set.site
                            AND ord_codes.status='A'
                            AND ord_codes.flag=@flag
                            AND ord_code_set.flag=@flag", string.Join(",", valParams.Item2)
                );
                if (codes.Count > 0)
                {
                    sql += " AND ord_codes.id IN (" + string.Join(",", codeParams.Item2) + ")";
                    sqlParams.AddRange(codeParams.Item1);
                }

                var codehash = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                using (var con = new SqlConnection(DB.GetConnectionString()))
                {
                    con.Open();
                    var results = new DB.Select
                    {
                        Connection = con,
                        Sql = sql,
                        Parameters = sqlParams.ToArray()
                    }.RunForDataReader();
                    while (results.Read())
                    {
                        var ocsName = results["ocsname"]?.ToString();
                        var id = results["id"].ToString();
                        var name = results["name"].ToString();
                        if (!codehash.ContainsKey(ocsName))
                        {
                            codehash[ocsName] = new Dictionary<string, Dictionary<string, string>>();
                        }
                        codehash[ocsName][id] = new Dictionary<string, string>
                        {
                            { "name", name }
                        };
                    }
                    results.Close();
                    con.Close();
                }

                foreach(var ocsName in codehash.Keys)
                {
                    SetValues(siteId, ocsName, codehash[ocsName]);
                }
            }

            /// <summary>
            /// Get codeset values
            /// </summary>
            /// <param name="siteId">Site identifier</param>
            /// <param name="name">Name</param>
            /// <returns>Codeset values dictionary</returns>
            public static Dictionary<string, Dictionary<string, string>> GetValues(byte siteId, string name)
            {
                if (values.ContainsKey(siteId) && values[siteId].ContainsKey(name))
                {
                    return values[siteId][name];
                }

                var sql = @"SELECT
                            ord_codes.id, ord_codes.name
                           FROM
                            ord_codes, ord_code_set
                           WHERE
                            ord_codes.type=ord_code_set.typeid
                            AND ord_code_set.name=@name
                            AND ord_codes.site=@site
                            AND ord_codes.site=ord_code_set.site
                            AND ord_codes.status='A'
                            AND ord_codes.flag=@flag
                            AND ord_code_set.flag=@flag";

                var codehash = new Dictionary<string, Dictionary<string, string>>();
                using (var con = new SqlConnection(DB.GetConnectionString()))
                {
                    con.Open();
                    var results = new DB.Select
                    {
                        Connection = con,
                        Sql = sql,
                        Parameters = new SqlParameter[]
                        {
                            new SqlParameter("@name", SqlDbType.VarChar) { Value = name },
                            new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                            new SqlParameter("@flag", SqlDbType.Char) { Value = Constants.ORDER_QUERY_ID }
                        }
                    }.RunForDataReader();
                    while (results.Read())
                    {
                        codehash[results["id"].ToString().TrimEnd()] = new Dictionary<string, string>
                        {
                            { "name", results["name"].ToString() },
                            { "misc", null }
                        };
                    }
                    results.Close();
                    con.Close();
                }

                SetValues(siteId, name, codehash);

                return codehash;
            }

            /// <summary>
            /// Store codeset values in the cache
            /// </summary>
            /// <param name="siteId">Site identifier</param>
            /// <param name="name">Name</param>
            /// <param name="codehash">Codeset values hash</param>
            private static void SetValues(byte siteId, string name, Dictionary<string, Dictionary<string, string>> codehash)
            {
                if (!values.ContainsKey(siteId))
                {
                    values[siteId] = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                }
                values[siteId][name] = codehash;
            }
        }

        public class DepartmentIndicator
        {
            public string Status { get; set; }

            public int DepartmentNumber {
                set { _patCol = "ord" + (value + 47); }
            }

            private string _patCol;
            public string PatientColumn {
                get { return _patCol; }
                set { _patCol = value; }
            }

            public string DepartmentLetter { get; set; }
        }

        /// <summary>
        /// Constants used in order entry
        /// </summary>
        public class Constants
        {
            /// <summary>
            /// Columns in query_info
            /// </summary>
            public static readonly List<string> QUERY_INFO_COLS = new List<string> {
                "status",
                "adreq_default",
                "adreq_maledefault",
                "adreq_femaledefault",
                "display_on_chart",
                "req",
                "mnemonic",
                "description",
                "maxlength",
                "displaytype",
                "position",
                "orderspage",
                "ordersonce",
                "genderdefaults",
                "codeset",
                "instruction"
            };

            /// <summary>
            /// Columns in query_defaults
            /// </summary>
            public static readonly List<string> QUERY_DEFAULTS_COLS = new List<string> {
                "svccode",
                "mnemonic",
                "defvalue",
                "req",
                "maledefault",
                "femaledefault",
                "display_on_chart"
            };


            /// <summary>
            /// Columns in svc for the order settings
            /// </summary>
            public static readonly List<string> ORDER_SETTINGS_COLS = new List<string> {
                "code",
                "name",
                "amt",
                "svctype",
                "face",
                "apc",
                "prompt",
                "promptreq",
                "lrreq",
                "specimens",
                "chgcode",
                "hcpcs",
                "task",
                "department",
                "lvlpts",
                "lvlmin",
                "timeout",
                "repeat",
                "maxqty",
                "cpt",
            };

            #region Interface types
            /// <summary>
            /// Admission request interface identifier
            /// </summary>
            public const string ADMISSION_REQ_INTERFACE = "A";

            /// <summary>
            /// Consults interface identifier
            /// </summary>
            public const string CONSULTS_INTERFACE = "Z";

            /// <summary>
            /// CVT interface identifier
            /// </summary>
            public const string CVT_INTERFACE = "C";

            /// <summary>
            /// EEG interface identifier
            /// </summary>
            public const string EEG_INTERFACE = "N";

            /// <summary>
            /// EKG interface identifier
            /// </summary>
            public const string EKG_INTERFACE = "E";

            /// <summary>
            /// Laboratory interface identifier
            /// </summary>
            public const string LABORATORY_INTERFACE = "L";

            /// <summary>
            /// Med orders interface identifier
            /// </summary>
            public const string MED_ORDERS_INTERFACE = "M";

            /// <summary>
            /// Nursing Orders interface identifier
            /// </summary>
            public const string NURSING_ORDERS = "Q";

            /// <summary>
            /// Nutritiion interface identifier
            /// </summary>
            public const string NUTRITION_INTERFACE = "D";

            /// <summary>
            /// Physical therapy interface identifier
            /// </summary>
            public const string PHYSICAL_THERAPY_INTERFACE = "H";

            /// <summary>
            /// Radiology interface identifier
            /// </summary>
            public const string RADIOLOGY_INTERFACE = "X";

            /// <summary>
            /// Respiratory interface identifier
            /// </summary>
            public const string RESPIRATORY_INTERFACE = "R";
            #endregion

            #region Order status codes
            /// <summary>
            /// "Outstanding" order status code
            /// </summary>
            public const string OUTSTANDING = "O";

            /// <summary>
            /// "Queued" order status code
            /// </summary>
            public const string QUEUED = "I";

            /// <summary>
            /// "Resulted or Status Updated" status code
            /// </summary>
            public const string RESULTED_OR_STATUS_UPDATED = "D";

            /// <summary>
            /// "Sent and acknowledged" status code
            /// </summary>
            public const string SENT_AND_ACKNOWLEDGED = "S";

            /// <summary>
            /// "Specimen received" status code
            /// </summary>
            public const string SPECIMEN_RECEIVED = "A";

            /// <summary>
            /// "Specimen received" status code
            /// </summary>
            public const string CUSTOM_STATUS = "C";
            #endregion

            #region Numeric order status codes
            /// <summary>
            /// "Canceled order" status code 1
            /// </summary>
            public const string CANCELED_ORDER_1 = "50";

            /// <summary>
            /// "Canceled order" status code 2
            /// </summary>
            public const string CANCELED_ORDER_2 = "52";

            /// <summary>
            /// "Pending order" status code
            /// </summary>
            public const string PENDING_ORDER = "10";

            /// <summary>
            /// "Received order" status code
            /// </summary>
            public const string RECEIVED_ORDER = "30";

            /// <summary>
            /// "Repeating order" status code
            /// </summary>
            public const string REPEATING_ORDER = "21";

            /// <summary>
            /// "Resulted order" status code
            /// </summary>
            public const string RESULTED_ORDER = "40";

            /// <summary>
            /// "Sent order" status code
            /// </summary>
            public const string SENT_ORDER = "20";
            #endregion

            /// <summary>
            /// Flag indication the order was sent inbound from an external interface
            /// </summary>
            public const string INBOUND_ORDER = "I";

            /// <summary>
            /// Flag indicating the order will be sent outbound
            /// </summary>
            public const string OUTBOUND_ORDER = "O";

            /// <summary>
            /// Identify this query as an order query (instead of an admiission request query). This constant
            /// is used for the value of query_info.ord_or_adreq when saving queries to the database, if a 
            /// value is not provided.
            /// </summary>
            public const string ORDER_QUERY_ID = "O";

            /// <summary>
            /// Identify this query as an admission request query.
            /// </summary>
            public const string ADMISSION_REQUEST_QUERY_ID = "A";

            /// <summary>
            /// Maximum query position value. Smallint storage in database.
            /// </summary>
            public const int MAX_QUERY_POSITION = 32767;

            /// <summary>
            /// Pat table columns used to store indicator value for each type of interface
            /// </summary>
            public static readonly Dictionary<string, string> INDICATOR_COLUMNS = new Dictionary<string, string>
            {
                { ADMISSION_REQ_INTERFACE, "ord56" },
                { CONSULTS_INTERFACE, "ord46" },
                { CVT_INTERFACE, "ord20" },
                { EKG_INTERFACE, "ord3" },
                { PHYSICAL_THERAPY_INTERFACE, "ord26" },
                { LABORATORY_INTERFACE, "ord1" },
                { MED_ORDERS_INTERFACE, "ord30" },
                { EEG_INTERFACE, "ord22" },
                { NURSING_ORDERS, "ord4" },
                { NUTRITION_INTERFACE, "ord25" },
                { RADIOLOGY_INTERFACE, "ord0" },
                { RESPIRATORY_INTERFACE, "ord2" },
            };

            /// <summary>
            /// # ord.face value = Indicator text
            /// </summary>
            public static readonly Dictionary<string, string> INDICATOR_TEXT = new Dictionary<string, string> {
                { CONSULTS_INTERFACE, "Z" },
                { CVT_INTERFACE, "C" },
                { EEG_INTERFACE, "G" },
                { EKG_INTERFACE, "E" },
                { LABORATORY_INTERFACE, "L" },
                { NURSING_ORDERS, "N" },
                { NUTRITION_INTERFACE, "D" },
                { PHYSICAL_THERAPY_INTERFACE, "P" },
                { RADIOLOGY_INTERFACE, "X" },
                { RESPIRATORY_INTERFACE, "R" }
            };

            /// <summary>
            /// Status code for each order indicator color
            /// </summary>
            public static readonly Dictionary<string, string> STATUS_CODES = new Dictionary<string, string> {
                { PENDING_ORDER, "Ordered" },
                { SENT_AND_ACKNOWLEDGED, "Sent and Acknowleged" },
                { RESULTED_OR_STATUS_UPDATED, "Resulted or Status Updated" },
                { CUSTOM_STATUS, "Custom Status" },                         
                { SPECIMEN_RECEIVED, "Specimen Received" },
                { QUEUED, "Queued" },
                { OUTSTANDING, "Outstanding" }
            };

            // TODO: finish getting the correct values
            public static readonly Dictionary<string, string> ORDER_STATUS = new Dictionary<string, string> {
                { "blank", " " },                           // Needs to be a space
                { RECEIVED_ORDER, SENT_AND_ACKNOWLEDGED },
                { "green", RESULTED_OR_STATUS_UPDATED },
                { "grey", CUSTOM_STATUS },
                { "orange", SPECIMEN_RECEIVED },
                { SENT_ORDER, QUEUED },
                { PENDING_ORDER, OUTSTANDING }
            };

            /// <summary>
            /// Type codes
            /// </summary>
            public static readonly Dictionary<string, string> TYPE_CODES = new Dictionary<string, string>
            {
                { "aerosol", "9" },
                { "blood", "O" },
                { "body", "U" },
                { "collectmethod", "AM" },
                { "frequency", "AA" },
                { "priority", "2" },
                { "reason", "AB" },
                { "specimen", "I" },
                { "transport", "0" }
            };
        }
    }
}