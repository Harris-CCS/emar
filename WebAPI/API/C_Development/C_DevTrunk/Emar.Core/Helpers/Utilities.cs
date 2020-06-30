using System;
using System.Globalization;

namespace Emar.Core
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the specified string to title case.
        /// </summary>
        /// <param name="value">The string to convert to title case.</param>
        /// <returns>The specified string converted to title case.</returns>
        public static string ToTitleCase(this string value)
        {
            return (!String.IsNullOrEmpty(value) ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLowerInvariant()) : String.Empty);
        }
    }

    public static class DateTimeOffsetExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateTimeOffset)
        {
            var CurrentDate = DateTime.UtcNow;
            int Age = CurrentDate.Year - dateTimeOffset.Year;

            if (CurrentDate < dateTimeOffset.AddYears(Age))
            {
                Age--;
            }

            return Age;
        }
    }
}
