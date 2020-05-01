using Interfaces.DomainModel;
using PulseCheck.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace DomainModel
{
    public class Patient : Person, ICloneable, IPatient
    {
        public Patient()
        {
            Demographics = new Demographics();
            Encounters = new List<Encounter>();
            Chart = new Chart();
        }

        public string EnterpriseId { get; set; }
        public string Ibex { get; set; }

        private string _acctnum;
        public string AcctNum
        {
            get { return this._acctnum != null ? this._acctnum.Trim() : ""; }
            set { this._acctnum = value?.Trim() ?? ""; }
        }

        private string _mrn;
        public string MedicalRecordNumber
        {
            get { return this._mrn != null ? this._mrn.Trim() : ""; }
            set { this._mrn = value?.Trim() ?? ""; }
        }

        private string _ssn { get; set; }
        public string Ssn
        {
            get { return this._ssn != null ? this._ssn.Trim() : ""; }
            set { this._ssn = value?.Trim() ?? ""; }
        }

        //[Key, Column(Order = 1), MaxLength(20)]
        //public string PersonId { get; set; }

        [NotMapped]
        public Chart Chart { get; set; }

        [NotMapped]
        public List<Encounter> Encounters { get; set; } = new List<Encounter>();

        [NotMapped]
        public Demographics Demographics { get; set; } = new Demographics();

        [NotMapped]
        public List<Object> Providers { get; set; }

        [NotMapped]
        public Urgency Urgency { get; set; }

        [NotMapped]
        public VitalIndicator VitalIndicator { get; set; }

        [NotMapped]
        public List<VitalSign> VitalSigns { get; set; }

        [NotMapped]
        public List<Comment> Comments { get; set; }

        //public List<Identifier> PatientIdentifiers { get; set; }
        //public Pharmacy Pharmacy { get; set; }

        public bool Readmit { get; set; }
        public int LOSMins { get; set; }
        public int FirstDoctor { get; set; }

        [NotMapped]
        public string DisplayName { get; set; }

        [NotMapped]
        public Indicator Registration { get; set; }

        [NotMapped]
        public List<Indicator> OrderIndicators { get; set; }

        [NotMapped]
        public Complaint Complaint { get; set; }

        [NotMapped]
        public Disposition DispoCode { get; set; }

        [NotMapped]
        public Disposition DispoLocation { get; set; }

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

        public Decimal? Height { get; set; }
        public Decimal? Weight { get; set; }


        /// <summary>
        /// Height value formatted for display
        /// </summary>
        [NotMapped]
        public string DisplayHeight
        {
            get
            {
                return this.Height != null && this.Height > 0 ? string.Format("{0:0.##}", this.Height.ToString()) + " cm" : "";
            }
        }

        /// <summary>
        /// Weight value formatted for display
        /// </summary>
        [NotMapped]
        public string DisplayWeight
        {
            get { return this.Weight != null && this.Weight != 0 ? (
                        this.Weight < 0 ? string.Format("{0:0.##}", Math.Abs((sbyte)this.Weight)) +  " kg (est.)" :
                        string.Format("{0:0.##}", this.Weight) + " kg"
                   ) : "";
            }
        }

        private static Dictionary<string, string> OrgOptions = new Dictionary<string, string>();

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

            if (fields["MiddleName"].Trim().Length > 0 && !GetOrgOption("FULL_MIDDLE_NAMES").Equals("Y"))
            {
                format["MiddleName"]["Length"] = "1";
                fields["MiddleName"] = fields["MiddleName"].Substring(0, 1);
            }

            var formatPos = 0;
            var nameFormat = "";
            var nameFields = new string[fieldOrder.Count];
            foreach(var field in fieldOrder)
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

        public Patient Clone() {
            return (Patient)this.MemberwiseClone();
        }

        object ICloneable.Clone() {
            return Clone();
        }

        private string GetOrgOption(string optName)
        {
            if (!OrgOptions.ContainsKey(optName))
            {
                OrgOptions[optName] = new DB.Select
                {
                    Sql = "SELECT [dbo].[fnGetOrgOption](@siteId, @optName)",
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@siteId", SqlDbType.TinyInt) { Value = SiteId },
                        new SqlParameter("@optName", SqlDbType.VarChar) { Value = optName }
                    }
                }.RunForScalar().ToString();
            }

            return OrgOptions[optName];
        }
    }
}