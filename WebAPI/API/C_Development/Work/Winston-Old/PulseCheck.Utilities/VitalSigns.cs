using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle Vital Signs
    /// </summary>
    public static class VitalSigns
    {
        private static Time _t = null;

        /// <summary>
        /// Stores information linking vital sign color code to style information
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> styleInfo = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Format the provided date/time string for vitals display. Only display the
        /// time portion of the passed value, unless the year, month, or day of the
        /// value is different from the current time, in which case the year, month, 
        /// and day are displayed as well.
        /// </summary>
        /// <param name="ts">Date/time string to format</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>Formatted date/time string</returns>
        public static string DateFormat(string ts, byte siteId)
        {
            if (_t == null)
            {
                _t = new Time(siteId);
            }
            var sysdate = _t.Timestamp();
            var ret = _t.ShortTime(ts);
            if (!ts.Substring(0, 8).Equals(sysdate.Substring(0, 8))) {
                ret = _t.ShortDateTime(ts);
            }

            return ret;
        }

        /// <summary>
        /// Get vital signs type order for a particular site.
        /// NOTE: This takes the identifier for the CURRENT PATIENT'S SITE, NOT THE ORG.VSCS SITE FOR THE CURRENT PATIENT'S SITE.
        /// </summary>
        /// <param name="site">Current patient's site identifier</param>
        /// <returns>List of vital sign name strings, in their defined order.</returns>
        public static List<string> GetTypeOrder(byte site)
        {
            var typeOrder = new List<string>();
            var result = new DB.Select
            {
                Sql = "SELECT v.name FROM vital_signs v JOIN org o ON v.site = o.vscs WHERE o.site=@site AND v.enabled=1 ORDER BY v.position",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site }
                }
            }.RunForDataSet();

            if (result != null && result.Tables.Count > 0)
            {
                typeOrder = result.Tables[0].AsEnumerable().Select(r => r.Field<string>("name")).ToList();
            }

            return typeOrder;
        }

        /// <summary>
        /// Get the list of valid vital sign sections, in order.
        /// </summary>
        /// <param name="site">Current patient's site identifier</param>
        /// <returns>List of valid vital sign sections</returns>
        public static List<string> GetVitalSignSections(byte site)
        {
            var vsSections = new List<string> { EMR.Constants.SECT_VITAL_SIGNS };
            vsSections.AddRange(GetVitalSignSections(site));
            vsSections.Add("TIME");
            return vsSections;
        }

        /// <summary>
        /// Get a list of the vital sign types
        /// </summary>
        /// <returns>List of vital sign types</returns>
        public static List<string> GetVitalSignType()
        {
            return new List<string>
            {
                Constants.BP,
                Constants.Pulse,
                Constants.Respiration,
                Constants.Temperature,
                Constants.Pain,
                Constants.O2Sat,
                Constants.Time,
                Constants.MAP,
                Constants.EndTidalCO2
            };
        }

        /// <summary>
        /// Get the Style information associated with a particular vital sign color code for range
        /// </summary>
        /// <param name="vitalColorCode">Color code determined after checking ranges for vital sign</param>
        /// <returns>Dictionary with keys and values that can be used to create a new Style object</returns>
        public static Dictionary<string, string> GetVitalStyleInfo(string vitalColorCode)
        {
            vitalColorCode = vitalColorCode.ToUpperInvariant();
            var returnStyleInfo = new Dictionary<string, string>();
            LoadStyleInfo();

            if (styleInfo.ContainsKey(vitalColorCode))
            {
                returnStyleInfo = styleInfo[vitalColorCode];
            }

            return returnStyleInfo;
        }

        /// <summary>
        /// Get vital signs range information for a particular site and patient age (in days).
        /// NOTE: This takes the identifier for the CURRENT PATIENT'S SITE, NOT THE ORG.VSCS SITE FOR THE CURRENT PATIENT'S SITE.
        /// </summary>
        /// <param name="site">Current patient's site identifier</param>
        /// <returns>Dictionary keyed by vital type identifier, pointing to a Dictionary keyed by vital range identifier, with value double defining range type limit</returns>
        public static Dictionary<string, Dictionary<string, Double?>> GetVSRangeForAge(byte site, int age)
        {
            var ds = new DB.Select
            {
                Sql = "SELECT * FROM [api].[GetSiteVSRanges] (@siteId, @age)",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@siteId", SqlDbType.TinyInt) { Value = site },
                    new SqlParameter("@age", SqlDbType.Int) { Value = age }
                }
            }.RunForDataSet();
            
            if (ds == null || ds.Tables.Count < 1 || ds.Tables[0].Rows.Count < 1)
            {
                return null;
            }

            var ranges = new Dictionary<string, Dictionary<string, Double?>>();
            foreach(DataRow dr in ds.Tables[0].Rows)
            {
                var typeKey = dr["vitalType"].ToString();
                var rangeKey = dr["vitalRange"].ToString();
                Double? value = Double.Parse(dr["value"].ToString());

                if (!ranges.ContainsKey(typeKey))
                {
                    var typeDict = new Dictionary<string, Double?> {
                        { Constants.RANGE_PANIC_LOW, null },
                        { Constants.RANGE_NORMAL_LOW, null },
                        { Constants.RANGE_NORMAL_HIGH, null },
                        { Constants.RANGE_PANIC_HIGH, null }
                    };

                    ranges.Add(typeKey, typeDict);
                }

                // There is no meaningful 'low' values for pain or 'high' values for O2, so they are set to null.
                if (typeKey.Equals("Pain") && rangeKey.ToLowerInvariant().IndexOf("low") > 0)
                {
                    value = null;
                } else if (typeKey.Equals("O2 Saturation") && rangeKey.ToLowerInvariant().IndexOf("high") > 0)
                {
                    value = null;
                }

                ranges[typeKey][rangeKey] = value;
            }

            return ranges;
        }

        // TODO: Move this into the context so these can be created for any color code as needed.
        private static void LoadStyleInfo()
        {
            if (styleInfo.Keys.Count > 0)
            {
                return;
            }

            var ds = new DB.Select
            {
                Sql = "SELECT pc_id, name, value1, value2 FROM [dbo].[lu_codes] where [type] = 'COLOR' AND pc_id IN('Y','R','X')"
            }.RunForDataSet();

            foreach(DataRow dr in ds.Tables[0].Rows)
            {
                var key = dr["pc_id"].ToString();
                var style = new Dictionary<string, string>
                {
                    { "ColorCode", key },
                    { "ColorName", dr["name"].ToString() },
                    { "ColorValue1", dr["value1"]?.ToString() },
                    { "ColorValue2", dr["value2"]?.ToString() }
                };

                styleInfo[key] = style;
            }
        }

        /// <summary>
        /// Remove dashes from vital values, but only if the value has both leading and trailing dashes. This allows negative values.
        /// </summary>
        /// <param name="vs">Vital sign value which may or may not contain dashes</param>
        /// <returns>Modified, dash-free (non-negative) or single-dash (negative) value</returns>
        public static string RemoveDashes(string vs)
        {
            vs = vs.Trim();
            while(vs.StartsWith("-") && vs.EndsWith("-"))
            {
                // handle the situation where the user only put in dashes for a value
                if (vs.Length < 2)
                    break;

                vs = vs.Substring(1, vs.Length - 2);
                vs = vs.Trim();
            }
            return vs;
        }

        /// <summary>
        /// Constants for Vital Signs
        /// </summary>
        public static class Constants
        {
            #region vital sign name/type identifiers
            /// <summary>
            /// Blood pressure
            /// </summary>
            public const string BP = "BP";

            /// <summary>
            /// Pulse
            /// </summary>
            public const string Pulse = "Pulse";

            /// <summary>
            /// Respiration
            /// </summary>
            public const string Respiration = "Respiration";

            /// <summary>
            /// Temperature
            /// </summary>
            public const string Temperature = "Temperature";

            /// <summary>
            /// Pain
            /// </summary>
            public const string Pain = "Pain";

            /// <summary>
            /// O2 Saturation
            /// </summary>
            public const string O2Sat = "O2 Saturation";

            /// <summary>
            /// Time
            /// </summary>
            public const string Time = "Time";

            /// <summary>
            /// MAP
            /// </summary>
            public const string MAP = "MAP";

            /// <summary>
            /// End-tidal
            /// </summary>
            public const string EndTidalCO2 = "End-Tidal CO2";
            #endregion

            /// <summary>
            /// Number of vital signs expected in core
            /// </summary>
            public const int EXPECTED_CORE_VITALS_COUNT = 10;

            #region range type identifiers
            /// <summary>
            /// Panic low range identifier
            /// </summary>
            public const string RANGE_PANIC_LOW = "Panic low";

            /// <summary>
            /// Normal low range identifier
            /// </summary>
            public const string RANGE_NORMAL_LOW = "Normal low";

            /// <summary>
            /// Normal high range identifier
            /// </summary>
            public const string RANGE_NORMAL_HIGH = "Normal high";

            /// <summary>
            /// Panic high range identifier
            /// </summary>
            public const string RANGE_PANIC_HIGH = "Panic high";
            #endregion

            #region range color codes
            /// <summary>
            /// Color code applied when vital falls above the panic high value
            /// </summary>
            public const string PANIC_HIGH_CODE = "R";

            /// <summary>
            /// Color code applied when the vital falls above the warn high value, but below the panic high value
            /// </summary>
            public const string WARN_HIGH_CODE = "Y";

            /// <summary>
            /// Color code applied when the vital falls below the panic low value
            /// </summary>
            public const string PANIC_LOW_CODE = "R";

            /// <summary>
            /// Color code applied when the vital falls below the warn low value, but above the panic low value
            /// </summary>
            public const string WARN_LOW_CODE = "Y";

            /// <summary>
            /// Color code applied when the vital falls within the normal range (between warn low and warn high)
            /// </summary>
            public const string NORMAL_CODE = "X";
            #endregion
        }
    }
}