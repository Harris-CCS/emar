using Interfaces.DomainModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to work with interface triggers
    /// </summary>
    public static class Trigger
    {
        /// <summary>
        /// Create or update interface trigger file according to interface and action performed. If the method fails, an error
        /// message will be returned to the caller. On success, null will be returned.
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="userId">User identifier</param>
        /// <param name="triggerContent">String of information to write to trigger</param>
        /// <param name="interfaceName">Interface type (ie bil)</param>
        /// <param name="sourceName">What is requesting the trigger (ie 38, 91, ...)</param>
        /// <param name="append">Whether an existing trigger should have this content appended</param>
        /// <returns>Error message on error, null on success</returns>
        public static string Create(ISite site, string patientId, int userId, string triggerContent, string interfaceName, string sourceName, bool append = false)
        {
            // Check for missing arguments
            if (string.IsNullOrWhiteSpace(patientId)) {
                return MissingArgumentError("create", "Patient identifier");
            } else if (string.IsNullOrWhiteSpace(interfaceName))
            {
                return MissingArgumentError("create", "Interface name");
            } else if (userId == 0)
            {
                return MissingArgumentError("create", "User identifier");
            }

            // Check for supported interfaces
            if (!Constants.SUPPORTED_INTERFACES.ContainsKey(interfaceName))
            {
                return string.Format(Constants.ERROR_INTERFACE_NOT_SUPPORTED, interfaceName);
            }

            // Only process if interface is enabled
            if (!IsInterfaceEnabled(site, interfaceName))
            {
                return null;
            }

            var formats = GetFormats(site, interfaceName);

            // Are these only DB interface triggers? No trigger files...
            if (Constants.DB_ONLY_TRIGGERS.ContainsKey(interfaceName))
            {
                var error = WriteTriggerDB(site, patientId, userId, interfaceName, sourceName, append);
                if (error != null)
                {
                    return error;
                }
            }

            return null; 
        }

        /// <summary>
        /// Retrieves which format(s) to populate when a trigger is created
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="interfaceName">Interface name</param>
        /// <returns>Int of format constants</returns>
        public static int GetFormats(ISite site, string interfaceName)
        {
            var format = 0;
            if (Constants.TRIGGER_FORMATS.ContainsKey(interfaceName))
            {
                var formatOption = Constants.TRIGGER_FORMATS[interfaceName];
                var formatValue = site.GetOrgOption(formatOption);
                if (formatValue.Equals(Constants.TRIGGER_TEXT))
                {
                    format = Constants.FORMAT_TEXT;
                } else if (formatValue.Equals(Constants.TRIGGER_DB))
                {
                    format = Constants.FORMAT_DB;
                } else if (formatValue.Equals(Constants.TRIGGER_XML))
                {
                    format = Constants.FORMAT_XML;
                } else if (formatValue.Equals(Constants.TRIGGER_BOTH))
                {
                    format = Constants.FORMAT_XML + Constants.FORMAT_TEXT;
                }
            } else
            {
                format = Constants.FORMAT_TEXT;
            }

            return format;
        }

        /// <summary>
        /// Queries site options to check if interface is enabled.
        /// </summary>
        /// <param name="site">ISite instance</param>
        /// <param name="interfaceName">Interface name</param>
        /// <returns>Boolean for whether interface is enabled</returns>
        public static bool IsInterfaceEnabled(ISite site, string interfaceName)
        {
            if (Constants.INTERFACE_ORG_FIELDS.ContainsKey(interfaceName))
            {
                var setting = new DB.Select
                {
                    Sql = "SELECT " + Constants.INTERFACE_ORG_FIELDS[interfaceName] + " FROM org WHERE site=@site",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                    }
                }.RunForScalar().ToString();

                if (!setting.Equals("N"))
                {
                    return true;
                }
            } else if (Constants.INTERFACE_ORG_OPTIONS.ContainsKey(interfaceName))
            {
                var setting = site.GetOrgOption(Constants.INTERFACE_ORG_OPTIONS[interfaceName]);

                if (!setting.Equals("N"))
                {
                    return true;
                }
            }

            return false;
        }

        private static string CheckDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                return null;
            }

            try
            {
                Directory.CreateDirectory(directory);
            } catch (Exception)
            {
                return string.Format(Constants.ERROR_CANNOT_CREATE_DIRECTORY, directory);
            }

            return null;
        }

        private static Dictionary<string, string> GetPatient(ISite site, string patientId)
        {
            var res = new DB.Select
            {
                Sql = "SELECT * FROM pat WHERE site=@site AND ibex=@ibex",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id },
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId }
                }
            }.RunForDictionary();

            if (res.Count == 0)
            {
                res = new DB.Select
                {
                    Sql = "SELECT * FROM hst WHERE site=@site AND ibex=@ibex",
                    Parameters = new SqlParameter[]
                                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id },
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patientId }
                                }
                }.RunForDictionary();

                res["archive"] = "1";
            }

            return res;
        }

        private static string GetTPEDMessageId(byte siteId, string interfaceName, Dictionary<string, string> patient)
        {
            var firstName = patient.ContainsKey("fname") ? patient["fname"] : "";
            var lastName = patient.ContainsKey("lname") ? patient["lname"] : "";

            if (string.IsNullOrWhiteSpace(firstName))
            {
                return MissingArgumentError("GetTPEDMessageId", "Patient first name");
            } else if (string.IsNullOrWhiteSpace(lastName))
            {
                return MissingArgumentError("GetTPEDMessageId", "Patient last name");
            }

            if (!Constants.INSTANCE_ID_BY_INTERFACE.ContainsKey(interfaceName) || !Constants.MESSAGE_ID_PATH.ContainsKey(interfaceName))
            {
                return null;
            }

            var _t = new Time();
            var received = _t.Timestamp();
            var arguments = new Dictionary<string, string>
            {
                { "patient_fname", firstName },
                { "patient_lname", lastName },
                { "patient_acctnum", patient["acctnum"] },
                { "patient_medrec", patient["medrec"] },
                { "received_datetime", received },
                { "received_hour", received.Substring(8, 2) },
                { "received_date", received.Substring(0, 8) },
                { "instance_id", Constants.INSTANCE_ID_BY_INTERFACE[interfaceName].ToString() },
                { "path", Constants.MESSAGE_ID_PATH[interfaceName] },
                { "site", siteId.ToString() }
            };

            var sql = @"INSERT INTO touchpoint.dbo.pie_hl7_log (
                patient_fname, patient_lname, patient_acctnum, patient_medrec, 
                received_datetime, received_hour, received_date, instance_id, path, site
            ) VALUES (
                @patient_fname, @patient_lname, @patient_acctnum, @patient_medrec,
                @received_datetime, @received_hour, @received_date, @instance_id, @path, @site
            );SELECT @@IDENTITY AS message_id";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@patient_fname", SqlDbType.VarChar) { Value = arguments["patient_fname"] },
                new SqlParameter("@patient_lname", SqlDbType.VarChar) { Value = arguments["patient_lname"] },
                new SqlParameter("@patient_acctnum", SqlDbType.VarChar) { Value = arguments["patient_acctnum"] },
                new SqlParameter("@patient_medrec", SqlDbType.VarChar) { Value = arguments["patient_medrec"] },
                new SqlParameter("@received_datetime", SqlDbType.Char) { Value = arguments["received_datetime"] },
                new SqlParameter("@received_hour", SqlDbType.Char) { Value = arguments["received_hour"] },
                new SqlParameter("@received_date", SqlDbType.Char) { Value = arguments["received_date"] },
                new SqlParameter("@instance_id", SqlDbType.Int) { Value = arguments["instance_id"] },
                new SqlParameter("@path", SqlDbType.VarChar) { Value = arguments["path"] },
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = arguments["site"] }
            };

            string messageId = null;
            try
            {
                messageId = new DB.Insert
                {
                    Sql = sql,
                    Parameters = parameters.ToArray()
                }.Run().ToString();
            } catch (Exception e)
            {
                return string.Format(
                    "Unable to create trigger in database table. Sql({0}) with values({1}) created error({2}).",
                    sql,
                    string.Join(";", arguments),
                    e.Message
                );
            }

            return messageId;
        }

        private static string GetTriggerDirectory(ISite site, string interfaceName)
        {
            if (Constants.TRIGGER_DIRECTORIES.ContainsKey(interfaceName))
            {
                return site.Root.Trim() + "\\" + Constants.TRIGGER_DIRECTORIES[interfaceName];
            }

            return null;
        }

        private static string InvalidValueError(string valueName)
        {
            return string.Format(Constants.ERROR_INVALID_VALUE, valueName);
        }

        private static string MissingArgumentError(string methodName, string missingArgumentDescription)
        {
            return string.Format(Constants.ERROR_MISSING_ARGUMENT, methodName, missingArgumentDescription);
        }

        private static string WriteTriggerDB(ISite site, string patientId, int userId, string interfaceName, string sourceName, bool append = false)
        {
            // Verify source is valid
            if (!Constants.MAP_INTERFACE_SOURCE_TO_ACTION.ContainsKey(sourceName))
            {
                return string.Format(Constants.ERROR_INVALID_SOURCE, sourceName);
            }

            // Validate patient information
            var patient = GetPatient(site, patientId);
            if (!patient.ContainsKey("acctnum") || string.IsNullOrWhiteSpace(patient["acctnum"]))
            {
                return InvalidValueError("patient account number");
            } else if (!patient.ContainsKey("medrec") || string.IsNullOrWhiteSpace(patient["medrec"]))
            {
                return InvalidValueError("patient medical record number");
            }

            // Determine event action and account for chart coding vs. archive chart coding.
            var eventAction = Constants.MAP_INTERFACE_SOURCE_TO_ACTION[sourceName];
            if (patient.ContainsKey("archive") && patient["archive"].Equals("1") && eventAction.IndexOf("CHART_CODING") >= 0)
            {
                eventAction = "ARCHIVE_" + eventAction;
            }

            var messageId = GetTPEDMessageId(site.Id, interfaceName, patient);

            

            return null;
        }

        private static string WriteTriggerText(ISite site, string patientId, string triggerContent, string interfaceName, bool append = false)
        {
            if (interfaceName.Equals(Constants.HIE) && site == null)
            {
                return MissingArgumentError("WriteTriggerText", "Site");
            }

            // Get trigger directory
            var directory = GetTriggerDirectory(site, interfaceName);
            var error = CheckDirectory(directory);
            if (!string.IsNullOrWhiteSpace(error))
            {
                return error;
            }

            // Write trigger file
            var filename = directory + "\\" + patientId;
            FileWriter.Write(filename, triggerContent, append);

            return null;
        }

        private static string WriteTriggerXML()
        {
            return null;
        }
 
        /// <summary>
        /// Constants used in Triggers
        /// </summary>
        public class Constants
        {
            #region Trigger type constants
            public const string TRIGGER_MED_SVC_HL7 = "H";
            public const string TRIGGER_MED_SVC_IMAGE = "I";
            public const string TRIGGER_MED_SVC_BOTH = "B";
            public const string TRIGGER_MED_SVC_NONE = "N";
            #endregion

            #region Trigger format constants
            internal const int FORMAT_DB = 2;
            internal const int FORMAT_TEXT = 1;
            internal const int FORMAT_XML = 2;
            #endregion

            #region Trigger format type constants
            internal const string TRIGGER_TEXT = "T";
            internal const string TRIGGER_XML = "X";
            internal const string TRIGGER_DB = "D";
            internal const string TRIGGER_BOTH = "B";
            #endregion

            #region Error message constants
            internal const string ERROR_CANNOT_CREATE_DIRECTORY = "Unable to create interface trigger file. Cannot create directory '{0}'.";
            internal const string ERROR_INTERFACE_NOT_SUPPORTED = "Unable to create interface trigger. Interface '{0}' is not supported.";
            internal const string ERROR_INVALID_SOURCE = "Unable to write interface trigger file. Interface source '{0}' is invalid.";
            internal const string ERROR_INVALID_VALUE = "Unable to create trigger. Missing {0}.";
            internal const string ERROR_MISSING_ARGUMENT = "Unable to create interface trigger. Method {0} is missing required argument {1}.";
            #endregion

            #region Interface identifier constants
            public const string ADMIN_UPDATE = "admin";
            public const string DISCRETE = "cln";
            public const string ECODE = "ecd";
            public const string EXTRANET = "extranet";
            public const string FACILITY_BILLING = "chg";
            public const string HIE = "hie";
            public const string PHYSICIAN_BILLING = "bil";
            public const string PULSEVIEW = "pvw";
            public const string EMR_TEXT = "txt";
            public const string MEDICATION_SERVICE_HL7 = "medhl7";
            public const string MEDICATION_SERVICE_IMAGE = "medimage";
            #endregion

            #region Supported interfaces
            /// <summary>
            /// Interfaces that can create trigger files
            /// </summary>
            internal static readonly Dictionary<string, int> SUPPORTED_INTERFACES = new Dictionary<string, int>
            {
                { DISCRETE, 1 },
                { ECODE, 1 },
                { EXTRANET, 1 },
                { HIE, 1 },
                { FACILITY_BILLING, 1 },
                { PHYSICIAN_BILLING, 1 },
                { PULSEVIEW, 1 },
                { EMR_TEXT, 1 },
                { MEDICATION_SERVICE_HL7, 1 },
                { MEDICATION_SERVICE_IMAGE, 1 }
            };
            #endregion

            #region Interface settings
            /// <summary>
            /// Interface trigger creation formats
            /// </summary>
            internal static readonly Dictionary<string, string> TRIGGER_FORMATS = new Dictionary<string, string>
            {
                { DISCRETE, "TRIGGER_CLN" },
                { ECODE, "TRIGGER_ECD" },
                { FACILITY_BILLING, "TRIGGER_CHG" },
                { PHYSICIAN_BILLING, "TRIGGER_BIL" },
                { EMR_TEXT, "TRIGGER_TXT" },
                { MEDICATION_SERVICE_HL7, "TRIGGER_MED_HSF" },
                { MEDICATION_SERVICE_IMAGE, "TRIGGER_MED_HSF" }
            };

            /// <summary>
            /// Map interface name to org field that turns the interface on and off
            /// </summary>
            internal static readonly Dictionary<string, string> INTERFACE_ORG_FIELDS = new Dictionary<string, string>
            {
                { ECODE, "gotecode" },
                { EXTRANET, "gotextra" },
                { HIE, "gothie" },
                { FACILITY_BILLING, "gotchg" },
                { PHYSICIAN_BILLING, "gotbil" },
                { PULSEVIEW, "gotpulseview" },
                { EMR_TEXT, "gottxt" },
                { MEDICATION_SERVICE_HL7, "gotmeds" },
                { MEDICATION_SERVICE_IMAGE, "gotmeds" }
            };

            /// <summary>
            /// Map interface name to interface trigger file directory for text triggers
            /// </summary>
            internal static readonly Dictionary<string, string> TRIGGER_DIRECTORIES = new Dictionary<string, string>
            {
                { DISCRETE, "link\\" + DISCRETE },
                { ECODE, "link\\" + ECODE },
                { EXTRANET, "link\\" + EXTRANET },
                { HIE, "link\\" + HIE },
                { FACILITY_BILLING, "link\\" + FACILITY_BILLING },
                { PHYSICIAN_BILLING, "link\\" + PHYSICIAN_BILLING },
                { PULSEVIEW, "link\\" + PULSEVIEW },
                { EMR_TEXT, "link\\" + EMR_TEXT }
            };

            /// <summary>
            /// Map interface sources to actions
            /// </summary>
            internal static readonly Dictionary<string, string> MAP_INTERFACE_SOURCE_TO_ACTION = new Dictionary<string, string>
            {
                { "38", "REMOVE_FROM_ED" },
                { "91", "MANUAL" },
                { "5a:M", "MANUAL" },
                { "92", "CHART_CODING" },
                { "digsig:A", "CHART_SIGNATURE" },
                { "digsig:D", "MD_SIGNATURE" },
                { "4c", "CHART_ENTRY" },
                { "4q", "MANUAL" },
            };

            /// <summary>
            /// Interface message ID paths
            /// </summary>
            internal static readonly Dictionary<string, string> MESSAGE_ID_PATH = new Dictionary<string, string>
            {
                { FACILITY_BILLING, "FacilityBilling" },
                { EMR_TEXT, "EMRText" },
                { PHYSICIAN_BILLING, "PhysicianBilling" },
                { MEDICATION_SERVICE_HL7, "MedicationServiceHL7" },
                { MEDICATION_SERVICE_IMAGE, "MedicationServiceImage" }
            };

            /// <summary>
            /// Used for pie_hl7_log insert_id column value
            /// </summary>
            internal static readonly Dictionary<string, int> INSTANCE_ID_BY_INTERFACE = new Dictionary<string, int>
            {
                { ADMIN_UPDATE, 0 },
                { FACILITY_BILLING, 200 },
                { EMR_TEXT, 300 },
                { PHYSICIAN_BILLING, 400 },
                { MEDICATION_SERVICE_HL7, 500 },
                { MEDICATION_SERVICE_IMAGE, 500 }
            };

            /// <summary>
            /// Map interface name to org option that turns the interface on and off
            /// </summary>
            internal static readonly Dictionary<string, string> INTERFACE_ORG_OPTIONS = new Dictionary<string, string>
            {
                { DISCRETE, "CLINICAL_INF" }
            };

            /// <summary>
            /// Interfaces that only create DB triggers, no file triggers
            /// </summary>
            internal static readonly Dictionary<string, int> DB_ONLY_TRIGGERS = new Dictionary<string, int>
            {
                { MEDICATION_SERVICE_HL7, 1 },
                { MEDICATION_SERVICE_IMAGE, 1 }
            };

            /// <summary>
            /// Interfaces that can create DB trigger files
            /// </summary>
            internal static readonly Dictionary<string, int> TRIGGER_DB_INTERFACES = new Dictionary<string, int>
            {
                { FACILITY_BILLING, 1 },
                { EMR_TEXT, 1 },
                { PHYSICIAN_BILLING, 1 },
                { MEDICATION_SERVICE_HL7, 1 },
                { MEDICATION_SERVICE_IMAGE, 1 }
            };
            #endregion
        }
    }
}
 