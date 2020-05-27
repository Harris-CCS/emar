using System;
using System.Collections.Generic;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle patient age math
    /// </summary>
    public static class AgeUtil
    {
        /// <summary>
        /// Dictionary defining how to multiply a provided age value to turn it into a number of days
        /// </summary>
        private static Dictionary<string, float> UnitMultipliers = new Dictionary<string, float>
        {
            { Constants.AGEUNIT_DAYS, 1 },
            { Constants.AGEUNIT_WEEKS, Constants.DAYS_IN_WEEK },
            { Constants.AGEUNIT_MONTHS, Constants.DAYS_PER_MONTH },
            { Constants.AGEUNIT_YEARS, Constants.DAYS_IN_YEAR }
        };

        /// <summary>
        ///  This routine will return a patient's age in days.  Certain assumptions are made, which
        ///  make these calculations slightly inaccurate. First, there are ~30.416 days in a
        ///  month (365/12). Second, there are no leap years. Third, fractions of an ageunit are ignored.
        /// 
        /// If ageunits is not provided, it is assumed to be 'Y' (Years)
        /// The calculated number of days is rounded down (only a non-integer when ageunits are 'M'.
        /// </summary>
        /// <param name="age">Age value</param>
        /// <param name="ageUnits">Age units identifier</param>
        /// <returns>Patient's calculated age in days</returns>
        public static int DaysOld(int age, string ageUnits)
        {
            if (age < 0)
            {
                age = 0;
            }
            if (String.IsNullOrEmpty(ageUnits))
            {
                ageUnits = Constants.AGEUNIT_YEARS;
            } else if (ageUnits.Length > 1)
            {
                ageUnits = ageUnits.Substring(0, 1);
            }

            return (int)(age * UnitMultipliers[ageUnits]);
        }

        /// <summary>
        /// Age-related constants
        /// </summary>
        public static class Constants
        {
            #region number constants
            // --- Number constants --- //
            /// <summary>
            /// Months in a year
            /// </summary>
            public const int MONTHS_IN_YEAR = 12;

            /// <summary>
            /// Weeks in a year
            /// </summary>
            public const int WEEKS_IN_YEAR = 52;

            /// <summary>
            /// Days in a year
            /// </summary>
            public const int DAYS_IN_YEAR = 365;

            /// <summary>
            /// Approximation of number of weeks in a month
            /// </summary>
            public const float WEEKS_PER_MONTH = (float)WEEKS_IN_YEAR / (float)MONTHS_IN_YEAR;

            /// <summary>
            /// Approximation of number of days in a month
            /// </summary>
            public const float DAYS_PER_MONTH = (float)DAYS_IN_YEAR / (float)MONTHS_IN_YEAR;

            /// <summary>
            /// Days in a week
            /// </summary>
            public const int DAYS_IN_WEEK = 7;

            /// <summary>
            /// Maximum tinyint value
            /// </summary>
            public const int MAX_INT = 255;

            /// <summary>
            /// Maximum number of days in a days ageunit
            /// </summary>
            public const int MAX_DAYS = 30;

            /// <summary>
            /// Maximum number of weeks in a weeks ageunit
            /// </summary>
            public const int MAX_WEEKS = 15;

            /// <summary>
            /// Maximum number of months in a months ageunit
            /// </summary>
            public const int MAX_MONTHS = 36;
            #endregion

            #region ageunit constants
            // --- ageunit constants --- //

            /// <summary>
            /// Identifier for days ageunit
            /// </summary>
            public const string AGEUNIT_DAYS = "D";

            /// <summary>
            /// Identifier for weeks ageunit
            /// </summary>
            public const string AGEUNIT_WEEKS = "W";

            /// <summary>
            /// Identifier for months ageunit
            /// </summary>
            public const string AGEUNIT_MONTHS = "M";

            /// <summary>
            /// Identifier for years ageunit
            /// </summary>
            public const string AGEUNIT_YEARS = "Y";
            #endregion
        }
    }
}