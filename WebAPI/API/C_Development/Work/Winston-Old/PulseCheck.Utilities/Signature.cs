using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using PulseCheck.IDomain;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle digital signatures
    /// </summary>
    public static class Signature
    {
        /// <summary>
        /// Creates queue file that the digital signature daemon uses to process a signature request
        /// </summary>
        /// <param name="root">Root directory of PulseCheck application</param>
        /// <param name="patient">IPatient instance</param>
        /// <param name="user">IUser instance</param>
        /// <param name="losecs">Losecs value of signature request entry</param>
        /// <param name="retry">Optional count of current retry attempt</param>
        /// <returns>Null string on success, non-null string on failure</returns>
        public static string CreateQueueFile(string root, IPatient patient, IUser user, int losecs, int retry = 0)
        {
            var errMsg = "";
            if (user == null || user.SiteId == 0)
            {
                errMsg += "Missing site number. ";
            }
            if (patient == null || patient.Ibex == null)
            {
                errMsg += "Missing ibex number. ";
            }
            if (user == null || user.Id == 0)
            {
                errMsg += "Missing user number. ";
            }
            if (losecs == 0)
            {
                errMsg += "Missing losecs number. ";
            }
            if (errMsg.Length > 0)
            {
                return errMsg;
            }

            // Naming convention for dig sig request files:
            //
            // {site #}_{ibex #}_{drs #}_{losecs}{_R{retry #}}?
            var queueFile = root + "\\link\\digsig\\" + user.SiteId + "_" + patient.Ibex + "_" + user.Id + "_" + losecs;
            if (retry > 0)
            {
                queueFile += "_R" + retry;
            }

            if (File.Create(queueFile) == null)
            {
                return "Unable to create queue file " + queueFile;
            }

            return null;
        }

        /// <summary>
        /// Get bytes from user key file
        /// </summary>
        /// <param name="pemString">Content of user key file</param>
        /// <param name="section">Section name within key file</param>
        /// <returns></returns>
        private static byte[] GetBytesFromPEM(string pemString, string section)
        {
            var header = String.Format("-----BEGIN {0}-----", section);
            var footer = String.Format("-----END {0}-----", section);

            var start = pemString.IndexOf(header, StringComparison.Ordinal);
            if (start < 0)
            {
                return null;
            }

            start += header.Length;
            var end = pemString.IndexOf(footer, start, StringComparison.Ordinal) - start;

            if (end < 0)
            {
                return null;
            }

            return Convert.FromBase64String(pemString.Substring(start, end));
        }

        /// <summary>
        /// Calculates the time the signature request was made. This is done by taking the triage time
        /// and adding the losecs.
        /// </summary>
        /// <param name="ibex">Patient identifier / triage time</param>
        /// <param name="losecs">Losecs value</param>
        /// <returns>YYYYMMDDHHmmSS timestamp</returns>
        public static string GetSignatureDate(string ibex, int losecs)
        {
            var _t = new Time();
            var triageDT = Time.DateTimeFromString(ibex);
            if (triageDT.HasValue)
            {
                triageDT.Value.AddSeconds(losecs);
                return _t.DateTimeToString(triageDT.Value);
            }
            return "UNKNOWN";
        }

        /// <summary>
        /// Queue signature request for processing by digital signature daemon
        /// </summary>
        /// <param name="patient">IPatient instance</param>
        /// <param name="user">IUser instance</param>
        /// <returns>Null string for success, non-null string on error</returns>
        public static string QueueNewChartForSigning(IPatient patient, IUser user)
        {
            var siteInfo = new DB.Select
            {
                Sql = "SELECT root FROM org WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId }
                }
            }.RunForDataRow();

            if (siteInfo == null)
            {
                return "Invalid site identifier";
            }

            var root = siteInfo["root"].ToString().Trim();
            var keyFile = root + "sign\\" + user.Id + ".prv.decrypt";
            if (!File.Exists(keyFile))
            {
                return "Private key file does not exist";
            }

            var fileContent = File.ReadAllText(keyFile);
            var keyBytes = GetBytesFromPEM(fileContent, "DSA PRIVATE KEY");
            if (keyBytes == null)
            {
                return "Private key error";
            }

            AsymmetricKeyParameter pubSpec, privSpec;
            Asn1Sequence seq = (Asn1Sequence)Asn1Object.FromByteArray(keyBytes);
            DerInteger p = (DerInteger)seq[1];
            DerInteger q = (DerInteger)seq[2];
            DerInteger g = (DerInteger)seq[3];
            DerInteger y = (DerInteger)seq[4];
            DerInteger x = (DerInteger)seq[5];

            DsaParameters parameters = new DsaParameters(p.Value, q.Value, g.Value);

            privSpec = new DsaPrivateKeyParameters(x.Value, parameters);
            pubSpec = new DsaPublicKeyParameters(y.Value, parameters);

            if (new AsymmetricCipherKeyPair(pubSpec, privSpec) == null)
            {
                return "Private key error";
            }

            // Write trx record
            var losecs = Trx(patient, user.SiteId, user.Id);
            if (losecs <= 0)
            {
                return "Could not write transaction record";
            }

            // Remove sigaud record
            var result = new DB.Update
            {
                Sql = "DELETE FROM sigaud WHERE ibex=@ibex AND usr=@usr AND site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patient.Ibex },
                    new SqlParameter("@usr", SqlDbType.Int) { Value = user.Id },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId }
                }
            }.Run();

            // Write signature pending record to chart
            var name = user.LastName + ", " + user.FirstName;

            var _t = new Time(user.SiteId);
            var line = new EMR.Line
            {
                LineHeader = new EMR.Line.Header
                {
                    sys_time = _t.Timestamp(),
                    user = user.Id,
                    losecs = losecs.ToString(),
                },
                LinePart = new EMR.Line.Part
                {
                    nct = EMR.Constants.NCT_DIG_SIG,
                    section = EMR.Constants.SECT_ADMIN,
                    part = "DIGITAL SIGNATURE"
                },
                Data = "^C=" + name + "&" + Constants.SIG_PENDING
            };
            line.ForSigning = true;

            if (!WriteNewLineToChart(user.SiteId, patient, line))
            {
                UndoChartSigning(root, patient, user, losecs);
                return "Error writing " + Constants.SIG_PENDING + " to chart";
            }

            var msg = CreateQueueFile(root, patient, user, losecs);
            if (msg != null)
            {
                UndoChartSigning(root, patient, user, losecs);
                return msg;
            }

            return null;
        }

        /// <summary>
        /// Write a transaction record for signing the chart
        /// </summary>
        /// <param name="patient">IPatient instance</param>
        /// <param name="siteId">Site identifier</param>
        /// <param name="userId">User identifier</param>
        /// <returns>Losecs value of trx entry</returns>
        private static int Trx(IPatient patient, byte siteId, int userId)
        {
            Dictionary<string, object> Values = new Dictionary<string, object>
            {
                { Transaction.Constants.Name, Constants.TRX_NAME },
                { Transaction.Constants.Service, Constants.TRX_SERVICE },
                { Transaction.Constants.Type, Constants.TRX_TYPE }
            };

            var t = new Transaction(siteId, patient, userId, Values, null);
            return t.AddTransaction();
        }

        /// <summary>
        /// Reverse all actions associated with submitting signature request
        /// </summary>
        /// <param name="root">Root directory of PulseCheck application</param>
        /// <param name="patient">IPatient instance</param>
        /// <param name="user">IUser instance</param>
        /// <param name="losecs">Losecs value of signature request entry in chart</param>
        public static void UndoChartSigning(string root, IPatient patient, IUser user, int losecs)
        {
            // Insert record into sigaud table
            var chgdate = GetSignatureDate(patient.Ibex, losecs);
            var result = new DB.Insert
            {
                Sql = "INSERT INTO sigaud (ibex,usr,site,chgdate) VALUES (@ibex, @usr, @site, @chgdate)",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patient.Ibex },
                    new SqlParameter("@usr", SqlDbType.Int) { Value = user.Id },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId },
                    new SqlParameter("@chgdate", SqlDbType.Char) { Value = chgdate }
                }
            }.Run();

            // Delete trx record
            result = new DB.Update
            {
                Sql = "DELETE FROM trx WHERE ibex=@ibex AND site=@site AND type=@type AND service=@service AND name=@name AND losecs=@losecs",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@ibex", SqlDbType.Char) { Value = patient.Ibex },
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = user.SiteId },
                    new SqlParameter("@type", SqlDbType.Char) { Value = Constants.TRX_TYPE },
                    new SqlParameter("@service", SqlDbType.Int) { Value = Constants.TRX_SERVICE },
                    new SqlParameter("@name", SqlDbType.VarChar) { Value = Constants.TRX_NAME },
                    new SqlParameter("@losecs", SqlDbType.Int) { Value = losecs }
                }
            }.Run();

            // Search chart for all signable entries for this user and inactivate pending signature entry
            var emr = new EMR(user.SiteId, patient.Ibex);
            var lineNumber = 0;
            foreach(var line in emr.Lines)
            {
                if (line.IsInactive() || line.User() != user.Id)
                {
                    continue;
                }
                if (line.NCT() == EMR.Constants.NCT_DIG_SIG && line.Losecs() == losecs.ToString())
                {
                    lineNumber = line.LineNumber;
                }
            }
            if (lineNumber > 0)
            {
                emr.WriteLine(lineNumber, user.Id);
            }
        }

        /// <summary>
        /// Writes the given line to the chart
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="patient">IPatient instance</param>
        /// <param name="line">EMR.Line instance</param>
        /// <returns>Boolean flag for success/failure</returns>
        private static bool WriteNewLineToChart(byte siteId, IPatient patient, EMR.Line line)
        {
            var EMR = new EMR(siteId, patient.Ibex, true);
            return EMR.WriteLine(line);
        }

        /// <summary>
        /// Signature constants
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// "Signature Pending" text written to chart
            /// </summary>
            public const string SIG_PENDING = "SIGNATURE PENDING";

            /// <summary>
            /// Name of digital signature trx entry
            /// </summary>
            public const string TRX_NAME = "Digital Signature";

            /// <summary>
            /// Service number of digital signature trx entry
            /// </summary>
            public const int TRX_SERVICE = 302;

            /// <summary>
            /// Type value of digital signature trx entry
            /// </summary>
            public const string TRX_TYPE = "I";
        }
    }
}