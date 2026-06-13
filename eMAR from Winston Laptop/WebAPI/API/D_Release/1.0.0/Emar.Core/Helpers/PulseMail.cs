using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Emar.Core.OutboundChart.Model;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to handle sending internal PulseMails
    /// </summary>
    public class PulseMail
    {
        /// <summary>
        /// Sender's Site instance
        /// </summary>
        public ISite Site { get; set; }

        /// <summary>
        /// Site's mailroot value
        /// </summary>
        public string MailRoot { get; set; }

        /// <summary>
        /// List of errors encountered while trying to send a PulseMail
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Create a new PulseMail object
        /// </summary>
        /// <param name="site">ISite instance</param>
        public PulseMail(ISite site)
        {
            Site = site;

            MailRoot = new DB.Select
            {
                Sql = "SELECT mailroot FROM org WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site.Id }
                }
            }.RunForScalar().ToString().Trim();

            ClearErrors();
        }

        /// <summary>
        /// Add a new error to the PulseMail sending error list
        /// </summary>
        /// <param name="error"></param>
        private void AddError(string error)
        {
            Errors.Add(error);
        }

        /// <summary>
        /// Clear the PulseMail sending error list
        /// </summary>
        public void ClearErrors()
        {
            Errors.Clear();
        }

        /// <summary>
        /// Send a new PulseMail
        /// </summary>
        /// <param name="to">Recipient user number</param>
        /// <param name="subject">Subject of message</param>
        /// <param name="message">Message being sent</param>
        /// <param name="from">Sender user number</param>
        /// <param name="otherSite">Optional different site number if sending between sites</param>
        /// <param name="returnReceipt">Optional boolean flag for return receipt</param>
        /// <returns>Boolean flag indicating success of creating the PulseMail</returns>
        public bool SendMessage(int to, string subject, string message, int from, byte otherSite = 0, bool returnReceipt = false)
        {
            ClearErrors();

            if (string.IsNullOrWhiteSpace(subject))
            {
                AddError("Subject is not correctly set");
                return false;
            }
            else
            {
                subject = subject.Trim();
                if (subject.Length > 80)
                {
                    subject = subject.Substring(0, 80);
                }
            }

            byte insertSite = (otherSite > 0) ? otherSite : Site.Id;

            string mailFile = RandomCharacters();

            var _t = new Time(insertSite);
            var sql = "INSERT INTO mal(mid, rec, fr, dateadd, subject, status, rr, site) VALUES (@mid, @rec, @fr, @dateadd, @subject, @status, @rr, @site)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@mid", SqlDbType.Char) { Value = mailFile },
                new SqlParameter("@rec", SqlDbType.Int) { Value = to },
                new SqlParameter("@fr", SqlDbType.Int) { Value = from },
                new SqlParameter("@dateadd", SqlDbType.Char) { Value = _t.TimestampNoSeconds() },
                new SqlParameter("@subject", SqlDbType.VarChar) { Value = subject },
                new SqlParameter("@status", SqlDbType.Char) { Value = "N" },
                new SqlParameter("@rr", SqlDbType.Char) { Value = returnReceipt ? "Y" : "N" },
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = insertSite }
            };

            try
            {
                var result = new DB.Insert
                {
                    Sql = sql,
                    Parameters = parameters
                }.Run();

                if (result != 1)
                {
                    AddError("Failed insert into MAL table");
                    return false;
                }

                var mailDir = MailRoot + "\\" + to;
                var userMailFile = mailDir + "\\" + mailFile;

                message = Regex.Replace(message, @"<BR>", "\n", RegexOptions.IgnoreCase);
                message = message.Trim();
                message = Regex.Replace(message, @"\$\$MAILD\$\$", mailFile);

                FileWriter.Write(userMailFile, message);

            }
            catch (SqlException e)
            {
                DTFL.Write(Site.Id, from, e, sql, parameters);
                AddError("Failed insert into MAL table: " + e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generate a string of random characters, similar to IBEX::Utils::crand()
        /// </summary>
        /// <param name="maxSize">Desired length of random character string</param>
        /// <returns>String of random characters</returns>
        private string RandomCharacters(int maxSize = 8)
        {
            char[] chars = new char[36];
            chars = "abcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}