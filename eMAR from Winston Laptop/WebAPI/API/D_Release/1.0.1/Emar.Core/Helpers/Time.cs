using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to help with time logic
    /// </summary>
    public class Time : ITimeUtility
    {
        /// <summary>
        /// Site id associated with this time instance
        /// </summary>
        public byte Site { get; set; }

        /// <summary>
        /// Short date format associated with this time instance
        /// </summary>
        private string ShortDateFormat { get; set; }

        /// <summary>
        /// Long date format associated with this time instance
        /// </summary>
        private string LongDateFormat { get; set; }

        /// <summary>
        /// Dictionary that defines how to divide to convert seconds into different units of time
        /// </summary>
        private static Dictionary<string, int> Divisors = new Dictionary<string, int>
        {
            { Constants.SECONDS, 1 },
            { Constants.MINUTES, Constants.SECONDS_IN_MINUTE },
            { Constants.HOURS,   Constants.SECONDS_IN_HOUR },
            { Constants.DAYS,    Constants.SECONDS_IN_DAY }
        };

        #region date formatting functions
        /// <summary>
        /// Get the day of the year
        /// </summary>
        private const string DayOfYearFunction = "DayOfYear";

        /// <summary>
        /// Get the day of the week
        /// </summary>
        private const string DayOfWeekFunction = "DayOfWeek";

        /// <summary>
        /// Get the week of the year
        /// </summary>
        private const string WeekNumberFunction = "GetWeekOfYear";

        /// <summary>
        /// Get the locale's short date
        /// </summary>
        private const string LocalShortDateFunction = "ShortDatePattern";

        /// <summary>
        /// Get the time zone name
        /// </summary>
        private const string TimeZoneFunction = "TimeZoneNames.Id";
        #endregion

        #region date formatting constants
        /// <summary>
        /// Constant that tells format function to generate a long date
        /// </summary>
        private const string LONGDATE = "LD";

        /// <summary>
        /// Constant that tells format function to generate a short time
        /// </summary>
        private const string SHORTTIME = "ST";

        /// <summary>
        /// Constant that tells format function to generate a short date
        /// </summary>
        private const string SHORTDATE = "SD";
        #endregion

        /// <summary>
        /// Dictionary that defines how to convert Perl formatters used in strftime, to their equivalent .Net formats.
        /// Note that this only supports what PulseCheck supported at the time of implementation. There are other formats
        /// available to strftime, but we don't expect to see them used.
        /// </summary>
        private static Dictionary<string, string> PerlToNetFormats = new Dictionary<string, string>
        {
            { "%b", "MMM" },                    // Abbreviated month name in current locale
            { "%B", "MMMM" },                   // Full month name in current locale
            { "%d", "dd" },                     // The day of the month as a decimal number (01-31)
            { "%m", "MM" },                     // Two-digit month
            { "%y", "yy" },                     // Two-digit year
            { "%Y", "yyyy" },                   // Four-digit year
            { "%j", DayOfYearFunction },        // Day of the year, zero-padded
            { "%w", DayOfWeekFunction },        // Day of week, starting with Sunday as 0
            { "%a", "ddd" },                    // Abbreviated day name
            { "%A", "dddd" },                   // Full day name
            { "%U", WeekNumberFunction },       // Week number, using Sunday as the first day of the week
            { "%W", WeekNumberFunction },       // Week number, using Monday as the first day of the week
            { "%x", LocalShortDateFunction },   // Locale's date representation
            { "%Z", TimeZoneFunction }          // Time zone name
        };

        private static Regex numRE = new Regex(@"^\d+$", RegexOptions.Compiled);

        #region Time constructors
        /// <summary>
        /// Default empty constructor. In the future, will need to know how to deal with timezones.
        /// </summary>
        public Time()
        {

        }

        /// <summary>
        /// Constructor with site ID, which is necessary for some formatting operations.
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        public Time(byte siteId)
        {
            Site = siteId;
            var dr = new DB.Select
            {
                Sql = "SELECT TOP 1 short_date_fmt, long_date_fmt FROM drs WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = Site }
                }
            }.RunForDataRow();

            ShortDateFormat = dr["short_date_fmt"].ToString();
            LongDateFormat = dr["long_date_fmt"].ToString();
        }
        #endregion

        /// <summary>
        /// Get the current datestamp, in 8-digit format (year, month, day)
        /// </summary>
        /// <returns>8-digit current datestamp string</returns>
        public string Datestamp()
        {
            return DateTime.Now.ToString(Constants.FORMAT_DATESTAMP);
        }

        /// <summary>
        /// Given a date/time string in the YYYYMMDDHHmmss format (or similar, shorter format), get a DateTime object with the same date/time
        /// </summary>
        /// <remarks>This will return DateTime.MinValue for null/empty string input, which makes it good to use when comparing DateTimes. If you want to get a null value back, use DateTimeOrNullFromString instead.</remarks>
        /// <param name="ts">Date/time string</param>
        /// <returns>DateTime? object</returns>
        public static DateTime? DateTimeFromString(string ts)
        {
            if (!ValidDateTimeString(ts))
            {
                return DateTime.MinValue;
            }
            return StringToDateTime(ts);
        }

        /// <summary>
        /// Given a date/time string in the YYYYMMDDHHmmss format (or similar, shorter format), get a DateTime object with the same date/time
        /// </summary>
        /// <remarks>This differs from DateTimeFromString in that instead of returning DateTime.MinValue for null/empty string inputs, it returns null.</remarks>
        /// <param name="ts">Date/time string</param>
        /// <returns>DateTime? object or null</returns>
        public static DateTime? DateTimeOrNullFromString(string ts)
        {
            var trimmed = ts != null ? ts.TrimEnd() : null;
            if (!ValidDateTimeString(trimmed))
            {
                return null;
            }
            return StringToDateTime(trimmed);
        }

        /// <summary>
        /// Given a DateTime object, get a string in the YYYYMMDDHHmmss format with the same date/time
        /// </summary>
        /// <param name="dt">DateTime object</param>
        /// <returns>YYYYMMDDHHmmss string</returns>
        public string DateTimeToString(DateTime? dt)
        {
            if (dt == null || !dt.HasValue)
            {
                return Timestamp();
            }

            return dt.Value.ToString(Constants.FORMAT_TIMESTAMP);
        }

        /// <summary>
        /// Given a DateTime object, get a string in the YYYYMMDDHHmm format with the same date/time
        /// </summary>
        /// <param name="dt">DateTime object</param>
        /// <returns>YYYYMMDDHHmm string</returns>
        public string DateTimeToStringNoSeconds(DateTime? dt)
        {
            return DateTimeToString(dt).Substring(0, 12);
        }

        /// <summary>
        /// Return the number of minutes elapsed between two date/timestamps
        /// </summary>
        /// <param name="time1">First date/timestamp</param>
        /// <param name="time2">Second date/timestamp. If null, current date/timestamp is used.</param>
        /// <returns>Integer difference in minutes between two date/timestamps</returns>
        public int DiffMinutes(string time1, string time2 = null)
        {
            return Diff(Constants.MINUTES, time1, time2);
        }

        /// <summary>
        /// Return the number of seconds elapsed between two date/timestamps
        /// </summary>
        /// <param name="time1">First date/timestamp</param>
        /// <param name="time2">Second date/timestamp. If null, current date/timetstamp is used.</param>
        /// <returns>Integer difference in seconds between two date/timestamps</returns>
        public int DiffSeconds(string time1, string time2 = null)
        {
            return Diff(Constants.SECONDS, time1, time2);
        }

        /// <summary>
        /// Determine the difference (in provided units) between two date/timestamps.
        /// </summary>
        /// <param name="unit">Unit identifier (from constants)</param>
        /// <param name="time1">First date/timestamp</param>
        /// <param name="time2">Second date/timestamp</param>
        /// <returns>Integer difference value</returns>
        private int Diff(string unit, string time1, string time2 = null)
        {
            if (time2 == null || String.IsNullOrEmpty(time2.Trim()))
            {
                time2 = Timestamp();
            }

            time1 = time1.Trim();
            time2 = time2.Trim();

            int divisor = Divisors.ContainsKey(unit) ? Divisors[unit] : 1;

            DateTime dt1 = (DateTime)DateTimeFromString(time1);
            DateTime dt2 = (DateTime)DateTimeFromString(time2);

            var secondsDiff = Math.Abs((dt1 - dt2).TotalSeconds);
            var unitDiff = secondsDiff / divisor;

            return (int)unitDiff;
        }

        /// <summary>
        /// Get the formatting string that should be used for a provided date/time stamp, based on its length.
        /// </summary>
        /// <param name="ts">Date/time stamp</param>
        /// <returns>Formatting constant string</returns>
        private static string GetFormattingString(string ts)
        {
            ts = ts.Trim();
            return ts.Length == 14 ? Constants.FORMAT_TIMESTAMP :
                    ts.Length == 12 ? Constants.FORMAT_TIMESTAMP_NO_SECONDS :
                    ts.Length == 8 ? Constants.FORMAT_DATESTAMP :
                    Constants.FORMAT_TIMESTAMP;
        }

        /// <summary>
        /// Given a date time string, return the long date (Defined by org table setting)
        /// </summary>
        /// <param name="ts">Optional timestamp. Defaults to current date/time</param>
        /// <returns>Formatted long date</returns>
        public string LongDate(string ts = null)
        {
            return Format(LONGDATE, ts);
        }

        /// <summary>
        /// Given a date/time string, return the short long date and time (Defined by org table setting, HH:mm)
        /// </summary>
        /// <param name="ts">Optional timestamp. Defaults to current date/time</param>
        /// <returns>Formatted long date time</returns>
        public string LongDateTime(string ts = null)
        {
            return (LongDate(ts) + " " + ShortTime(ts)).Trim();
        }

        /// <summary>
        /// Given a date time string, return the short date (MM/DD/YYYY)
        /// </summary>
        /// <param name="ts">Optional timestamp. Defaults to current date/time</param>
        /// <returns>Formatted short date</returns>
        public string ShortDate(string ts = null)
        {
            return Format(SHORTDATE, ts);
        }

        /// <summary>
        /// Given a date/time string, return the short date time (MM/DD/YYYY HH:mm)
        /// </summary>
        /// <param name="ts">Optional timestamp. Defaults to current date/time</param>
        /// <returns>Formatted short date time</returns>
        public string ShortDateTime(string ts = null)
        {
            return (ShortDate(ts) + " " + ShortTime(ts)).Trim();
        }

        /// <summary>
        /// Given a date time string, return the short time (HH:mm)
        /// </summary>
        /// <param name="ts">Optional timestamp. Defaults to current date/time</param>
        /// <returns>Formatted short time</returns>
        public string ShortTime(string ts = null)
        {
            if (ts != null && (ts.Length == 4 || ts.Length == 6))
            {
                var now = (new Time()).Timestamp();
                ts = now.Substring(0, 8) + ts;
            }
            return Format(SHORTTIME, ts);
        }

        /// <summary>
        /// Format a provided date/time string using the format type specified
        /// </summary>
        /// <param name="formatType">Type of formatting to perform</param>
        /// <param name="ts">Date/time string to format</param>
        /// <returns></returns>
        private string Format(string formatType, string ts = null)
        {
            if (String.IsNullOrEmpty(ts))
            {
                ts = Timestamp();
            }

            var matchString = "";
            switch (formatType)
            {
                case LONGDATE:
                    matchString = String.IsNullOrEmpty(LongDateFormat) ? "%a %b %d %Y" : LongDateFormat;
                    break;

                case SHORTTIME:
                    if (CultureInfo.CurrentCulture.Name.ToLowerInvariant().Contains("en-us"))
                    {
                        matchString = "HH:mm";
                    }
                    else
                    {
                        matchString = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                    }
                    break;

                case SHORTDATE:
                    matchString = String.IsNullOrEmpty(ShortDateFormat) ? "%x" : ShortDateFormat;
                    break;

                default:
                    break;
            }

            var dt = (DateTime)DateTimeFromString(ts);
            var formattedDateTime = ts;
            if (matchString.Length > 0)
            {
                // Pull % formatting characters (Perl) out of matchString and replace them with .Net formats
                var finalFormatString = matchString;

                foreach (Match m in Regex.Matches(matchString, @"(%.)"))
                {
                    var mText = m.Groups[1].Value;
                    var replacement = PerlToNetFormats.ContainsKey(mText) ? PerlToNetFormats[mText] : mText;
                    switch (replacement)
                    {
                        case DayOfYearFunction:
                            finalFormatString = finalFormatString.Replace(mText, dt.DayOfYear.ToString());
                            break;

                        case DayOfWeekFunction:
                            finalFormatString = finalFormatString.Replace(mText, ((int)dt.DayOfWeek).ToString());
                            break;

                        case WeekNumberFunction:
                            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                            Calendar cal = dfi.Calendar;
                            var firstDOfW = mText.Equals("%W") ? DayOfWeek.Monday : DayOfWeek.Sunday;
                            finalFormatString = finalFormatString.Replace(mText, cal.GetWeekOfYear(dt, dfi.CalendarWeekRule, firstDOfW).ToString());
                            break;

                        case LocalShortDateFunction:
                            finalFormatString = finalFormatString.Replace(mText, String.Format("{0:d}", dt));
                            break;

                        case TimeZoneFunction:
                            var tzName = TimeZoneInfo.Local.IsDaylightSavingTime(dt) ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.StandardName;
                            finalFormatString = finalFormatString.Replace(mText, tzName);
                            break;

                        default:
                            finalFormatString = finalFormatString.Replace(mText, replacement);
                            break;
                    }
                }
                formattedDateTime = dt.ToString(finalFormatString, CultureInfo.CurrentCulture);
            }

            return formattedDateTime;
        }

        /// <summary>
        /// Convert a provided string to a DateTime? object
        /// </summary>
        /// <remarks>This method assumes that the string has already been validated and will parse properly</remarks>
        /// <param name="ts">Date/time string</param>
        /// <returns>DateTime? object</returns>
        private static DateTime? StringToDateTime(string ts)
        {
            string fmt = GetFormattingString(ts);
            return DateTime.ParseExact(ts, fmt, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// .NET implementation of Perl/UNIX 'time' command.
        /// </summary>
        /// <returns> Number of seconds elapsed since January 1, 1970 GMT.</returns>
        public Int32 time()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        /// <summary>
        /// Get the current timestamp, in 14-digit format.
        /// </summary>
        /// <returns>14-digit current timestamp string</returns>
        // TODO: This does not support timezones. It might need to.
        public string Timestamp()
        {
            return DateTime.Now.ToString(Constants.FORMAT_TIMESTAMP);
        }

        /// <summary>
        /// Get the current timestamp, in 12-digit format (no seconds).
        /// </summary>
        /// <returns>12-digit current timestamp string</returns>
        // TODO: This does not support timezones. It might need to.
        public string TimestampNoSeconds()
        {
            return DateTime.Now.ToString(Constants.FORMAT_TIMESTAMP_NO_SECONDS);
        }

        /// <summary>
        /// Check whether a provided timestamp looks like a valid format
        /// </summary>
        /// <param name="ts">Date/time string to check</param>
        /// <returns>Boolean for whether the string is valid</returns>
        private static bool ValidDateTimeString(string ts)
        {
            // Null or whitespace strings are invalid
            if (string.IsNullOrWhiteSpace(ts))
            {
                return false;
            }

            // Strings that are not entirely numeric are invalid
            if (!numRE.IsMatch(ts))
            {
                return false;
            }

            // Strings with an odd length or a length less than 6 are invalid
            if (ts.Length % 2 != 0 || ts.Length < 6)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Constants used in Time
        /// </summary>
        public class Constants
        {
            #region Format constants
            // --- FORMAT CONSTANTS --- //

            /// <summary>
            /// Format for 14-digit timestamp
            /// </summary>
            public const string FORMAT_TIMESTAMP = "yyyyMMddHHmmss";

            /// <summary>
            /// Format for 12-digit (no seconds) timestamp
            /// </summary>
            public const string FORMAT_TIMESTAMP_NO_SECONDS = "yyyyMMddHHmm";

            /// <summary>
            /// Format for 8-digit (year, month, day) datestamp
            /// </summary>
            public const string FORMAT_DATESTAMP = "yyyyMMdd";
            #endregion

            #region unit constants
            // --- UNIT CONSTANTS --- //

            /// <summary>
            /// Number of seconds in a minute
            /// </summary>
            public const int SECONDS_IN_MINUTE = 60;

            /// <summary>
            /// Number of minutes in an hour
            /// </summary>
            public const int MINUTES_IN_HOUR = 60;

            /// <summary>
            /// Number of hours in a day
            /// </summary>
            public const int HOURS_IN_DAY = 24;

            /// <summary>
            /// Number of seconds in an hour
            /// </summary>
            public const int SECONDS_IN_HOUR = MINUTES_IN_HOUR * SECONDS_IN_MINUTE;

            /// <summary>
            /// Number of seconds in a day
            /// </summary>
            public const int SECONDS_IN_DAY = SECONDS_IN_HOUR * HOURS_IN_DAY;
            #endregion

            #region unit id constants
            // --- UNIT IDENTIFIER CONSTANTS --- //

            /// <summary>
            /// Seconds identifier
            /// </summary>
            public const string SECONDS = "S";

            /// <summary>
            /// Minutes identifier
            /// </summary>
            public const string MINUTES = "M";

            /// <summary>
            /// Hours identifier
            /// </summary>
            public const string HOURS = "H";

            /// <summary>
            /// Days identifier
            /// </summary>
            public const string DAYS = "D";
            #endregion
        }
    }
}
