using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Emar.Core.OutboundChart.Model
{
    public class Patient : Person, ICloneable, IPatient
    {
        public Patient()
        {
            Chart = new Chart();
        }

        public string Ibex { get; set; }
        public Chart Chart { get; set; }
        public byte Age { get; set; }
        public string AgeUnit { get; set; }
        public List<Object> Providers { get; set; }
        private string _department { get; set; }
        public string Department
        {
            get { return this._department != null ? this._department.Trim() : ""; }
            set { this._department = value?.Trim() ?? ""; }
        }

        private string _ward { get; set; }
        public string Ward
        {
            get { return this._ward != null ? this._ward.Trim() : ""; }
            set { this._ward = value?.Trim() ?? ""; }
        }

        private string _ward2 { get; set; }
        public string Ward2
        {
            get { return this._ward2 != null ? this._ward2.Trim() : ""; }
            set { this._ward2 = value?.Trim() ?? ""; }
        }

        private string _bed { get; set; }
        public string Bed
        {
            get { return this._bed != null ? this._bed.Trim() : ""; }
            set { this._bed = value?.Trim() ?? ""; }
        }
        public byte SiteId { get; set; }

        /// <summary>
        /// Get the formatted patient name
        /// </summary>
        /// <returns>Patient name string</returns>
        public string GetName()
        {
            var fieldOrder = new List<string> { "LastName", "FirstName", "MiddleName", "Suffix" };
            var fields = new Dictionary<string, string>
            {
                { "FirstName", FirstName },
                { "LastName", LastName },
                { "MiddleName", MiddleName ?? "" },
                { "Suffix", Suffix ?? "" }
            };

            var hasName = false;
            var format = new Dictionary<string, Dictionary<string, string>>();
            foreach (var field in fieldOrder)
            {
                format[field] = new Dictionary<string, string>
                {
                    { "Length", null },
                    { "Delimiter", " " }
                };

                if (!string.IsNullOrWhiteSpace(fields[field]))
                {
                    hasName = true;
                }
            }

            if (!hasName)
            {
                return "";
            }
            format["Suffix"]["Delimiter"] = null;
            format["LastName"]["Delimiter"] = ", ";

            // Comment since assuming it is full middle names
//            if (fields["MiddleName"].Trim().Length > 0 && !GetOrgOption("FULL_MIDDLE_NAMES").Equals("Y"))
//            {
//                format["MiddleName"]["Length"] = "1";
//                fields["MiddleName"] = fields["MiddleName"].Substring(0, 1);
//            }

            var formatPos = 0;
            var nameFormat = "";
            var nameFields = new string[fieldOrder.Count];
            foreach (var field in fieldOrder)
            {
                nameFormat += "{" + formatPos + ":S";
                if (format[field].ContainsKey("Length") && !string.IsNullOrWhiteSpace(format[field]["Length"]))
                {
                    nameFormat += format[field]["Length"];
                }
                nameFormat += "}" + format[field]["Delimiter"];
                nameFields[formatPos] = (fields[field] ?? "");
                formatPos++;
            }

            var patName = string.Format(nameFormat, nameFields);
            patName = Regex.Replace(patName, @"\s+", " ");
            patName = patName.Trim();

            return patName;
        }

        public Patient Clone()
        {
            return (Patient)this.MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Age-related constants
        /// </summary>
        public static class Constants
        {
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
        }
    }
}
