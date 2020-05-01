using DomainModel;
using Interfaces.DomainModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle results stuff
    /// </summary>
    public class Results
    {

        // Results config
        public class Config
        {
            /// <summary>
            /// Path to configresults.xml
            /// </summary>
            public string FilePath { get; set; }

            /// <summary>
            /// XDocument representing the content stored in FilePath
            /// </summary>
            public XDocument ConfigInfo { get; set; }

            /// <summary>
            /// Create a new Results.Config instance using the provided path to configresults.xml
            /// </summary>
            /// <param name="configFilePath"></param>
            public Config(string configFilePath)
            {
                Init(configFilePath);
            }

            /// <summary>
            /// Create a new Results.Config instance using the provided ISite instance
            /// </summary>
            /// <param name="site">ISite instance</param>
            public Config(ISite site)
            {
                var root = site.Root;
                var configFilePath = root + "\\inc\\" + site.Id + "\\configresults.xml";
                if (!File.Exists(configFilePath))
                {
                    configFilePath = root + "\\inc\\1\\configresults.xml";
                }

                Init(configFilePath);
            }

            /// <summary>
            /// Initialize the Results.Config instance with the provided path to configresults.xml
            /// </summary>
            /// <param name="configFilePath">Path to configresults.xml</param>
            private void Init(string configFilePath)
            {
                if ((string.IsNullOrWhiteSpace(FilePath) || !FilePath.Equals(configFilePath)) && File.Exists(configFilePath))
                {
                    FilePath = configFilePath;
                    ConfigInfo = XDocument.Load(configFilePath);
                }
            }

            /// <summary>
            /// Retrieve a global settings entry
            /// </summary>
            /// <param name="key">Key name</param>
            /// <returns></returns>
            public string GetEntry(string key)
            {
                if (string.IsNullOrWhiteSpace(key) || ConfigInfo == null)
                {
                    return null;
                }

                foreach (var location in new string[] { "site", "default" })
                {
                    var node = from c in ConfigInfo.Root.Descendants(location)
                               where c.Attribute("name").Value.Equals("global_settings")
                               select c;
                    if (node != null)
                    {
                        var result = from c in node.Descendants("entry")
                                     where c.Attribute("name").Value.Equals(key)
                                     select c.Attribute("value").Value;
                        if (result != null && result.Count() > 0)
                        {
                            return result.First();
                        }
                    }
                }

                return null;
            }

            /// <summary>
            /// Retrieve a global settings entry with a subkey
            /// </summary>
            /// <param name="key">Key name</param>
            /// <param name="subKey">Sub key name</param>
            /// <returns></returns>
            public string GetEntry(string key, string subKey)
            {
                if (string.IsNullOrWhiteSpace(key) || ConfigInfo == null)
                {
                    return null;
                }

                foreach (var location in new string[] { "site", "default" })
                {
                    var node = from c in ConfigInfo.Root.Descendants(location)
                               where c.Attribute("name").Value.Equals("global_settings")
                               select c;
                    if (node != null)
                    {
                        var subNodes = from c in node.Descendants("entry")
                                       where c.Attribute("name").Value.Equals(key)
                                       select c;
                        if (subNodes != null)
                        {
                            var result = from c in subNodes.Descendants("entry_key")
                                             where c.Attribute("name").Value.Equals(subKey)
                                             select c.Attribute("value").Value;
                            if (result != null && result.Count() > 0)
                            {
                                return result.First();
                            }
                        }
                    }
                }

                return null;
            }

            /// <summary>
            /// Retrieve an HL7 settings entry. This method will first look at the
            /// site settings and if the key is not found, look at the default settings.
            /// </summary>
            /// <param name="key">Key name</param>
            /// <param name="subKey">Subkey name</param>
            /// <returns></returns>
            public string GetHl7Entry(string key, string subKey)
            {
                if (string.IsNullOrWhiteSpace(key) || ConfigInfo == null)
                {
                    return null;
                }

                foreach(var location in new string[] { "site", "default" })
                {
                    var node = from c in ConfigInfo.Root.Descendants(location)
                               where c.Attribute("name").Value.Equals("HL7_settings")
                               select c;
                    if (node != null)
                    {
                        var result = from c in node.Descendants("HL7_entry")
                                     where c.Attribute("name").Value.Equals(key)
                                     select c.Attribute(subKey).Value;
                        if (result != null && result.Count() > 0)
                        {
                            return result.First();
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Results levels
        /// </summary>
        public class Levels
        {
            /// <summary>
            /// Level/code information
            /// </summary>
            private Dictionary<string, string> Codes = new Dictionary<string, string>();

            /// <summary>
            /// Match expected result values
            /// </summary>
            private Regex numRE = new Regex(@"^-?\d+?\.?\d+$", RegexOptions.Compiled);

            /// <summary>
            /// Create a new Levels object
            /// </summary>
            /// <param name="codes">Dictionary of leve/code information</param>
            public Levels(Dictionary<string, string> codes)
            {
                Codes = codes;
            }

            /// <summary>
            /// Determine the appropriate level for a result flag, range, and result.
            /// </summary>
            /// <param name="data">TestField containing flag, range, and result data</param>
            /// <returns>Level string or null if none.</returns>
            public string GetLevel(ITestFields data)
            {
                if (data == null)
                {
                    return null;
                }

                if (data.Flag != null && Codes.ContainsKey(data.Flag.ToUpperInvariant()))
                {
                    return Codes[data.Flag.ToUpperInvariant()];
                }

                var range = data.Range ?? "";
                range = range.Replace(" ", "").ToUpperInvariant();
                var result = data.Result ?? "";
                result = result.Replace(" ", "").ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(range) || string.IsNullOrWhiteSpace(result))
                {
                    return null;
                }

                if (Constants.NON_NUM_NEG.ContainsKey(range))
                {
                    return (Constants.NON_NUM_NEG.ContainsKey(result) ? null : Constants.ABNORMAL_RESULT_FLAG);
                }
                else if (Constants.NON_NUM_POS.ContainsKey(range))
                {
                    return (Constants.NON_NUM_POS.ContainsKey(result) ? null : Constants.ABNORMAL_RESULT_FLAG);
                }
                else if (numRE.IsMatch(result))
                {
                    if (range.Substring(0, 1).Equals("<"))
                    {
                        var max = range.Substring(1);
                        if (numRE.IsMatch(max) && double.Parse(result) > double.Parse(max))
                        {
                            return Constants.HIGH_CRITICAL_RESULT_FLAG;
                        }
                    }
                    else if (range.Substring(0, 1).Equals(">"))
                    {
                        var min = range.Substring(1);
                        if (numRE.IsMatch(min) && double.Parse(result) < double.Parse(min))
                        {
                            return Constants.LOW_CRITICAL_RESULT_FLAG;
                        }
                    }
                    else
                    {
                        var parts = range.Split(new char[] { '-' });
                        var min = parts[0];
                        var max = parts[1];
                        if (min.Length > 0 && max.Length > 0)
                        {
                            if (numRE.IsMatch(max) && double.Parse(result) > double.Parse(max))
                            {
                                return Constants.HIGH_CRITICAL_RESULT_FLAG;
                            }
                            if (numRE.IsMatch(min) && double.Parse(result) < double.Parse(min))
                            {
                                return Constants.LOW_CRITICAL_RESULT_FLAG;
                            }
                        }
                    }
                }

                return null;
            }

            /// <summary>
            /// Get the class name for a particular result TestFields set
            /// </summary>
            /// <param name="level">ITestFields instance with flag, range, and result data</param>
            /// <returns>Class name string</returns>
            public string GetStyleClass(ITestFields level)
            {
                var l = GetLevel(level);
                return GetStyleClass(l);
            }

            /// <summary>
            /// Get the class name for a particular result level
            /// </summary>
            /// <param name="level"></param>
            /// <returns>Class name string</returns>
            public string GetStyleClass(string level)
            {
                return Constants.RESULTS_CLASSES.ContainsKey(level) ?
                    Constants.RESULTS_CLASSES[level] : null;
            }

            /// <summary>
            /// Constants used in results levels
            /// </summary>
            public static class Constants
            {
                // Flags for various types of results
                /// <summary>
                /// Abnormal result flag value
                /// </summary>
                public const string ABNORMAL_RESULT_FLAG = "a";

                /// <summary>
                /// Low critical result flag value
                /// </summary>
                public const string LOW_CRITICAL_RESULT_FLAG = "ll";

                /// <summary>
                /// Low result flag value
                /// </summary>
                public const string LOW_RESULT_FLAG = "l";

                /// <summary>
                /// High critical result flag value
                /// </summary>
                public const string HIGH_CRITICAL_RESULT_FLAG = "hh";

                /// <summary>
                /// High result flag value
                /// </summary>
                public const string HIGH_RESULT_FLAG = "h";

                /// <summary>
                /// Ranges/results for "negative"
                /// </summary>
                public static readonly Dictionary<string, int> NON_NUM_NEG = new Dictionary<string, int>
                {
                    { "NEG", 1 },
                    { "NEGATIVE", 1 },
                    { "N", 1 }
                };

                /// <summary>
                /// Ranges/results for "positive"
                /// </summary>
                public static readonly Dictionary<string, int> NON_NUM_POS = new Dictionary<string, int>
                {
                    { "POS", 1 },
                    { "POSITIVE", 1 },
                    { "P", 1 }
                };

                /// <summary>
                /// Standard abnormal style classes: maps level to style class
                /// </summary>
                public static readonly Dictionary<string, string> RESULTS_CLASSES = new Dictionary<string, string>
                {
                    { ABNORMAL_RESULT_FLAG, "results_abnormal" },
                    { LOW_CRITICAL_RESULT_FLAG, "results_low_critical" },
                    { LOW_RESULT_FLAG, "results_low" },
                    { HIGH_CRITICAL_RESULT_FLAG, "results_high_critical" },
                    { HIGH_RESULT_FLAG, "results_high" }
                };
            }
        }

        /// <summary>
        /// Write a 'Results viewed' Admin->Chart View entry to the patient's record
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="userId">User identifier</param>
        public static void AuditView(byte siteId, string patientId, int userId)
        {
            var _t = new Time(siteId);
            var viewLine = new EMR.Line
            {
                LineHeader = new EMR.Line.Header
                {
                    sys_time = _t.Timestamp(),
                    user = userId
                },
                LinePart = new EMR.Line.Part
                {
                    nct = EMR.Constants.NCT_CHART_VIEW,
                    section = EMR.Constants.SECT_ADMIN,
                    part = "CHART VIEW"
                },
                DataSegments = new List<EMR.Line.DataSegment> {
                    new EMR.Line.DataSegment(EMR.Line.DataSegment.Constants.TYPE_CHECKBOX, "Results viewed")
                }
            };

            var chart = new EMR(siteId, patientId, true);
            chart.WriteLine(viewLine, userId);
        }

        /// <summary>
        /// Get a dictionary of abnormal code information for results
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <returns>Abnormal code information dictionary</returns>
        public static Dictionary<string, string> GetAbnormalCodes(byte siteId)
        {
            var abnormalCodes = new Dictionary<string, string>();
            var codesResult = new DB.Select
            {
                Sql = "SELECT id, misc FROM idx WHERE site=@site AND status=@status AND type=@type",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                    new SqlParameter("@status", SqlDbType.Char) { Value = "A" },
                    new SqlParameter("@type", SqlDbType.Char) { Value = "BA" }
                }
            }.RunForDataSet();
            if (codesResult != null)
            {
                foreach (DataRow c in codesResult.Tables[0].Rows)
                {
                    abnormalCodes[c["id"].ToString().TrimEnd().ToUpperInvariant()] = c["misc"].ToString().TrimEnd();
                }
            }

            return abnormalCodes;
        }

        /// <summary>
        /// Get information about results that have been posted to a patient's chart
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patient">IPatient instance</param>
        /// <returns>Dictionary of posted result information</returns>
        public static Dictionary<int, string> GetPostedResults(byte siteId, IPatient patient)
        {
            var postedResults = new Dictionary<int, string>();
            if (patient != null)
            {
                // Figure out which results have already been posted to the chart
                var posted = new DB.Select
                {
                    Sql = "SELECT line_num, order_number FROM ord_results_post WHERE site=@site AND ibex=@ibex",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                        new SqlParameter("@ibex", SqlDbType.VarChar) { Value = patient.Ibex }
                    }
                }.RunForDataSet();
                if (posted != null)
                {
                    foreach (DataRow dr in posted.Tables[0].Rows)
                    {
                        postedResults[Convert.ToInt32(dr["line_num"].ToString())] = dr["order_number"]?.ToString();
                    }
                }
            }

            return postedResults;
        }

        /// <summary>
        /// Generate the bulk of the DataSegments required for a results entry in the chart
        /// </summary>
        /// <param name="result">Result line</param>
        /// <param name="testType">Result type</param>
        /// <param name="abnormal">Boolean abnormal result flag</param>
        /// <param name="useTable">Boolean for whether results should be printed in table format</param>
        /// <param name="spaceLen">Max length of the result before wrapping</param>
        /// <param name="test">Test value from result</param>
        /// <param name="units">Units value from result</param>
        /// <returns></returns>
        public static List<EMR.Line.DataSegment> GetResultMarkup(string result, string testType, bool abnormal, bool useTable, int spaceLen, string test, string units)
        {
            var segments = new List<EMR.Line.DataSegment>();
            if (testType.Equals("LAB") && useTable)
            {
                var lines = result.Split(new string[] { "<LF>" }, StringSplitOptions.None);
                var count = 1;
                foreach(var resultLine in lines)
                {
                    if(resultLine.Length > spaceLen)
                    {
                        var tempLine = WrapText(resultLine, spaceLen);
                        var parts = tempLine.Split(new char[] { '~' });
                        foreach(var part in parts)
                        {
                            segments.AddRange(GetTableRow(test, part, units, abnormal));
                            if (++count > 1)
                            {
                                test = units = "";
                            }
                        }
                    } else
                    {
                        segments.AddRange(GetTableRow(test, resultLine, units, abnormal));
                    }
                    if (++count > 1)
                    {
                        test = units = "";
                    }
                }
            } else
            {
                var value = test + "  " + result + " " + units;
                if (abnormal)
                {
                    value = "*" + value;
                }
                segments.Add(GetTableText("\n" + value));
            }

            return segments;
        }

        /// <summary>
        /// Get a table cell data segment with the provided text
        /// </summary>
        /// <param name="cellText">Text to place inside the cell</param>
        /// <returns>New EMR.Line.DataSegment</returns>
        public static EMR.Line.DataSegment GetTableCellDataSegment(string cellText)
        {
            return new EMR.Line.DataSegment(EMR.Line.DataSegment.Constants.TYPE_TABLE_CELL, cellText);
        }

        /// <summary>
        /// Get a list of DataSegments for generating a 3-column table row
        /// </summary>
        /// <param name="col1">Column 1 value</param>
        /// <param name="col2">Column 2 value</param>
        /// <param name="col3">Column 3 value</param>
        /// <param name="abnormal">Boolean flag for whether this row should should be abnormal</param>
        /// <returns></returns>
        public static List<EMR.Line.DataSegment> GetTableRow(string col1, string col2, string col3, bool abnormal)
        {
            List<EMR.Line.DataSegment> segments = new List<EMR.Line.DataSegment>();
            var headerSegment = new EMR.Line.DataSegment();
            var type = EMR.Line.DataSegment.Constants.TYPE_TABLE;
            if (abnormal)
            {
                type += EMR.Line.DataSegment.Constants.MODIFIER_TABLE_ABNORMAL;
            }
            headerSegment.Type = type;
            segments.AddRange(new List<EMR.Line.DataSegment> {
                headerSegment,
                GetTableCellDataSegment(col1),
                GetTableCellDataSegment(col2),
                GetTableCellDataSegment(col3)
            });

            var value = col1 + " " + col2 + " " + col3;
            var segment = new EMR.Line.DataSegment();
            if (abnormal)
            {
                segment.Type = EMR.Line.DataSegment.Constants.TYPE_TEXT;
                segment.ValueSegments.abnormal = EMR.Line.DataSegment.Constants.ABNORMAL;
                value = "*" + value;
            } else
            {
                segment.Type = "D";
            }
            segment.value = new EMR.Line.DataSegment.Value
            {
                value = "\n" + value
            };
            segments.Add(segment);

            return segments;
        }

        /// <summary>
        /// Get a "table start" data segment for writing results to the chart
        /// </summary>
        /// <returns>New EMR.Line.DataSegment</returns>
        public static EMR.Line.DataSegment GetTableStartDataSegment()
        {
            return new EMR.Line.DataSegment(EMR.Line.DataSegment.Constants.TYPE_TABLE, "");
        }

        /// <summary>
        /// Generate a ^D DataSegment with a given value
        /// </summary>
        /// <param name="val">Text value</param>
        /// <param name="startOfRow">Boolean for for whether this is the start of a row</param>
        /// <returns>New EMR.Line.DataSegment</returns>
        public static EMR.Line.DataSegment GetTableText(string val, bool startOfRow = false)
        {
            var segment = new EMR.Line.DataSegment("D", val);
            segment.KeepTrailingDelimiter = startOfRow;

            return segment;
        }

        /// <summary>
        /// Store the results line number and order number in the database when it gets posted to the chart
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="lineNum">Result line number</param>
        /// <param name="orderNum">Result order number</param>
        /// <returns>Boolean flag for success/failure</returns>
        public static bool StoreResultsPostChart(byte siteId, string patientId, int lineNum, string orderNum)
        {
            var res = new DB.Insert
            {
                Sql = "INSERT INTO ord_results_post (site, ibex, line_num, order_number) VALUES (@site, @ibex, @line_num, @order_number)",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId },
                    new SqlParameter("@line_num", SqlDbType.Int) { Value = lineNum },
                    new SqlParameter("@order_number", SqlDbType.VarChar) { Value = orderNum ?? "" }
                }
            }.Run();

            return (res > 0);
        }

        /// <summary>
        /// Wrap content being written to the chart
        /// </summary>
        /// <param name="inLine">String to write to the chart</param>
        /// <param name="maxLine">Maximum line length</param>
        /// <returns>Wrapped content</returns>
        public static string WrapText(string inLine, int maxLine)
        {
            var indent = "  ";
            var rx = 0;
            var tempString = "";
            while(inLine.Length > maxLine)
            {
                var substrOffset = 1;
                rx = inLine.LastIndexOf(' ', maxLine);
                if (rx < 0)
                {
                    rx = maxLine - 2;
                    substrOffset = 0;
                }

                var tempString2 = inLine.Substring(rx + substrOffset);
                tempString = tempString + inLine.Substring(0, rx) + "~" + indent;
                inLine = tempString2;
            }

            tempString = tempString + inLine;
            return tempString;
        }
    }
}