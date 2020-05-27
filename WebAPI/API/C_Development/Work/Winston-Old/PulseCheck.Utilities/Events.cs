using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Web;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to event logging
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// Medication Services identifier for authentication type
        /// </summary>
        public const string ID_MEDSVC = "medsvc";

        /// <summary>
        /// Prescriptions identifier for authentication type
        /// </summary>
        public const string ID_RX = "rx";
        
        /// <summary>
        /// Log api-specific PulseCheck events
        /// </summary>
        /// <param name="accountLogin"></param>
        /// <param name="status"></param>
        /// <param name="ip"></param>
        /// <param name="reason"></param>
        /// <param name="dataSource"></param>
        public static void LogAPIEvent(string eventCode, string accountLogin = null, byte? siteId = null, string reason = null, string ip = null, string dataSource = Constants.DATA_SOURCE_MOBILE)
        {
            if (ip == null && (HttpContext.Current != null && HttpContext.Current.Request != null))
            {
                try
                {
                    ip = HttpContext.Current.Request.UserHostAddress?.ToString();
                }
                catch (UriFormatException)
                {
                }
            }

            var sql = "INSERT INTO api_log(account_login, event_code, site, ip, reason, data_source) VALUES (@account_login, @event_code, @site, @ip, @reason, @data_source)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@event_code", SqlDbType.Char) { Value = eventCode },
                new SqlParameter("@account_login", SqlDbType.VarChar) { Value  = accountLogin ?? SqlString.Null},
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId == null ? DBNull.Value : (object)siteId },
                new SqlParameter("@ip", SqlDbType.Char) { Value = ip ?? SqlString.Null},
                new SqlParameter("@reason", SqlDbType.VarChar) { Value = reason ?? SqlString.Null },
                new SqlParameter("@data_source", SqlDbType.Char) { Value = dataSource },
            };

            try
            {
                new DB.Update
                {
                    Sql = sql,
                    Parameters = parameters
                }.Run();
            }
            catch (SqlException ex)
            {
                DTFL.Write(1, 0, ex, sql, parameters);
            }
        }

        /// <summary>
        /// Log an event
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="userId">User identifier</param>
        /// <param name="reff">Program reporting event</param>
        /// <param name="status">Event type: A=Login, B=duplicate login with same userid, C=not logged in, D=data server failed, E=unhandled exception, F=data write failed, G=not authorized, I=multiple active session for same user, K=parameter fault, M=Logout, N=Password Change, P=Password failure, Q=Account locked, R=proximity badge logout, S=Account created, T=time out, U=forged cookie, V=failed fax, W=Account inactivated, X=Barcode/Medication cross-reference exception</param>
        /// <param name="reason">Detailed explanation of the above status</param>
        public static void Log(byte siteId, int userId, string reff, string status, string reason)
        {
            var t = new Time();
            var logDate = t.Timestamp();
            var logDay = (int)DateTime.Now.DayOfWeek;
            var referrer = "";
            var ip = "";

            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                try
                {
                    referrer = HttpContext.Current.Request.UrlReferrer?.ToString();
                    ip = HttpContext.Current.Request.UserHostAddress?.ToString();
                }
                catch (UriFormatException)
                {

                }
            }

            // TODO: These may actually be needed some day.
            var faxJob = 0;
            var faxKey = 0;

            var sql = "INSERT INTO logg(usr, logdate, logday, site, ref, status, ip, faxjob, faxkey, reason, refer, data_source) VALUES (@usr, @logdate, @logday, @site, @ref, @status, @ip, @faxjob, @faxkey, @reason, @refer, @data_source)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@usr", SqlDbType.Int) { Value  = userId },
                new SqlParameter("@logdate", SqlDbType.Char) { Value = logDate },
                new SqlParameter("@logday", SqlDbType.TinyInt) { Value = logDay },
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                new SqlParameter("@ref", SqlDbType.VarChar) { Value = reff ?? SqlString.Null },
                new SqlParameter("@status", SqlDbType.Char) { Value = status },
                new SqlParameter("@ip", SqlDbType.Char) { Value = ip },
                new SqlParameter("@faxjob", SqlDbType.Int) { Value = faxJob },
                new SqlParameter("@faxkey", SqlDbType.Int) { Value = faxKey },
                new SqlParameter("@reason", SqlDbType.VarChar) { Value = reason ?? SqlString.Null },
                new SqlParameter("@refer", SqlDbType.VarChar) { Value = referrer ?? SqlString.Null },
                new SqlParameter("@data_source", SqlDbType.Char) { Value = Constants.DATA_SOURCE_MOBILE },
            };

            try
            {
                new DB.Update
                {
                    Sql = sql,
                    Parameters = parameters
                }.Run();
            } catch (SqlException ex)
            {
                DTFL.Write(siteId, userId, ex, sql, parameters);
            }
        }

        /// <summary>
        /// Log a failure on Medication Services or Prescriptions
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="userId">User identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="losecs">Losecs value of med ordered for  patient</param>
        /// <param name="action">Action on Med Svc or Rx that failed (e.g., 'hold')</param>
        /// <param name="type">Identifier constant</param>
        /// <param name="drugName">Drug name</param>
        /// <returns>Boolean flag for success or failure</returns>
        public static bool AuthenticationFailure(byte siteId, int userId, string patientId, int losecs, string action, string type, string drugName)
        {
            var sql = "INSERT INTO auth_failures(usr, site, date, ibex, losecs, action, type, name) VALUES (@usr, @site, @date, @ibex, @losecs, @action, @type, @name)";
            var now = DateTime.Now;

            // Try to audit the failure. If we can't, it's because there was already a failure for that user, patient, and action.
            var result = 0;
            try
            {
                result = new DB.Update
                {
                    Sql = sql,
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@usr", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                        new SqlParameter("@date", SqlDbType.DateTime) { Value = now },
                        new SqlParameter("@ibex", SqlDbType.VarChar) { Value = patientId },
                        new SqlParameter("@losecs", SqlDbType.Int) { Value = losecs },
                        new SqlParameter("@action", SqlDbType.VarChar) { Value = action },
                        new SqlParameter("@type", SqlDbType.VarChar) { Value = type },
                        new SqlParameter("@name", SqlDbType.VarChar) { Value = drugName ?? SqlString.Null }
                    }
                }.Run();
            } catch (SqlException)
            {
                result = 0;
            }

            if (result == 0)
            {
                sql = "UPDATE auth_failures SET date = @date WHERE usr = @usr AND ibex = @ibex AND losecs = @losecs AND type = @type";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@date", SqlDbType.DateTime) { Value = now },
                    new SqlParameter("@usr", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@ibex", SqlDbType.VarChar) { Value = patientId },
                    new SqlParameter("@losecs", SqlDbType.Int) { Value = losecs },
                    new SqlParameter("@type", SqlDbType.VarChar) { Value = type }
                };
                try
                {
                    result = new DB.Update
                    {
                        Sql = sql,
                        Parameters = parameters
                    }.Run();
                } catch (SqlException)
                {
                    result = 0;
                }

                if (result == 0)
                {
                    DTFL.Write(siteId, userId, "Failed auth_failures insert/update", sql, parameters);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Clear out a Med Svc or Prescriptions authentication failure
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="userId">User identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="losecs">Losecs value of med ordered for patient</param>
        /// <param name="action">Action on Med Svc or Rx that failed (e.g., 'hold')</param>
        /// <param name="type">Identifier constant</param>
        /// <returns>Boolean flag for success or failure</returns>
        public static bool AuthenticationSuccess(byte siteId, int userId, string patientId, int losecs, string action, string type)
        {
            // Clear the failures from the table for the user, patient, med, and action.
            var sql = "DELETE FROM auth_failures WHERE usr = @usr AND ibex = @ibex AND losecs = @losecs AND site = @site AND action = @action AND type = @type";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@usr", SqlDbType.Int) { Value = userId },
                new SqlParameter("@ibex", SqlDbType.VarChar) { Value = patientId },
                new SqlParameter("@losecs", SqlDbType.Int) { Value = losecs },
                new SqlParameter("@site", SqlDbType.TinyInt) { Value = siteId },
                new SqlParameter("@action", SqlDbType.VarChar) { Value = action },
                new SqlParameter("@type", SqlDbType.VarChar) { Value = type },
            };
            var result = new DB.Update
            {
                Sql = sql,
                Parameters = parameters
            }.Run();

            if (result == 0)
            {
                DTFL.Write(siteId, userId, "", sql, parameters);
                return false;
            }

            return true;
        }

        public class Constants
        {
            public const string DATA_SOURCE_MOBILE = "M";

            public class Events
            {
                public const string LOGIN = "A";
                public const string DUPLICATE_LOGIN = "B";
                public const string NOT_LOGGED_IN = "C";
                public const string DATASERVER_FAILED = "D";
                public const string UNHANDLED_EXCEPTION = "E";
                public const string DATAWRITE_FAILED = "F";
                public const string NOTAUTHORIZED = "G";
                public const string NO_ACCOUNT = "H";
                public const string MULTIPLEACTIVESESSIONS = "I";
                public const string PARAMETERFAULT = "K";
                public const string LOGOUT = "M";
                public const string PASSWORDCHANGE = "N";
                public const string PASSWORDFAILURE = "P";
                public const string ACCOUNTLOCKED = "Q";
                public const string PROXBADGELOGOUT = "R";
                public const string ACCOUNTCREATED = "S";
                public const string TIMEOUT = "T";
                public const string FORGEDCOOKIE = "U";
                public const string FAILEDFAX = "V";
                public const string ACCOUNTINACTIVATED = "W";
                public const string BARCODEMEDXREFEXCEPTION = "X";
                public const string NO_MAPPED_USERS = "Y";
                public const string INVALID_DEVICE = "Z";
            }
        }
    }
}