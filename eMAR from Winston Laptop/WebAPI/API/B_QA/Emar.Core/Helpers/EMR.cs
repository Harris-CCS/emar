using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
//using PulseCheck.IDomain;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to handle EMR access
    /// </summary>
    public class EMR : IEMRUtility
    {
        #region EMR Properties
        /// <summary>
        /// Site identifier for this EMR
        /// </summary>
        public byte SiteId { get; private set; }

        /// <summary>
        /// Patient identifier for this EMR
        /// </summary>
        public string Ibex { get; private set; }

        /// <summary>
        /// Path to the EMR file
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Flag for whether this object is used only for writing to the chart
        /// </summary>
        public bool ForWriting { get; set; }

        /// <summary>
        /// Flag for whether this object should keep the raw chart lines after reading
        /// </summary>
        public bool KeepRawLines { get; set; }

        /// <summary>
        /// Flag for whether this object represents a historical patient's chart
        /// </summary>
        public bool Historical { get; set; }

        /// <summary>
        /// Users who made entries (either active or inactive) in this EMR
        /// </summary>
        public Dictionary<int, int> Users { get; private set; }

        /// <summary>
        /// NCT numbers present in this EMR
        /// </summary>
        public Dictionary<int, int> NCTs { get; private set; }

        /// <summary>
        /// Lines of this EMR
        /// </summary>
        public List<Line> Lines { get; private set; }

        /// <summary>
        /// Current read position within EMR lines
        /// </summary>
        private int EMRIndex = -1;

        /// <summary>
        /// List of raw lines from the EMR
        /// </summary>
        private List<string> RawLines = new List<string>();

        /// <summary>
        /// List of DB field names
        /// </summary>
        private static List<string> DBFields = new List<string>
        {
            Constants.SystemTime,
            Constants.User,
            Constants.Losecs,
            Constants.Level,
            Constants.Status,
            Constants.DocId,
            Constants.NoLink,
            Constants.InactiveTime,
            Constants.InactiveUser,
            Constants.TableXRef,
            Constants.Audio,
            Constants.ChartXRef,
            Constants.UserTime,
            Constants.NCT,
            Constants.Section,
            Constants.Part,
            Constants.Data,
            Constants.DataSource,
        };

        /// <summary>
        /// Used to match and replace $$user$$ placeholder in seen time part
        /// </summary>
        private static Regex userReplace = new Regex(@"\$\$user\$\$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Used to match and replace $$time$$ placeholder in seen time part
        /// </summary>
        private static Regex dtReplace = new Regex(@"\$\$time\$\$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Map user identifiers to user initials
        /// </summary>
        private Dictionary<Int32, string> Inits = new Dictionary<Int32, string>();
        #endregion

        /// <summary>
        /// Create a new EMR object for the given patient at the given site.
        /// </summary>
        /// <param name="site">Site Identifier</param>
        /// <param name="patientId">Patient Identifier</param>
        /// <param name="forWriting">Flag for whether this object will be used only for writing to the chart</param>
        /// <param name="keepRawLines">Flag for whether this object should keep the raw EMR lines</param>
        /// <param name="historical">Flag for whether this object is being used for a historical patient</param>
        public EMR(byte site, string patientId, bool forWriting = false, bool keepRawLines = false, bool historical = false)
        {
            ResetIndex();
            SiteId = site;
            Ibex = patientId;
            ForWriting = forWriting;
            KeepRawLines = keepRawLines;
            Historical = historical;

            NCTs = new Dictionary<int, int>();
            Users = new Dictionary<int, int>();
            Lines = new List<Line>();
            RawLines = new List<string>();

            if (!forWriting)
            {
                LoadEMR();
                ResetIndex();
            }
        }

        /// <summary>
        /// Add a list of clinical identifiers to the database
        /// </summary>
        /// <param name="ids"></param>
        public void AddClinicalIdentifiers(List<ClinicalId> ids)
        {
            var connection = DB.GetConnectionString();
            using (var con = new SqlConnection(connection))
            {
                foreach (ClinicalId c in ids)
                {
                    c.Add(con);
                }
            }
        }

        /// <summary>
        /// Determine whether a particular nct and part name combination requires a signature
        /// </summary>
        /// <param name="nct">NCT number</param>
        /// <param name="partName">Part name</param>
        /// <returns>Boolean flag for whether a signature is required</returns>
        public static bool NoSignatureRequired(int nct, string partName)
        {
            if (nct == EMR.Constants.NCT_ADMISSION_REQUEST || nct == EMR.Constants.NCT_CHART_VIEW || nct == EMR.Constants.NCT_IMAGING ||
                nct == EMR.Constants.NCT_MED_ADMIN || nct == EMR.Constants.NCT_ORDER_DETAILS || nct == EMR.Constants.NCT_PROBLEM_LIST ||
                (nct == EMR.Constants.NCT_EVENTS && partName.Equals("TRANSFER")))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert a line's data segment to an integer.
        /// </summary>
        /// <remarks>This assumes the developer knows that the data *should be* an integer. It would not be wise to run this on most data segments, since all non-numerics are stripped and the resulting number is returned. This was added to handle a pre-existing PulseCheck issue that was writing invalid line inactivation entries to the chart when inactivating allergies.</remarks>
        /// <param name="lineData">The EMR Line's data segment value</param>
        /// <returns>An int resulting from removing all non-numeric characters from the string and converting it to an int</returns>
        private int ConvertDataToInt(string lineData)
        {
            lineData = lineData ?? string.Empty;
            return Convert.ToInt32(new string(lineData.Where(p => char.IsDigit(p)).ToArray()));
        }

        /// <summary>
        /// Take a seen time info value and format it by replacing placeholder values with their actual values
        /// </summary>
        /// <param name="value">Seen time info value string</param>
        /// <param name="user">User object</param>
        /// <param name="currentDateTime">Current date time string</param>
        /// <returns>Formatted value</returns>
        ////public static string FormatSeenTimeInfo(string value, IUser user, string currentDateTime)
        ////{
        ////    var userName = user.LastName + ", " + user.FirstName;
        ////    value = userReplace.Replace(value, userName);
        ////    value = dtReplace.Replace(value, currentDateTime);
        ////    return value;
        ////}

        /// <summary>
        /// Get the current read position of this EMR
        /// </summary>
        /// <returns>int for the current line number of the EMR</returns>
        public int GetCurrentIndex()
        {
            return EMRIndex;
        }

        private Line Inactivation(string sysTime, int user, int lineNumber)
        {
            return new Line
            {
                LineHeader = new Line.Header
                {
                    sys_time = sysTime,
                    user = user,
                },
                LinePart = new Line.Part
                {
                    nct = Constants.NCT_INACTIVE,
                },
                Data = lineNumber.ToString()
            };
        }

        /// <summary>
        /// Load EMR entries into this object.
        /// NOTE: Added support for patient charts in the hst table.
        /// </summary>
        /// <returns></returns>
        private bool LoadEMR()
        {
//            var emarCall = Assembly.GetCallingAssembly().Location.Contains("Emar.Api"); // added during investigation
            var connection = DB.GetConnectionString();
            using (var con = new SqlConnection(connection))
            {
                var procedure = Historical ? "[dbo].pc_retrieve_lines_from_archive_chart" : "[dbo].pc_retrieve_lines_from_active_chart";
                using (SqlCommand cmd = new SqlCommand(procedure, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ibex", SqlDbType.Char, 14).Value = Ibex;
                    cmd.Parameters.Add("@site", SqlDbType.Int).Value = SiteId;
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        int i = 0;
                        while (reader.Read())
                        {
                            var EMRLine = new Line(i)
                            {
                                // TODO: Might want to convert Data to DataSegments here.
                                Data = reader[Constants.Data]?.ToString(),
                                LinePart = new Line.Part()
                                {
                                    nct = Convert.ToInt32(reader[Constants.NCT]),
                                    section = reader[Constants.Section]?.ToString() ?? "",
                                    part = reader[Constants.Part]?.ToString() ?? ""
                                },
                                LineHeader = new Line.Header()
                                {
                                    audio = reader[Constants.Audio] != DBNull.Value ? Encoding.ASCII.GetString((byte [])reader[Constants.Audio]) : "",
                                    chart_xref = reader[Constants.ChartXRef]?.ToString() ?? "",
                                    doc_id = reader[Constants.DocId]?.ToString() ?? "",
                                    inactive_time = reader[Constants.InactiveTime]?.ToString() ?? "",
                                    inactive_user = (reader[Constants.InactiveUser] != DBNull.Value ? (int)reader[Constants.InactiveUser] : 0),
                                    level = reader[Constants.Level]?.ToString() ?? "",
                                    losecs = reader[Constants.Losecs]?.ToString() ?? "",
                                    no_link = reader[Constants.NoLink]?.ToString() ?? "",
                                    status = reader[Constants.Status]?.ToString() ?? "",
                                    sys_time = reader[Constants.SystemTime]?.ToString() ?? "",
                                    table_xref = reader[Constants.TableXRef]?.ToString() ?? "",
                                    user = (reader[Constants.User] != DBNull.Value ? (int)reader[Constants.User] : 0),
                                    user_time = reader[Constants.UserTime]?.ToString() ?? ""
                                }
                            };

                            var sourceLine = EMRLine.Clone();
                            var nct = EMRLine.NCT();

                            // If an entry indicates that a previous entry is inactive, appropriately mark that previous entry.
                            if (nct == Constants.NCT_INACTIVE)
                            {
                                if (!String.IsNullOrEmpty(EMRLine.Data))
                                {
                                    var inactiveLineNumber = ConvertDataToInt(EMRLine.Data);
                                    var lineToInactivate = Lines[inactiveLineNumber];
                                    if (lineToInactivate != null)
                                    {
                                        lineToInactivate.LineHeader.status = Constants.INACTIVE;
                                        lineToInactivate.LineHeader.inactive_user = Convert.ToInt32(EMRLine.LineHeader.get(Constants.User));
                                        lineToInactivate.LineHeader.inactive_time = EMRLine.LineHeader.get(Constants.SystemTime)?.ToString() ?? "";
                                    }
                                }

                                // At some point we changed "HISTORY" to "PAST MEDICAL HISTORY". Make sure that all sections marked
                                // as PMH (by nct number) have a section that is named "PAST MEDICAL HISTORY"
                            }
                            else
                            {
                                if (nct == Constants.NCT_PAST_MEDICAL_HISTORY)
                                    EMRLine.LinePart.section = Constants.SECT_PAST_MEDICAL_HISTORY;

                                EMRLine.LineNumber = i;
                                i++;
                                Lines.Add(EMRLine);
                                if (KeepRawLines)
                                    RawLines.Add(sourceLine.ToString());
                            }

                            // For digital signature, the user time should always be the time that the signature was requested, not the time to process the request.
                            if (nct == Constants.NCT_DIG_SIG && EMRLine.Data.IndexOf("SIGNATURE PENDING") < 0)
                            {
                                for (int j = Lines.Count - 1; j > 0; j--)
                                {
                                    var line = Lines[j];
                                    if (line.NCT() == Constants.NCT_DIG_SIG &&
                                        line.LineHeader.get(Constants.User).Equals(EMRLine.LineHeader.get(Constants.User)) &&
                                        line.Data.IndexOf("SIGNATURE PENDING") > 0
                                    )
                                    {
                                        EMRLine.LineHeader.user_time = line.LineHeader.get(Constants.UserTime)?.ToString() ?? "";
                                        break;
                                    }
                                }
                            }

                            // Save distinct users who made or inactivated entries in this EMR
                            var user = Convert.ToInt32(EMRLine.LineHeader.get(Constants.User));
                            if (!Users.ContainsKey(user))
                                Users.Add(user, user);

                            var inactiveUser = EMRLine.LineHeader.get(Constants.InactiveUser);
                            if (!String.IsNullOrEmpty(inactiveUser))
                            {
                                var inactiveUserInt = Convert.ToInt32(EMRLine.LineHeader.get(Constants.InactiveUser));
                                if (!Users.ContainsKey(inactiveUserInt))
                                    Users.Add(inactiveUserInt, inactiveUserInt);
                            }

                            // Save distinct NCT numbers present in this EMR
                            if (!NCTs.ContainsKey(nct))
                                NCTs.Add(nct, nct);
                        }

                        reader.Close();

                        LookupUsers();

                        // TODO here later: Add code to make certain sections display "No Recorded xxxxxxx" if no information is present.
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Look up and store all initials for users who are associated with this EMR
        /// </summary>
        private void LookupUsers()
        {
            if (Users.Count == 0)
            {
                return;
            }

            var sqlParams = new List<SqlParameter>();
            var sql = new StringBuilder("SELECT num, init FROM drs WHERE num IN(");
            var paramNameList = new List<string>();
            int i = 1;
            foreach (int id in Users.Keys)
            {
                var paramName = "@p" + i;
                paramNameList.Add(paramName);
                sqlParams.Add(new SqlParameter(paramName, SqlDbType.Int) { Value = id });
                i++;
            }
            sql.Append(String.Join(",", paramNameList));
            sql.Append(")");

            var result = new DB.Select
            {
                Sql = sql.ToString(),
                Parameters = sqlParams.ToArray()
            }.RunForDataSet();

            foreach (DataRow dr in result.Tables[0].Rows)
            {
                Inits.Add(Convert.ToInt32(dr["num"]), dr["init"].ToString());
            }
        }

        /// <summary>
        /// Get the initials of the user for this line
        /// </summary>
        /// <returns>User initials</returns>
        public string UserInitials(Int32 userId)
        {
            string ret = "";

            if (Inits.Count == 0)
            {
                LookupUsers();
            }

            if (Inits.ContainsKey(userId))
            {
                ret = Inits[userId];
            }

            return ret;
        }

        /// <summary>
        /// Remove a list of clinical identifiers from the database
        /// </summary>
        /// <param name="ids"></param>
        public void RemoveClinicalIdentifiers(List<ClinicalId> ids)
        {
            var connection = DB.GetConnectionString();
            using (var con = new SqlConnection(connection))
            {
                foreach (ClinicalId c in ids)
                {
                    c.Remove(con);
                }
            }
        }

        /// <summary>
        /// Reset the current read position of this EMR
        /// </summary>
        public void ResetIndex()
        {
            EMRIndex = -1;
        }

        #region simple chart line writers
        /// <summary>
        /// Write a line to the chart file
        /// </summary>
        /// <param name="chartLine">Line to write to chart file</param>
        /// <param name="userId">ID of user writing this line. Pulled from line if not provided.</param>
        /// <returns>Boolean success flag</returns>
        public bool WriteLine(Line chartLine, int userId = 0)
        {
            return WriteLines(new object[] { chartLine }, userId);
        }

        /// <summary>
        /// Write a line to the chart file
        /// </summary>
        /// <param name="chartLine">Line to write to the chart file</param>
        /// <param name="userId">ID of user writing this line. Pulled from line if not provided.</param>
        /// <returns>Boolean success flag</returns>
        public bool WriteLine(string chartLine, int userId = 0)
        {
            return WriteLines(new object[] { chartLine }, userId);
        }

        /// <summary>
        /// Write an inactivation entry to the chart file
        /// </summary>
        /// <param name="inactivateLineNumber"></param>
        /// <param name="userId">ID of user writing this line</param>
        /// <returns></returns>
        public bool WriteLine(int inactivateLineNumber, int userId)
        {
            return WriteLines(new object[] { inactivateLineNumber }, userId);
        }
        #endregion

        /// <summary>
        /// Write multiple lines to the chart file
        /// </summary>
        /// <param name="chartLines">Lines to write to chart file</param>
        /// <param name="userId">ID of user writing these lines. Pulled from lines if not provided.</param>
        /// <returns>Boolean success flag</returns>
        public bool WriteLines(object[] chartLines, int userId = 0)
        {
            var _t = new Time();
            var now = _t.Timestamp();
            bool wroteAllLines = true;

            var connection = DB.GetChartingConnectionString();
            using (var con = new SqlConnection(connection))
            {
                List<Line> addLines = new List<Line>();
                List<Line> removeLines = new List<Line>();
                List<ClinicalId> addClinicalIds = new List<ClinicalId>();
                List<ClinicalId> removeClinicalIds = new List<ClinicalId>();

                var procedure = Historical ? "pc_append_line_to_archive_chart" : "pc_append_line_to_active_chart";
                var sqlParamNames = new List<string> { "@ibex", "@site" };
                foreach (string name in DBFields)
                {
                    sqlParamNames.Add("@" + name);
                }
                var appendSql = "EXECUTE ibex.." + procedure + " " + String.Join(",", sqlParamNames);

                con.Open();
                var transaction = con.BeginTransaction("chart_write");

                var numRE = new Regex(@"^\d+$", RegexOptions.Compiled);
                foreach (object o in chartLines)
                {
                    Line writeLine = new Line();
                    bool inactivating = false;
                    if (o.GetType() == typeof(Line))
                    {
                        writeLine = (Line)o;
                    }
                    else if (o.GetType() == typeof(string))
                    {
                        if (numRE.IsMatch(o.ToString()))
                        {
                            var chartLineNumber = int.Parse(o.ToString());
                            writeLine = Inactivation(now, userId, chartLineNumber);

                            if (RawLines.Count > 0)
                            {
                                var lineToRemove = new Line(RawLines[chartLineNumber]);
                                inactivating = true;
                                foreach (Line.DataSegment ds in lineToRemove.DataSegments)
                                {
                                    if (String.IsNullOrEmpty(ds.NameSegments.clinical_id))
                                    {
                                        continue;
                                    }

                                    removeClinicalIds.Add(new ClinicalId
                                    {
                                        Id = ds.NameSegments.clinical_id,
                                        Site = SiteId,
                                        Ibex = Ibex,
                                        DrsNum = lineToRemove.User(),
                                        SysTime = lineToRemove.SysTime(),
                                        HName = lineToRemove.PartName(),
                                        TName = lineToRemove.SectionName(),
                                        NCTNum = lineToRemove.NCT()
                                    });
                                }
                            }
                        }
                        else
                        {
                            writeLine = new Line(o.ToString());
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Chart line of type " + o.GetType().ToString() + " is not supported for writing");
                    }

                    var writeLineNCT = writeLine.NCT();
                    if (inactivating && writeLineNCT != Constants.NCT_FLOWSHEET && writeLineNCT != Constants.NCT_FLOWSHEET_VS)
                    {
                        foreach (Line.DataSegment ds in writeLine.DataSegments)
                        {
                            if (String.IsNullOrEmpty(ds.NameSegments.clinical_id))
                            {
                                continue;
                            }

                            addClinicalIds.Add(new ClinicalId
                            {
                                Id = ds.NameSegments.clinical_id,
                                Site = SiteId,
                                Ibex = Ibex,
                                DrsNum = writeLine.User(),
                                Label = ds.ChartWriter().Substring(1),
                                Value = ds.ValueSegments.value,
                                SysTime = writeLine.SysTime(),
                                UserTime = writeLine.UserTime(),
                                HName = writeLine.PartName(),
                                TName = writeLine.SectionName(),
                                NCTNum = writeLine.NCT()
                            });
                        }
                    }

                    // Make sure the data segment is correct, and write the line.
                    var clonedLine = writeLine.Clone();
                    if (!writeLine.ForSigning)
                    {
                        var delimiter = ((clonedLine.NCT() == Constants.NCT_FLOWSHEET_VS) ? Line.DataSegment.Constants.DELIMITER : Constants.DelimiterData).ToString();
                        var newData = clonedLine.Data;
                        if (clonedLine.DataSegments != null && clonedLine.DataSegments.Count > 0)
                        {
                            newData = String.Join(delimiter, clonedLine.DataSegments);
                        }
                        clonedLine.Data = newData;
                    }

                    var sqlParameters = new SqlParameter[]
                    {
                            new SqlParameter("@ibex", SqlDbType.Char) { Value = Ibex },
                            new SqlParameter("@site", SqlDbType.Int)  { Value = SiteId },
                            new SqlParameter("@sys_time", SqlDbType.Char) { Value = clonedLine.SysTime() },
                            new SqlParameter("@usr", SqlDbType.Int) { Value = clonedLine.User() },
                            new SqlParameter("@losecs", SqlDbType.VarChar) { Value = DB.NullParam(clonedLine.Losecs()) },
                            new SqlParameter("@lvl", SqlDbType.VarChar) { Value = DB.NullParam(clonedLine.Level()) },
                            new SqlParameter("@status", SqlDbType.VarChar) { Value = DB.NullParam(clonedLine.Status()) },
                            new SqlParameter("@doc_id", SqlDbType.VarChar) { Value = DB.NullParam(clonedLine.DocId()) },
                            new SqlParameter("@no_link", SqlDbType.TinyInt) { Value = DB.NullParam(clonedLine.NoLink() ? 1 : 0) },
                            new SqlParameter("@inactive_time", SqlDbType.Char) { Value = DB.NullParam(clonedLine.InactiveTime()) },
                            new SqlParameter("@inactive_user", SqlDbType.Int) { Value = DB.NullParam(clonedLine.InactiveUser()) },
                            new SqlParameter("@table_xref", SqlDbType.Int) { Value = DB.NullParam(clonedLine.TableXRef()) },
                            new SqlParameter("@audio", SqlDbType.VarBinary) { Value = DB.NullParam(Encoding.ASCII.GetBytes(clonedLine.Audio())) },
                            new SqlParameter("@chart_xref", SqlDbType.VarChar) { Value = DB.NullParam(clonedLine.ChartXRef()) },
                            new SqlParameter("@user_time", SqlDbType.Char) { Value = (object)(clonedLine.UserTime() != clonedLine.SysTime() ? clonedLine.UserTime() : null) ?? DBNull.Value },
                            new SqlParameter("@nct", SqlDbType.Int) { Value = clonedLine.NCT() },
                            new SqlParameter("@section", SqlDbType.VarChar) { Value = clonedLine.SectionName() },
                            new SqlParameter("@part", SqlDbType.VarChar) { Value = clonedLine.PartName() },
                            new SqlParameter("@data", SqlDbType.VarChar) { Value = clonedLine.Data },
                            new SqlParameter("@data_source", SqlDbType.Char) { Value = Constants.DATA_SOURCE_EMAR },
                    };

                    try
                    {
                        var result = new DB.Update
                        {
                            Sql = appendSql,
                            Parameters = sqlParameters,
                            Connection = con,
                            Transaction = transaction
                        }.Run();

                        if (result != 1)
                        {
                            transaction.Rollback();
                            DTFL.Write(SiteId, userId, "Chart line append failed", appendSql, sqlParameters);
                            wroteAllLines = false;
                            return false;
                        }
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        DTFL.Write(SiteId, userId, ex, appendSql, sqlParameters);
                        wroteAllLines = false;
                        return false;
                    }
                }

                // TODO: Perl code seems to run this even if line writing fails. Is that true, and if yes, should we do the same?
                RemoveClinicalIdentifiers(removeClinicalIds);
                AddClinicalIdentifiers(addClinicalIds);

                if (wroteAllLines)
                {
                    transaction.Commit();
                }

                con.Close();
            }

            return wroteAllLines;
        }

        /// <summary>
        /// Class representing a single line from the EMR
        /// </summary>
        public class Line : ICloneable
        {
            #region EMR.Line properties
            /// <summary>
            /// List defining the identifiers and order of header segments
            /// </summary>
            public List<string> HeaderSegments = new List<string>()
            {
                Constants.SystemTime,
                Constants.User,
                Constants.Losecs,
                Constants.Level,
                Constants.Status,
                Constants.DocId,
                Constants.NoLink,
                Constants.InactiveTime,
                Constants.InactiveUser,
                Constants.TableXRef,
                Constants.Audio,
                Constants.ChartXRef,
                Constants.UserTime
            };

            /// <summary>
            /// List defining the identifiers and order of part segments
            /// </summary>
            public List<string> PartSegments = new List<string>()
            {
                Constants.NCT,
                Constants.Section,
                Constants.Part
            };

            /// <summary>
            /// Original line that this EMRLine was created from
            /// </summary>
            public string SourceLine { get; set; }

            /// <summary>
            /// EMR Line Header
            /// </summary>
            public Header LineHeader { get; set; } = new Header();

            /// <summary>
            /// EMR Line Part
            /// </summary>
            public Part LinePart { get; set; } = new Part();

            /// <summary>
            /// Data string from EMR Line
            /// </summary>
            public string Data { get; set; } = "";

            /// <summary>
            /// Data segments from EMR Line
            /// </summary>
            public List<DataSegment> DataSegments { get; set; } = new List<DataSegment>();

            /// <summary>
            /// Line number of EMR line
            /// </summary>
            public int LineNumber { get; set; }

            /// <summary>
            /// Database ID for the EMR line
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Flag to specify whether this line is for signing the chart
            /// </summary>
            public bool ForSigning { get; set; }
            #endregion

            #region EMR.Line constructors
            /// <summary>
            /// Create a new EMR Line object
            /// </summary>
            public Line()
            {

            }

            /// <summary>
            /// Create a new EMR Line object with a provided line number
            /// </summary>
            public Line(int lineNumber)
            {
                LineNumber = lineNumber;
            }

            /// <summary>
            /// Create a new EMR Line object from a provided string
            /// </summary>
            /// <param name="line">EMR line string</param>
            public Line(string line)
            {
                SourceLine = line.TrimEnd(new char[] { ' ', '\n', '\r' });
                var linePieces = SourceLine.Split(Constants.DelimiterLine);
                var headerPieces = linePieces[0].Split(Constants.DelimiterHeader);

                LineHeader = new Header()
                {
                    sys_time = headerPieces[0],
                    user = Convert.ToInt32(headerPieces[1]),
                    losecs = headerPieces[2],
                    level = headerPieces[3],
                    status = headerPieces[4],
                    doc_id = headerPieces[5],
                    no_link = headerPieces[6],
                    inactive_time = headerPieces[7],
                    inactive_user = Convert.ToInt32(headerPieces[8]),
                    table_xref = headerPieces[9],
                    audio = headerPieces[10],
                    chart_xref = headerPieces[11],
                    user_time = headerPieces[12]
                };

                LinePart = new Part()
                {
                    nct = Convert.ToInt32(linePieces[1]),
                    section = linePieces[2],
                    part = Escape.ChartUnescape(linePieces[3])
                };

                Data = Escape.ChartUnescape(linePieces[4]);
                DataSegments = data();
                LineNumber = Convert.ToInt32(linePieces[5]);
            }
            #endregion

            /// <summary>
            /// Return a string representation of this Line
            /// </summary>
            /// <returns>String representation of this Line</returns>
            public override string ToString()
            {
                return String.Join(Constants.DelimiterLine.ToString(), new string[]
                {
                    LineHeader.ToString(),
                    LinePart.ToString(),
                    Data
                });
            }

            /// <summary>
            /// Create the list of DataSegments for this line
            /// </summary>
            /// <returns></returns>
            public List<DataSegment> data()
            {
                var dataPieces = Data.Split(Constants.DelimiterData);
                List<DataSegment> parsedData = new List<DataSegment>();
                var nct = NCT();
                if (ForSigning)
                {
                    if (dataPieces.Length > 0)
                    {
                        // If we're doing a digital signature, don't change the data segment, because
                        // older entries could have all sorts of wackiness going on and we don't want
                        // to make something different than what was actually written to the chart
                        dataPieces[dataPieces.Length].TrimEnd(new char[] { '\n', '\r' });
                        foreach (string dataPiece in dataPieces)
                        {
                            parsedData.Add(new DataSegment(dataPiece, true));
                        }
                    }
                }
                else if (nct == Constants.NCT_FLOWSHEET || nct == Constants.NCT_FLOWSHEET_VS)
                {
                    parsedData.AddRange(HandleVsEntry(dataPieces));
                }
                else
                {
                    foreach (string dataPiece in dataPieces)
                    {
                        parsedData.Add(MakeDataSegment(dataPiece));
                    }
                }

                DataSegments = parsedData;
                return DataSegments;
            }

            /// <summary>
            /// Special parsing for VS entries in the EMR line
            /// </summary>
            /// <param name="dataPieces">List of pieces parsed from the data portion of the Line</param>
            /// <returns>List of new DataSegments</returns>
            private List<DataSegment> HandleVsEntry(string[] dataPieces)
            {
                List<DataSegment> ret = new List<DataSegment>();
                foreach (string data in dataPieces)
                {
                    if (data == null || data.Equals(""))
                    {
                        ret.Add(new DataSegment(null, ""));
                    }
                    else
                    {
                        var pieces = data.Split(DataSegment.Constants.DELIMITER);
                        foreach (string piece in pieces)
                        {
                            var value = Escape.ChartUnescape(piece);
                            ret.Add(new DataSegment(null, value));
                        }
                    }
                }

                return ret;
            }

            /// <summary>
            /// Create a DataSegment from a provided string in the EMR line
            /// </summary>
            /// <param name="dataPiece">Scalar piece of data from the line</param>
            /// <returns>New DataSegment</returns>
            private DataSegment MakeDataSegment(string dataPiece)
            {
                return new DataSegment(dataPiece);
            }

            /// <summary>
            /// Check whether line is active or inactive
            /// </summary>
            /// <returns></returns>
            public bool IsInactive()
            {
                return LineHeader.get(Constants.Status).Equals(Constants.INACTIVE);
            }

            #region Line information getters
            /// <summary>
            /// Get the NCT number for this line
            /// </summary>
            /// <returns>NCT number for this line</returns>
            public int NCT()
            {
                return Convert.ToInt32(LinePart.get(Constants.NCT));
            }

            /// <summary>
            /// Get the system time associated with this line
            /// </summary>
            /// <returns>System timestamp</returns>
            public string SysTime()
            {
                return LineHeader.get(Constants.SystemTime);
            }

            /// <summary>
            /// Get the User identifier for this line
            /// </summary>
            /// <returns>User ID number</returns>
            public int User()
            {
                var userString = LineHeader.get(Constants.User);
                return String.IsNullOrEmpty(userString) ? 0 : Convert.ToInt32(userString);
            }

            /// <summary>
            /// Get the user time associated with this line
            /// </summary>
            /// <returns>User-entered timestamp</returns>
            public string UserTime()
            {
                var sysTime = SysTime();
                var userTime = LineHeader.get(Constants.UserTime);
                return String.IsNullOrEmpty(userTime) ? sysTime : userTime;
            }

            /// <summary>
            /// Get the losecs value(s) associated with this line
            /// </summary>
            /// <returns>Losec(s) values</returns>
            public string Losecs()
            {
                return LineHeader.get(Constants.Losecs);
            }

            /// <summary>
            /// Get the level value associated with this line
            /// </summary>
            /// <returns>Level value</returns>
            public string Level()
            {
                return LineHeader.get(Constants.Level);
            }

            /// <summary>
            /// Get the status value associated with this line
            /// </summary>
            /// <returns>Status value</returns>
            public string Status()
            {
                return LineHeader.get(Constants.Status);
            }

            /// <summary>
            /// Get the documentation ID associated with this line
            /// </summary>
            /// <returns>Documentation ID value</returns>
            public string DocId()
            {
                return LineHeader.get(Constants.DocId);
            }

            /// <summary>
            /// Get the NoLink flag associated with this line
            /// </summary>
            /// <returns>No link boolean</returns>
            public bool NoLink()
            {
                var val = LineHeader.get(Constants.NoLink);
                return (val.Equals("1"));
            }

            /// <summary>
            /// Get the inactivation timestamp associated with this line
            /// </summary>
            /// <returns>Inactivation timestamp</returns>
            public string InactiveTime()
            {
                return LineHeader.get(Constants.InactiveTime);
            }

            /// <summary>
            /// Get the ID of the user responsible for inactivating this line
            /// </summary>
            /// <returns>Inactivation user ID</returns>
            public int InactiveUser()
            {
                var userString = LineHeader.get(Constants.InactiveUser);
                return String.IsNullOrEmpty(userString) ? 0 : Convert.ToInt32(userString);
            }

            /// <summary>
            /// Get the table XRref associated with this line
            /// </summary>
            /// <returns>Table XRef value</returns>
            public string TableXRef()
            {
                return LineHeader.get(Constants.TableXRef);
            }

            /// <summary>
            /// Get the audio data associated with this line
            /// </summary>
            /// <returns>Audio data</returns>
            public string Audio()
            {
                return LineHeader.get(Constants.Audio);
            }

            /// <summary>
            /// Get the chart XRef associated with this line
            /// </summary>
            /// <returns>Chart XRef value</returns>
            public string ChartXRef()
            {
                return LineHeader.get(Constants.ChartXRef);
            }

            /// <summary>
            /// Get the name of the chart section associated with this line
            /// </summary>
            /// <returns>Chart section name</returns>
            public string SectionName()
            {
                return LinePart.get(Constants.Section);
            }

            /// <summary>
            /// Get the name of the chart part associated with this line
            /// </summary>
            /// <returns>Chart part name</returns>
            public string PartName()
            {
                return LinePart.get(Constants.Part);
            }
            #endregion

            /// <summary>
            /// Create a clone of this line, which is sometimes necessary.
            /// </summary>
            /// <returns>Cloned lined</returns>
            public Line Clone()
            {
                return (Line)this.MemberwiseClone();
            }

            object ICloneable.Clone()
            {
                return Clone();
            }

            /// <summary>
            /// Class representing the "header" portion of an EMR line
            /// </summary>
            public class Header
            {
                #region EMR.Line.Header properties
                /// <summary>
                /// System time
                /// </summary>
                public string sys_time { private get; set; }

                /// <summary>
                /// User number
                /// </summary>
                public int user { private get; set; }

                /// <summary>
                /// Losecs value
                /// </summary>
                public string losecs { private get; set; }

                /// <summary>
                /// Level
                /// </summary>
                public string level { private get; set; }

                /// <summary>
                /// Status
                /// </summary>
                public string status { private get; set; }

                /// <summary>
                /// Doc ID
                /// </summary>
                public string doc_id { private get; set; }

                /// <summary>
                /// No link flag
                /// </summary>
                public string no_link { private get; set; }

                /// <summary>
                /// Inactivation timestamp
                /// </summary>
                public string inactive_time { private get; set; }

                /// <summary>
                /// Inactivation user
                /// </summary>
                public int inactive_user { private get; set; }

                /// <summary>
                /// Table Xref value
                /// </summary>
                public string table_xref { private get; set; }

                /// <summary>
                /// Audio data
                /// </summary>
                public string audio { private get; set; }

                /// <summary>
                /// Chart Xref value
                /// </summary>
                public string chart_xref { private get; set; }

                /// <summary>
                /// User-entered timestamp
                /// </summary>
                public string user_time { private get; set; }
                #endregion

                /// <summary>
                /// Return a string representation of this Header
                /// </summary>
                /// <returns>String representation of the Header</returns>
                public override string ToString()
                {
                    var headerFields = new string[]
                    {
                        get(Constants.SystemTime),
                        get(Constants.User),
                        get(Constants.Losecs),
                        get(Constants.Level),
                        get(Constants.Status),
                        get(Constants.DocId),
                        get(Constants.NoLink),
                        get(Constants.InactiveTime),
                        get(Constants.InactiveUser),
                        get(Constants.TableXRef),
                        get(Constants.Audio),
                        get(Constants.ChartXRef),
                        get(Constants.UserTime)
                    };

                    return String.Join(Constants.DelimiterHeader.ToString(), headerFields);
                }
                /// <summary>
                /// Get a piece of data from the EMR line header
                /// </summary>
                /// <param name="identifier">Constants constant identifier for the piece of data</param>
                /// <returns>data string (internally a string or int)</returns>
                public string get(string identifier)
                {
                    switch (identifier)
                    {
                        case Constants.SystemTime: return sys_time ?? "";
                        case Constants.User: return user.ToString();
                        case Constants.Losecs: return losecs ?? "";
                        case Constants.Level: return level ?? "";
                        case Constants.Status: return status ?? "";
                        case Constants.DocId: return doc_id ?? "";
                        case Constants.NoLink: return no_link ?? "";
                        case Constants.InactiveTime: return inactive_time ?? "";
                        case Constants.InactiveUser: return inactive_user.ToString();
                        case Constants.TableXRef: return table_xref ?? "";
                        case Constants.Audio: return audio ?? "";
                        case Constants.ChartXRef: return chart_xref ?? "";
                        case Constants.UserTime: return user_time ?? "";
                        default: return "";
                    }
                }
            }

            /// <summary>
            /// Class representing the 'part' portion of an EMR Line
            /// </summary>
            public class Part
            {
                #region EMR.Line.Part properties
                /// <summary>
                /// NCT number
                /// </summary>
                public int nct { private get; set; }

                /// <summary>
                /// Section name
                /// </summary>
                public string section { private get; set; }

                /// <summary>
                /// Part name
                /// </summary>
                public string part { private get; set; }
                #endregion

                #region EMR.Line.Part constructors
                /// <summary>
                /// Create a new, empty EMR Line Part
                /// </summary>
                public Part()
                {

                }

                /// <summary>
                /// Create a new EMR Line Part from a provided string
                /// </summary>
                /// <param name="part">String representing the part</param>
                public Part(string part)
                {
                    var partPieces = part.Split(Constants.DelimiterLine);
                    nct = Convert.ToInt32(partPieces[0]);
                    section = partPieces[1];
                    part = partPieces[2];
                }
                #endregion

                /// <summary>
                /// Return a string representation of this Part
                /// </summary>
                /// <returns>String representation of this Part</returns>
                public override string ToString()
                {
                    var partFields = new string[]
                    {
                        get(Constants.NCT),
                        get(Constants.Section),
                        get(Constants.Part)
                    };

                    return String.Join(Constants.DelimiterLine.ToString(), partFields);
                }

                /// <summary>
                /// Get a piece of data from the EMR line part
                /// </summary>
                /// <param name="identifier">Constants constant identifier for the piece of data</param>
                /// <returns></returns>
                public string get(string identifier)
                {
                    switch (identifier)
                    {
                        case Constants.NCT: return (nct != 0) ? nct.ToString() : "";
                        case Constants.Section: return section ?? "";
                        case Constants.Part: return part ?? "";
                        default: return "";
                    }
                }
            }

            /// <summary>
            /// Represents the different segments of a 'data' section of a chart line.
            /// </summary>
            public class DataSegment
            {
                #region EMR.Line.DataSegment properties
                /// <summary>
                /// Type of data segment
                /// </summary>
                public string Type { get; set; }

                /// <summary>
                /// Value of data segment
                /// </summary>
                public Value value { get; set; }

                /// <summary>
                /// Store the raw value of a datasegment
                /// </summary>
                private string RawValue = null;

                /// <summary>
                /// Name side chart writer value
                /// </summary>
                private string chart_writer = null;

                /// <summary>
                /// Segments contained in name portion of data segment
                /// </summary>
                public Name NameSegments { get; set; }

                /// <summary>
                /// Segments contained in value portion of data segment
                /// </summary>
                public Value ValueSegments { get; set; }

                /// <summary>
                /// Scoring type for this DataSegment
                /// </summary>
                public string Score { get; set; }

                /// <summary>
                /// Data segment label
                /// </summary>
                public string Label { get; set; }

                /// <summary>
                /// Flag for whether this data segment should keep trailing delimiters
                /// </summary>
                public bool KeepTrailingDelimiter { get; set; }

                /// <summary>
                /// Flag for whether this datasegment is involved in chart signing
                /// </summary>
                private bool ForSigning = false;

                private static List<string> NAME_SEGMENTS = new List<string>
                {
                    Name.Constants.InactivateLine,
                    Name.Constants.ChartWriter,
                    Name.Constants.JavaScript,
                    Name.Constants.CBD,
                    Name.Constants.Risk,
                    Name.Constants.ClinicalId,
                    Name.Constants.MDM
                };

                private static List<string> VALUE_SEGMENTS = new List<string>
                {
                    Value.Constants.Value,
                    Value.Constants.CBD,
                    Value.Constants.Abnormal,
                    Value.Constants.ClinicalId,
                    Value.Constants.Meta,
                    Value.Constants.LMRP,
                    Value.Constants.MDM
                };

                private static HashSet<string> VALUE_SIDE_MARKUP = new HashSet<string>
                {
                    Constants.TYPE_CHECKBOX,
                    Constants.TYPE_DROPDOWN,
                    Constants.TYPE_RADIO
                };
                #endregion

                #region EMR.Line.DataSegment constructors
                /// <summary>
                /// Create a new, empty data segment
                /// </summary>
                public DataSegment()
                {
                    NameSegments = new Name();
                    ValueSegments = new Value();
                }

                /// <summary>
                /// Create a new data segment from a provided data string
                /// </summary>
                /// <param name="p">Data string from the chart file</param>
                /// <param name="fs">Flag to specify whether this datasegment is for signing the chart</param>
                public DataSegment(string p, bool fs = false)
                {
                    ForSigning = fs;
                    RawValue = p;
                    NameSegments = new Name();
                    ValueSegments = new Value();
                    ParseDataMarkup(p);
                }

                /// <summary>
                /// Create a new data segment with a provided type and value
                /// </summary>
                /// <param name="t">Data segment type</param>
                /// <param name="v">Data segment value</param>
                public DataSegment(string t, string v)
                {
                    Type = t;
                    NameSegments = new Name();
                    ValueSegments = new Value
                    {
                        value = v
                    };
                }
                #endregion

                /// <summary>
                /// Boolean check for whether this segment is flagged as abnormal
                /// </summary>
                /// <returns>Boolean flag for abnormal</returns>
                public bool IsAbnormal()
                {
                    return ValueSegments.abnormal != null && (
                           ValueSegments.abnormal.ToUpperInvariant().Equals("U")
                        || ValueSegments.abnormal.ToUpperInvariant().Equals("1")
                    );
                }

                /// <summary>
                /// Get the text for the segment
                /// </summary>
                /// <returns>A delimited string representation of the segment</returns>
                public override string ToString()
                {
                    if (ForSigning)
                    {
                        return RawValue;
                    }

                    // Clear out segments that depend on what type of value it is.
                    if (VALUE_SIDE_MARKUP.Contains(Type))
                    {
                        NameSegments.abnormal = "";
                        NameSegments.cbd = "";
                        NameSegments.clinical_id = "";
                        NameSegments.mdm = "";
                        // Make sure the abnormal value is set properly.
                        if (IsAbnormal())
                        {
                            ValueSegments.abnormal = Constants.ABNORMAL;
                        }
                    }
                    else
                    {
                        ValueSegments.abnormal = "";
                        ValueSegments.cbd = "";
                        ValueSegments.clinical_id = "";
                        ValueSegments.mdm = "";
                    }

                    // If we're getting the full text of the segment, the value needs to be properly escaped.
                    // Do this on a copy of the values so that calling ToString() repeatedly on a given object
                    // won't cause repeated escaping (the infamous "<LT>AMP>")
                    var valueCopy = ValueSegments.Clone();
                    valueCopy.value = Escape.ChartEscape(ValueSegments.value);

                    var name = String.Join(Constants.DELIMITER.ToString(), new string[]
                    {
                        NameSegments.inactivate_line > 0 ? NameSegments.inactivate_line.ToString() : "",
                        ChartWriter() ?? "",
                        NameSegments.javascript ?? "",
                        NameSegments.cbd ?? "",
                        NameSegments.risk ?? "",
                        NameSegments.clinical_id ?? "",
                        NameSegments.mdm ?? ""
                    });

                    name = name.Trim().TrimEnd(new char[] { Constants.DELIMITER });
                    if (KeepTrailingDelimiter)
                    {
                        name += Constants.DELIMITER;
                    }

                    // if after determining the name is empty, restore the original value
                    // flowsheets do not have a name and must not be escaped to display properly
                    if (String.IsNullOrEmpty(name))
                    {
                        valueCopy.value = ValueSegments.value;
                    }
                    var value = String.Join(Constants.DELIMITER.ToString(), new string[]
                    {
                        valueCopy.value ?? "",
                        ValueSegments.cbd ?? "",
                        ValueSegments.abnormal ?? "",
                        ValueSegments.clinical_id ?? "",
                        ValueSegments.meta ?? "",
                        ValueSegments.lmrp ?? "",
                        ValueSegments.mdm ?? ""
                    });

                    value = value.Trim().TrimEnd(new char[] { Constants.DELIMITER });

                    var retList = new List<string>();
                    if (!String.IsNullOrEmpty(name))
                    {
                        retList.Add(name);
                    }
                    if (!String.IsNullOrEmpty(value))
                    {
                        retList.Add(value);
                    }

                    return String.Join(Constants.NAME_VAL_DELIMITER.ToString(), retList);
                }

                private void ParseDataMarkup(string p)
                {
                    var pieces = p.Split(new char[] { Constants.NAME_VAL_DELIMITER }, 2);
                    var name = pieces[0];
                    var value = pieces.Length > 1 ? pieces[1] : "";

                    var namePieces = name.Split(Constants.DELIMITER);
                    for (int i = 0; i < NAME_SEGMENTS.Count; i++)
                    {
                        var segmentName = NAME_SEGMENTS[i];
                        var segmentValue = (namePieces.Length > i && namePieces[i] != null) ? namePieces[i] : "";
                        if (segmentName.Equals(Name.Constants.ChartWriter))
                        {
                            Type = segmentValue;
                        }
                        NameSegments.set(segmentName, segmentValue);
                    }

                    var valuePieces = value.Split(Constants.DELIMITER);
                    for (int i = 0; i < VALUE_SEGMENTS.Count; i++)
                    {
                        var segmentName = VALUE_SEGMENTS[i];
                        var segmentValue = (valuePieces.Length > i && valuePieces[i] != null) ? Escape.ChartUnescape(valuePieces[i]) : "";
                        ValueSegments.set(segmentName, segmentValue);
                    }
                }

                /// <summary>
                /// Get or set the chart_writer value from the data segment
                /// </summary>
                /// <param name="cw">Optional chart writer value to set</param>
                /// <returns>chart_writer value</returns>
                public string ChartWriter(string cw = null)
                {
                    if (cw != null)
                    {
                        if (cw.Length > 0)
                        {
                            var type = cw.Substring(0, 1);
                            if (!type.Equals(Constants.TYPE_TABLE)
                                && !type.Equals(Constants.TYPE_EM_CAVEAT)
                                && !type.Equals(Constants.TYPE_HIGH_ROS.ToUpper())
                                && type.Equals(type.ToUpper()))
                            {
                                Score = Constants.ADD_TO_SCORE;
                                type = type.ToLower();
                            }
                            Type = type;
                        }
                        chart_writer = cw;
                    }

                    // TODO: chart_writer depends on type, score, abnormal, and label values, so this should get cleared out if any of those change.
                    else if (String.IsNullOrEmpty(chart_writer))
                    {
                        if (Type == null)
                        {
                            chart_writer = "";
                        }
                        else
                        {
                            var score = Score ?? "";
                            var currentType = Type;
                            var adjustedType = currentType;
                            if (IsAbnormal() && (currentType.Equals(Constants.TYPE_TEXT) || currentType.Equals(Constants.TYPE_CALENDAR)))
                            {
                                adjustedType = Constants.TYPE_ABNORMAL;
                            }
                            else if (score.Equals(Constants.FORCE_HIGH_SCORE))
                            {
                                adjustedType = Constants.TYPE_EM_CAVEAT;
                            }
                            else if (score.Equals(Constants.FORCE_HIGH_ROS) && (currentType.Equals(Constants.TYPE_DROPDOWN) || currentType.Equals(Constants.TYPE_RADIO) || currentType.Equals(Constants.TYPE_CHECKBOX)))
                            {
                                adjustedType = Constants.TYPE_HIGH_ROS;
                            }
                            else if (score.Equals(Constants.ADD_TO_SCORE))
                            {
                                adjustedType = currentType.ToUpper();
                            }

                            // Text and calendar inputs have the label appear after the type.
                            // High ROS are indicated by "n3"
                            var additional = (currentType.Equals(Constants.TYPE_TEXT) || currentType.Equals(Constants.TYPE_CALENDAR) || currentType.Equals(Constants.TYPE_DROPDOWN)) ? Label :
                                (adjustedType.Equals(Constants.TYPE_HIGH_ROS)) ? Constants.MODIFIER_HIGH_ROS :
                                "";

                            chart_writer = adjustedType + additional;
                        }
                    }

                    return chart_writer;
                }

                /// <summary>
                /// Constants reserverd for use in DataSegment.
                /// </summary>
                // TODO: This is missing a lot that is present in the Perl code, because the API didn't need it at the time.
                public class Constants
                {
                    #region delimiter constants
                    // --- DATA SEGMENT DELIMITERS --- //

                    /// <summary>
                    /// Delimiter for data segments
                    /// </summary>
                    public const char DELIMITER = '^';

                    /// <summary>
                    /// Delimiter for name and value within a data segment
                    /// </summary>
                    public const char NAME_VAL_DELIMITER = '=';
                    #endregion

                    /// <summary>
                    /// Abnormal flag
                    /// </summary>
                    public const string ABNORMAL = "U";

                    /// <summary>
                    /// The modifier needed to properly force a high ROS score
                    /// </summary>
                    public const string MODIFIER_HIGH_ROS = "3";

                    /// <summary>
                    /// Modifier for printing a standard table header
                    /// </summary>
                    public const string MODIFIER_TABLE_HEADER = "2";

                    /// <summary>
                    /// Modifier for printing an abnormal table header
                    /// </summary>
                    public const string MODIFIER_TABLE_ABNORMAL = "1";

                    #region scoring constants
                    // --- SCORING CONSTANTS --- //
                    /// <summary>
                    /// Add to score
                    /// </summary>
                    public const string ADD_TO_SCORE = "1";

                    /// <summary>
                    /// Force high ROS
                    /// </summary>
                    public const string FORCE_HIGH_ROS = "N";

                    /// <summary>
                    /// Force high score
                    /// </summary>
                    public const string FORCE_HIGH_SCORE = "E";
                    #endregion

                    #region data segment caret types
                    // --- DATA SEGMENT CARET TYPES --- //

                    /// <summary>
                    /// Calendar identifier
                    /// </summary>
                    public const string TYPE_CALENDAR = "c";

                    /// <summary>
                    /// Text identifier
                    /// </summary>
                    public const string TYPE_TEXT = "c";

                    /// <summary>
                    /// Checkbox identifier
                    /// </summary>
                    public const string TYPE_CHECKBOX = "d";

                    /// <summary>
                    /// Radio button identifier
                    /// </summary>
                    public const string TYPE_RADIO = "d";

                    /// <summary>
                    /// EM caveat identifier
                    /// </summary>
                    public const string TYPE_EM_CAVEAT = "E";

                    /// <summary>
                    /// Table markup identifier
                    /// </summary>
                    public const string TYPE_TABLE = "G";

                    /// <summary>
                    /// Table cell identifier
                    /// </summary>
                    public const string TYPE_TABLE_CELL = "g";

                    /// <summary>
                    /// Part prepend identifier
                    /// </summary>
                    public const string TYPE_PART_PREPEND = "h";

                    /// <summary>
                    /// Meta data identifier
                    /// </summary>
                    public const string TYPE_META = "m";

                    /// <summary>
                    /// High ROS identifier
                    /// </summary>
                    public const string TYPE_HIGH_ROS = "n";

                    /// <summary>
                    /// Drop down identifier
                    /// </summary>
                    public const string TYPE_DROPDOWN = "s";

                    /// <summary>
                    /// Abnormal data identifier
                    /// </summary>
                    public const string TYPE_ABNORMAL = "U";
                    #endregion
                }

                /// <summary>
                /// Represents a data segment name side
                /// </summary>
                public class Name
                {
                    #region EMR.Line.DataSegment.Name properties
                    /// <summary>
                    /// Name side abnormal flag
                    /// </summary>
                    public string abnormal { get; set; }

                    /// <summary>
                    /// Name side CBD value
                    /// </summary>
                    public string cbd { get; set; }

                    /// <summary>
                    /// Name side clinical ID value
                    /// </summary>
                    public string clinical_id { get; set; }

                    /// <summary>
                    /// Name side inactivate line value
                    /// </summary>
                    public int inactivate_line { get; set; }

                    /// <summary>
                    /// Name side JavaScript value
                    /// </summary>
                    public string javascript { get; set; }

                    /// <summary>
                    /// Name side MDM value
                    /// </summary>
                    public string mdm { get; set; }

                    /// <summary>
                    /// Name side risk value
                    /// </summary>
                    public string risk { get; set; }
                    #endregion

                    /// <summary>
                    /// Create a new empty data segment name side
                    /// </summary>
                    public Name()
                    {

                    }

                    /// <summary>
                    /// Set the value of a segment name in the data segments
                    /// </summary>
                    /// <param name="segmentName">Segment name string</param>
                    /// <param name="val">Segment value</param>
                    public void set(string segmentName, object val)
                    {
                        switch (segmentName)
                        {
                            case Constants.InactivateLine:
                                if (!String.IsNullOrEmpty(val.ToString().Trim()))
                                {
                                    inactivate_line = Convert.ToInt32(val.ToString());
                                }
                                break;
                            case Constants.ChartWriter:
                                break;
                            case Constants.JavaScript:
                                javascript = val.ToString();
                                break;
                            case Constants.CBD:
                                cbd = val.ToString();
                                break;
                            case Constants.Risk:
                                risk = val.ToString();
                                break;
                            case Constants.ClinicalId:
                                clinical_id = val.ToString();
                                break;
                            case Constants.MDM:
                                mdm = val.ToString();
                                break;
                            default:
                                break;
                        }
                    }

                    /// <summary>
                    /// Constants for DataSegment Name
                    /// </summary>
                    public class Constants
                    {
                        /// <summary>
                        /// Name side inactivate line value
                        /// </summary>
                        public const string InactivateLine = "inactivate_line";

                        /// <summary>
                        /// Name side chart writer value
                        /// </summary>
                        public const string ChartWriter = "chart_writer";

                        /// <summary>
                        /// Name side JavaScript value
                        /// </summary>
                        public const string JavaScript = "javascript";

                        /// <summary>
                        /// Name side CBD value
                        /// </summary>
                        public const string CBD = "cbd";

                        /// <summary>
                        /// Name side risk value
                        /// </summary>
                        public const string Risk = "risk";

                        /// <summary>
                        /// Name side clinical id value
                        /// </summary>
                        public const string ClinicalId = "clinical_id";

                        /// <summary>
                        /// Name side MDM value
                        /// </summary>
                        public const string MDM = "mdm";
                    }
                }

                /// <summary>
                /// Represents a data segment value side
                /// </summary>
                public class Value : ICloneable
                {
                    #region EMR.Line.DataSegment.Value properties
                    /// <summary>
                    /// Value side value
                    /// </summary>
                    public string value { get; set; }

                    /// <summary>
                    /// Value side CBD value
                    /// </summary>
                    public string cbd { get; set; }

                    /// <summary>
                    /// Value side abnormal value
                    /// </summary>
                    public string abnormal { get; set; }

                    /// <summary>
                    /// Value side clinical id value
                    /// </summary>
                    public string clinical_id { get; set; }

                    /// <summary>
                    /// Value side meta data value
                    /// </summary>
                    public string meta { get; set; }

                    /// <summary>
                    ///  Value side LMRP value
                    /// </summary>
                    public string lmrp { get; set; }

                    /// <summary>
                    /// Value side MDM value
                    /// </summary>
                    public string mdm { get; set; }
                    #endregion

                    /// <summary>
                    /// Create a new empty data segment value side
                    /// </summary>
                    public Value()
                    {

                    }

                    /// <summary>
                    /// Set the value of a segment value in the data segments
                    /// </summary>
                    /// <param name="segmentName">Segment name string</param>
                    /// <param name="val">Segment value</param>
                    public void set(string segmentName, object val)
                    {
                        switch (segmentName)
                        {
                            case Constants.Value:
                                value = val.ToString();
                                break;
                            case Constants.CBD:
                                cbd = val.ToString();
                                break;
                            case Constants.Abnormal:
                                abnormal = val.ToString();
                                break;
                            case Constants.ClinicalId:
                                clinical_id = val.ToString();
                                break;
                            case Constants.Meta:
                                meta = val.ToString();
                                break;
                            case Constants.LMRP:
                                lmrp = val.ToString();
                                break;
                            case Constants.MDM:
                                mdm = val.ToString();
                                break;
                            default:
                                break;
                        }
                    }

                    /// <summary>
                    /// Create a clone of this Value.
                    /// </summary>
                    /// <returns>Cloned Value</returns>
                    public Value Clone()
                    {
                        return (Value)this.MemberwiseClone();
                    }

                    object ICloneable.Clone()
                    {
                        return Clone();
                    }

                    /// <summary>
                    /// Constants for DataSegment Value
                    /// </summary>
                    public class Constants
                    {
                        /// <summary>
                        /// Value side value identifier
                        /// </summary>
                        public const string Value = "value";

                        /// <summary>
                        /// Value side CBD identifier
                        /// </summary>
                        public const string CBD = "cbd";

                        /// <summary>
                        /// Value side abnormal identifier
                        /// </summary>
                        public const string Abnormal = "abnormal";

                        /// <summary>
                        /// Value side clinical id identifier
                        /// </summary>
                        public const string ClinicalId = "clinical_id";

                        /// <summary>
                        /// Value side meta data identifier
                        /// </summary>
                        public const string Meta = "meta";

                        /// <summary>
                        /// Value side LMRP identifier
                        /// </summary>
                        public const string LMRP = "lmrp";

                        /// <summary>
                        /// Value side MDM identifier
                        /// </summary>
                        public const string MDM = "mdm";
                    }
                }
            }
        }

        /// <summary>
        /// Clinical ID object
        /// </summary>
        public class ClinicalId
        {
            /// <summary>
            /// Identifier
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Site identifier
            /// </summary>
            public byte Site { get; set; }

            /// <summary>
            /// Patient identifier
            /// </summary>
            public string Ibex { get; set; }

            /// <summary>
            /// Staff identifier
            /// </summary>
            public int DrsNum { get; set; }

            /// <summary>
            /// Clinical ID Label
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Clinical ID Value
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// System date/timestamp associated with this entry
            /// </summary>
            public string SysTime { get; set; }

            /// <summary>
            /// User date/timestamp associated with this entry
            /// </summary>
            public string UserTime { get; set; }

            /// <summary>
            /// Clinical ID T name
            /// </summary>
            public string TName { get; set; }

            /// <summary>
            /// Clinical ID H name
            /// </summary>
            public string HName { get; set; }

            /// <summary>
            /// Clinical ID NCT number
            /// </summary>
            public int NCTNum { get; set; }

            /// <summary>
            /// Create a new, empty Clinical ID
            /// </summary>
            public ClinicalId()
            {

            }

            /// <summary>
            /// Add a Clinical ID to the database
            /// </summary>
            /// <param name="con">Optional SqlConnection</param>
            /// <returns>Boolean success flag</returns>
            public bool Add(SqlConnection con = null)
            {
                var result = new DB.Insert
                {
                    Sql = "INSERT INTO clinical_id (id, site, ibex, drs_num, label, value, sysdate, usrdate, tname, hname, nct_num) VALUES (@id, @site, @ibex, @drs_num, @label, @value, @sysdate, @usrdate, @tname, @hname, @nct_num)",
                    Connection = con,
                    Parameters = GetSqlParameters()
                }.Run();

                return (result > 0);
            }

            /// <summary>
            /// Remove a Clinical ID from the database
            /// </summary>
            /// <param name="con">Optional SqlConnection</param>
            /// <returns></returns>
            public bool Remove(SqlConnection con = null)
            {
                var result = new DB.Update
                {
                    Sql = "DELETE FROM clinical_id WHERE id=@id AND site=@site AND ibex=@ibex AND drs_num=@drs_num AND label=@label AND value=@value AND sysdate=@sysdate AND usrdate=@usrdate AND tname=@tname AND hname=@hname AND nct_num=@nct_num",
                    Connection = con,
                    Parameters = GetSqlParameters()
                }.Run();

                return (result > 0);
            }

            private SqlParameter[] GetSqlParameters()
            {
                return new SqlParameter[] {
                new SqlParameter("@id", SqlDbType.VarChar) { Value = Id },
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = Site },
                new SqlParameter("@ibex", SqlDbType.Char) { Value = Ibex },
                new SqlParameter("@drs_num", SqlDbType.Int) { Value = DrsNum },
                new SqlParameter("@label", SqlDbType.VarChar) { Value = Label },
                new SqlParameter("@value", SqlDbType.Text) { Value = Value },
                new SqlParameter("@sysdate", SqlDbType.Char) { Value = SysTime },
                new SqlParameter("@usrdate", SqlDbType.Char) { Value = UserTime },
                new SqlParameter("@tname", SqlDbType.Char) { Value = TName },
                new SqlParameter("@hname", SqlDbType.Char) { Value = HName },
                new SqlParameter("@nct_num", SqlDbType.Int) { Value = NCTNum }
            };
            }

            /// <summary>
            /// Remove a Clinical ID Info row from the database
            /// </summary>
            /// <returns>Boolean success flag</returns>
            public bool RemoveInfo()
            {
                if (!String.IsNullOrEmpty(Id))
                {
                    return false;
                }
                var result = new DB.Update
                {
                    Sql = "DELETE FROM clinical_id_info WHERE id=@id",
                    Parameters = new SqlParameter[]
                    {
                    new SqlParameter("@id", SqlDbType.VarChar) { Value = Id }
                    }
                }.Run();

                return (result > 0);
            }

            /// <summary>
            /// Remove all Clinical IDs for a site from the database
            /// </summary>
            /// <param name="site">Site identifier</param>
            /// <returns>Boolean success flag</returns>
            public bool RemoveAll(byte site)
            {
                var result = new DB.Update
                {
                    Sql = "DELETE FROM clinical_id_info WHERE site=@site",
                    Parameters = new SqlParameter[]
                    {
                    new SqlParameter("@site", SqlDbType.SmallInt) { Value = site }
                    }
                }.Run();

                return (result > 0);
            }

            /// <summary>
            /// Look up clinical ID information by ID. 
            /// </summary>
            /// <param name="id">Clinical ID</param>
            /// <returns>Info object</returns>
            public Info LookupInfo(int id)
            {
                return new Info(new DB.Select
                {
                    Sql = "SELECT * FROM clinical_id_info WHERE id=@id",
                    Parameters = new SqlParameter[]
                    {
                    new SqlParameter("@id", SqlDbType.Int) { Value = id }
                    }
                }.RunForDataRow());
            }

            /// <summary>
            /// Look up clinical ID information by clinical ID and site.
            /// </summary>
            /// <param name="clinicalId">Clinical ID</param>
            /// <param name="site">Site ID</param>
            /// <returns>Info object</returns>
            public Info LookupInfo(string clinicalId, byte site)
            {
                return new Info(new DB.Select
                {
                    Sql = "SELECT * FROM clinical_id_info WHERE clinical_id=@clinical_id AND site=@site",
                    Parameters = new SqlParameter[]
                    {
                    new SqlParameter("@clinical_id", SqlDbType.VarChar) { Value = clinicalId },
                    new SqlParameter("@site", SqlDbType.SmallInt) { Value = site }                  // Smallint? Who did this?
                    }
                }.RunForDataRow());
            }

            /// <summary>
            /// Clinical ID info
            /// </summary>
            public class Info
            {
                /// <summary>
                /// Internal ID of clinical ID info
                /// </summary>
                public int Id { get; set; }

                /// <summary>
                /// Clinical ID
                /// </summary>
                public string ClinicalId { get; set; }

                /// <summary>
                /// Description of clinical ID info
                /// </summary>
                public string Description { get; set; }

                /// <summary>
                /// Site identifier for clinical ID info
                /// </summary>
                public byte Site { get; set; }

                /// <summary>
                /// User ID associated with clinical ID info
                /// </summary>
                public int User { get; set; }

                /// <summary>
                /// Date associated with clinical ID info
                /// </summary>
                public string Date { get; set; }

                /// <summary>
                /// Codes associated with this Clinical ID
                /// </summary>
                public List<Code> Codes { get; set; }

                private Dictionary<string, object> Changed = new Dictionary<string, object>();
                private Dictionary<string, List<Code>> ChangedCodes = new Dictionary<string, List<Code>>();
                private Dictionary<string, Code> Lookup = new Dictionary<string, Code>();

                /// <summary>
                /// Create a new empty Clinical ID Info object
                /// </summary>
                public Info()
                {
                    ChangedCodes.Add("add", new List<Code>());
                    ChangedCodes.Add("remove", new List<Code>());
                }

                /// <summary>
                /// Create a new Clinical ID Info object from a provided DataRow
                /// </summary>
                /// <param name="dr"></param>
                public Info(DataRow dr)
                {
                    Id = Convert.ToInt32(dr["id"].ToString());
                    ClinicalId = dr[Constants.ClinicalId].ToString();
                    Description = dr[Constants.Description].ToString();
                    Site = Convert.ToByte(dr[Constants.Site].ToString());
                    User = Convert.ToInt32(dr[Constants.User].ToString());
                    Date = dr[Constants.Date].ToString();
                }

                /// <summary>
                /// Get a value from this object
                /// </summary>
                /// <param name="id">Value identifier</param>
                /// <returns>String representation of the value</returns>
                public string Get(string id)
                {
                    switch (id)
                    {
                        case Constants.ClinicalId: return ClinicalId;
                        case Constants.Date: return Date;
                        case Constants.Description: return Description;
                        case Constants.Site: return Site.ToString();
                        case Constants.User: return User.ToString();
                        default: return null;
                    }
                }

                private void _Set(string id, object value)
                {
                    switch (id)
                    {
                        case Constants.ClinicalId:
                            ClinicalId = value.ToString();
                            break;
                        case Constants.Date:
                            Date = value.ToString();
                            break;
                        case Constants.Description:
                            Description = value.ToString();
                            break;
                        case Constants.Site:
                            Site = Convert.ToByte(value);
                            break;
                        case Constants.User:
                            User = Convert.ToInt32(value);
                            break;
                        default:
                            break;
                    }
                }

                /// <summary>
                /// Set changed fields
                /// </summary>
                /// <param name="fields"></param>
                public void Set(Dictionary<string, object> fields)
                {
                    foreach (string k in fields.Keys)
                    {
                        if (Constants.Fields.Contains(k))
                        {
                            if (!Get(k).Equals(fields[k].ToString()))
                            {
                                _Set(k, fields[k]);
                                Changed.Add(k, fields[k]);
                            }
                        }
                        else if (k.Equals(Constants.Codes))
                        {
                            var oldLookup = Lookup;

                            Lookup.Clear();
                            Codes.Clear();

                            foreach (Code c in (List<Code>)fields[k])
                            {
                                var key = c.CodeSystem + "." + c.code;
                                if (Lookup.ContainsKey(key))
                                {
                                    continue;
                                }
                                Lookup.Add(key, c);
                                Codes.Add(c);

                                if (oldLookup.ContainsKey(key))
                                {
                                    oldLookup.Remove(key);
                                }
                                else
                                {
                                    ChangedCodes["add"].Add(c);
                                }
                            }
                            foreach (Code c in Lookup.Values)
                            {
                                ChangedCodes["remove"].Add(c);
                            }
                        }
                    }
                }

                /// <summary>
                /// Object representing a code in a clinical ID
                /// </summary>
                public class Code
                {
                    /// <summary>
                    /// ID of code system that this code is associated with
                    /// </summary>
                    public int CodeSystem { get; set; }

                    /// <summary>
                    /// Code ID
                    /// </summary>
                    public int code { get; set; }

                    /// <summary>
                    /// Create a new empty Code
                    /// </summary>
                    public Code()
                    {

                    }

                    /// <summary>
                    /// Create a new Code with the provided code system and code IDs
                    /// </summary>
                    /// <param name="codeSystem">Code System ID</param>
                    /// <param name="cde">Code ID</param>
                    public Code(int codeSystem, int cde)
                    {
                        CodeSystem = codeSystem;
                        code = cde;
                    }
                }

                /// <summary>
                /// Constants used by the Info class
                /// </summary>
                public class Constants
                {
                    /// <summary>
                    /// ClinicalId in the database
                    /// </summary>
                    public const string ClinicalId = "clinical_id";

                    /// <summary>
                    /// Codes associated with a clinical ID
                    /// </summary>
                    public const string Codes = "codes";

                    /// <summary>
                    /// Description in the database
                    /// </summary>
                    public const string Description = "description";

                    /// <summary>
                    /// Site ID in the database
                    /// </summary>
                    public const string Site = "site";

                    /// <summary>
                    /// User ID in the database
                    /// </summary>
                    public const string User = "usr";

                    /// <summary>
                    /// Datestamp in the database
                    /// </summary>
                    public const string Date = "date";

                    /// <summary>
                    /// Fields used in Info
                    /// </summary>
                    public static readonly List<string> Fields = new List<string>
                    {
                        ClinicalId,
                        Description,
                        Site,
                        User,
                        Date
                    };
                }
            }
        }

        /// <summary>
        /// Defines constants used to reference various pieces of information in the EMR.
        /// </summary>
        public class Constants
        {
            #region status constants
            // --- STATUS CONSTANTS --- ///

            /// <summary>
            /// Active status flag
            /// </summary>
            public const string ACTIVE = "A";

            /// <summary>
            /// Inactive status flag
            /// </summary>
            public const string INACTIVE = "I";
            #endregion

            #region data identifier constants
            // --- DATA IDENTIFIER CONSTANTS --- //

            /// <summary>
            /// Identifier for system time of entry
            /// </summary>
            public const string SystemTime = "sys_time";

            /// <summary>
            /// Identifier for user that made the entry
            /// </summary>
            public const string User = "usr";

            /// <summary>
            /// Identifier for losecs value associated with the entry
            /// </summary>
            public const string Losecs = "losecs";

            /// <summary>
            /// Identifier for the level associated with the entry
            /// </summary>
            public const string Level = "lvl";

            /// <summary>
            /// Identifier for the status of the entry
            /// </summary>
            public const string Status = "status";

            /// <summary>
            /// Identifier for the documentation id of the entry
            /// </summary>
            public const string DocId = "doc_id";

            /// <summary>
            /// Identifier to flag whether there should not be a link in the entry
            /// </summary>
            public const string NoLink = "no_link";

            /// <summary>
            /// Identifier for the time when an entry was inactivated
            /// </summary>
            public const string InactiveTime = "inactive_time";

            /// <summary>
            /// Identifier for the user who inactivated an entry
            /// </summary>
            public const string InactiveUser = "inactive_user";

            /// <summary>
            /// Identifier for the table reference associated with an entry
            /// </summary>
            public const string TableXRef = "table_xref";

            /// <summary>
            /// Identifier for the audio portion associated with an entry
            /// </summary>
            public const string Audio = "audio";

            /// <summary>
            /// Identifier for the chart reference associated with an entry
            /// </summary>
            public const string ChartXRef = "chart_xref";

            /// <summary>
            /// Identifier for the user-entered time associated with an entry
            /// </summary>
            public const string UserTime = "user_time";

            /// <summary>
            /// Identifier for the NCT number portion of the part
            /// </summary>
            public const string NCT = "nct";

            /// <summary>
            /// Identifier for the section portion of the part
            /// </summary>
            public const string Section = "section";

            /// <summary>
            /// Identifier for the part name in the part
            /// </summary>
            public const string Part = "part";

            /// <summary>
            /// Identifier for the data portion of the entry
            /// </summary>
            public const string Data = "data";

            /// <summary>
            /// ID for the chart entry
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Source of the chart entry
            /// </summary>
            public const string DataSource = "data_source";
            #endregion

            #region delimiter constants
            // --- DELIMITER CONSTANTS --- //

            /// <summary>
            /// Data delimiter used in the header portion of the entry
            /// </summary>
            public const char DelimiterHeader = ':';

            /// <summary>
            /// Data delimiter used in the data portion of the entry
            /// </summary>
            public const char DelimiterData = '&';

            /// <summary>
            /// Data delimiter used in the entire entry
            /// </summary>
            public const char DelimiterLine = '|';
            #endregion

            #region NCT number constants
            // --- NCT NUMBER CONSTANTS --- //

            /// <summary>
            /// Addendum NCT number
            /// </summary>
            public const int NCT_ADDENDUM = 225;

            /// <summary>
            /// Admission Request NCT number
            /// </summary>
            public const int NCT_ADMISSION_REQUEST = 223;

            /// <summary>
            /// Allergy NCT number
            /// </summary>
            public const int NCT_ALLERGY = 19;

            /// <summary>
            /// Ambulance NCT number
            /// </summary>
            public const int NCT_AMBULANCE = 215;

            /// <summary>
            /// Attending NCT number
            /// </summary>
            public const int NCT_ATTENDING = 212;

            /// <summary>
            /// Call back NCT number
            /// </summary>
            public const int NCT_CALL_BACK = 211;

            /// <summary>
            /// Call In NCT number
            /// </summary>
            public const int NCT_CALLIN = 219;

            /// <summary>
            /// Care coordinator request NCT number
            /// </summary>
            public const int NCT_CARE_COORD_REQUEST = 241;

            /// <summary>
            /// Chart print NCT number
            /// </summary>
            public const int NCT_CHART_PRINT = 7;

            /// <summary>
            /// Chart view NCT number
            /// </summary>
            public const int NCT_CHART_VIEW = 5;

            /// <summary>
            /// Deleted signed entry NCT number
            /// </summary>
            public const int NCT_DELETED_SIGNED_ENTRY = 239;

            /// <summary>
            /// Digital signature NCT number
            /// </summary>
            public const int NCT_DIG_SIG = 6;

            /// <summary>
            /// Disposition NCT number
            /// </summary>
            public const int NCT_DISPOSITION = 16;

            /// <summary>
            /// EKG interpretation NCT number
            /// </summary>
            public const int NCT_EKG_INTERPRETATION = 201;

            /// <summary>
            /// Events NCT number
            /// </summary>
            public const int NCT_EVENTS = 238;

            /// <summary>
            /// Flowsheet vital signs NCT number
            /// </summary>
            public const int NCT_FLOWSHEET_VS = 10;

            /// <summary>
            /// Flowsheet NCT number
            /// </summary>
            public const int NCT_FLOWSHEET = 11;

            /// <summary>
            /// Form NCT number
            /// </summary>
            public const int NCT_FORM = 20;

            /// <summary>
            /// Greet NCT number
            /// </summary>
            public const int NCT_GREET = 214;

            /// <summary>
            /// Imaging NCT number
            /// </summary>
            public const int NCT_IMAGING = 21;

            /// <summary>
            /// Inactive entry NCT number
            /// </summary>
            public const int NCT_INACTIVE = -1;

            /// <summary>
            /// Instruction NCT number
            /// </summary>
            public const int NCT_INSTRUCTION = 17;

            /// <summary>
            /// Known Allergies NCT number
            /// </summary>
            public const int NCT_KNOWN_ALLERGY = 209;

            /// <summary>
            /// Lab interpretation NCT number
            /// </summary>
            public const int NCT_LAB_INTERPRETATION = 203;

            /// <summary>
            /// Meaningful Use NCT number
            /// </summary>
            public const int NCT_MEANINGFUL_USE = 23;

            /// <summary>
            /// Medication Administration NCT number
            /// </summary>
            public const int NCT_MED_ADMIN = 217;

            /// <summary>
            /// Medication Reconcilliation NCT number
            /// </summary>
            public const int NCT_MED_REC = 218;

            /// <summary>
            /// Medication Servcices NCT number
            /// </summary>
            public const int NCT_MED_SVC = 210;

            /// <summary>
            /// Medications NCT number
            /// </summary>
            public const int NCT_MEDICATIONS = 216;

            /// <summary>
            /// Orders NCT number
            /// </summary>
            public const int NCT_ORDERS = 205;

            /// <summary>
            /// Order Details NCT number
            /// </summary>
            public const int NCT_ORDER_DETAILS = 235;

            /// <summary>
            /// Hidden Section NCT number
            /// </summary>
            public const int NCT_HIDDEN_SECTION = 242;

            /// <summary>
            /// Past Medical History NCT number
            /// </summary>
            public const int NCT_PAST_MEDICAL_HISTORY = 3;

            /// <summary>
            /// Patient Data Change NCT number
            /// </summary>
            public const int NCT_PATIENT_DATA_CHANGE = 9;

            /// <summary>
            /// Patient Notes NCT number
            /// </summary>
            public const int NCT_PATIENT_NOTES = 213;

            /// <summary>
            /// Physical Exam NCT number
            /// </summary>
            public const int NCT_PHYSICAL_EXAM = 4;

            /// <summary>
            /// Physician Addendum NCT number
            /// </summary>
            public const int NCT_PHYSICIAN_ADDENDUM = 226;

            /// <summary>
            /// Prescription NCT number
            /// </summary>
            public const int NCT_PRESCRIPTION = 15;

            /// <summary>
            /// Problem List NCT number
            /// </summary>
            public const int NCT_PROBLEM_LIST = 236;

            /// <summary>
            /// Problem List Audit NCT number
            /// </summary>
            public const int NCT_PROBLEM_LIST_AUDIT = 237;

            /// <summary>
            /// Radiology Interpretation NCT number
            /// </summary>
            public const int NCT_RAD_INTERPRETATION = 204;

            /// <summary>
            /// Results NCT number
            /// </summary>
            public const int NCT_RESULTS = 14;

            /// <summary>
            /// ROS NCT number
            /// </summary>
            public const int NCT_ROS = 2;

            /// <summary>
            /// Prescriptions NCT number
            /// </summary>
            public const int NCT_RX = 15;

            /// <summary>
            /// Triage NCT number
            /// </summary>
            public const int NCT_TRIAGE = 1;

            /// <summary>
            /// Doctor Notes NCT number
            /// </summary>
            public const int NCT_DOCTOR_NOTES = 202;

            /// <summary>
            /// Nurse Notes NCT number
            /// </summary>
            public const int NCT_NURSE_NOTES = 206;

            /// <summary>
            /// Nursing Procedure - Restraint NCT number
            /// </summary>
            public const int NCT_NURSE_PROC_RESTRAINT = 207;

            /// <summary>
            /// Nursing Procedure - Sedation NCT number
            /// </summary>
            public const int NCT_NURSE_PROC_SEDATION = 208;

            /// <summary>
            /// Communications NCT number
            /// </summary>
            public const int NCT_COMMUNICATIONS = 299;
            #endregion

            #region section name constants
            // --- SECTION NAME CONSTANTS --- //

            /// <summary>
            /// Admin section name
            /// </summary>
            public const string SECT_ADMIN = "ADMIN";

            /// <summary>
            /// Known allergies section name
            /// </summary>
            public const string SECT_ALLERGY = "KNOWN ALLERGIES";

            /// <summary>
            /// Ambulance section name
            /// </summary>
            public const string SECT_AMBULANCE = "AMBULANCE";

            /// <summary>
            /// Call In section name
            /// </summary>
            public const string SECT_CALLIN = "CALL IN";

            /// <summary>
            /// Disposition section name
            /// </summary>
            public const string SECT_DISPOSITION = "DISPOSITION";

            /// <summary>
            /// Events section name
            /// </summary>
            public const string SECT_EVENTS = "EVENTS";

            /// <summary>
            /// Flowsheet section name
            /// </summary>
            public const string SECT_FLOWSHEET = "FLOWSHEET";

            /// <summary>
            /// Flowsheet attributes section name
            /// </summary>
            public const string SECT_FLOWSHEET_ATTRIB = "FLOWSHEET ATTRIBUTES";

            /// <summary>
            /// Form section name
            /// </summary>
            public const string SECT_FORM = "FORM";

            /// <summary>
            /// Greet section name
            /// </summary>
            public const string SECT_GREET = "GREET";

            /// <summary>
            /// Instruction section name
            /// </summary>
            public const string SECT_INSTRUCTION = "INSTRUCTION";

            /// <summary>
            /// Medication administration summary section name
            /// </summary>
            public const string SECT_MED_ADMIN = "MEDICATION ADMINISTRATION SUMMARY";

            /// <summary>
            /// Medication service section name
            /// </summary>
            public const string SECT_MED_SVC = "MEDICATION SERVICE";

            /// <summary>
            /// Current Medications section name
            /// </summary>
            public const string SECT_MEDICATIONS = "CURRENT MEDICATIONS";

            /// <summary>
            /// Orders section name
            /// </summary>
            public const string SECT_ORDERS = "ORDERS";

            /// <summary>
            /// Order details section name
            /// </summary>
            public const string SECT_ORDER_DETAILS = "ORDER DETAILS";

            /// <summary>
            /// Past Medical History section name
            /// </summary>
            public const string SECT_PAST_MEDICAL_HISTORY = "PAST MEDICAL HISTORY";

            /// <summary>
            /// Problem List section name
            /// </summary>
            public const string SECT_PROBLEM_LIST = "PROBLEM LIST";

            /// <summary>
            /// Problem List Audit section name
            /// </summary>
            public const string SECT_PROBLEM_LIST_AUDIT = "PROBLEM LIST AUDIT";

            /// <summary>
            /// Prescription section name
            /// </summary>
            public const string SECT_RX = "PRESCRIPTION";

            /// <summary>
            /// Triage section name
            /// </summary>
            public const string SECT_TRIAGE = "TRIAGE";

            /// <summary>
            /// Vital Signs section name
            /// </summary>
            public const string SECT_VITAL_SIGNS = "VITAL SIGNS";
            #endregion

            public const string DATA_SOURCE_MOBILE = "M";
            public const string DATA_SOURCE_EMAR = "E";
        }
    }
}