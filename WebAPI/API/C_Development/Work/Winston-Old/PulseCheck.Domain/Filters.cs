using System.Collections.Generic;

namespace PulseCheck.Domain
{
    public class Filters
    {
        public string Type { get; set; }
        private string _name { get; set; }
        public string Name
        {
            get { return this._name != null ? this._name.Trim() : ""; }
            set { this._name = value?.Trim() ?? ""; }
        }

        public List<Filter> Options { get; set; }

        public class Constants
        {
            /// <summary>
            /// Filter type identifier for patient filters
            /// </summary>
            public const string FILTER_TYPE_PATIENT = "patient";

            /// <summary>
            /// Filter type identifier for disposition filters
            /// </summary>
            public const string FILTER_TYPE_DISPO = "dispo";

            /// <summary>
            /// Identifier for "All Patients" filter
            /// </summary>
            public const string FILTER_PATIENT_ALL = "pA";

            /// <summary>
            /// Identifier for "My Patients" filter
            /// </summary>
            public const string FILTER_PATIENT_MY_PATIENTS = "pM";

            /// <summary>
            /// Identifier for "Mine and None" filter
            /// </summary>
            public const string FILTER_PATIENT_MINE_AND_NONE = "pN";

            /// <summary>
            /// Identifier for "All" dispo filter
            /// </summary>
            public const string FILTER_DISPO_ALL = "dA";

            /// <summary>
            /// Identifier for "Has Dispo" dispo filter
            /// </summary>
            public const string FILTER_DISPO_HAS_DISPO = "dH";

            /// <summary>
            /// Identifier for "No Dispo" dispo filter
            /// </summary>
            public const string FILTER_DISPO_NONE = "dN";

            /// <summary>
            /// Identifier for "Admission" dispo filter
            /// </summary>
            public const string FILTER_DISPO_ADM = "dZA";

            /// <summary>
            /// Identifier for "Inpatient" dispo filter
            /// </summary>
            public const string FILTER_DISPO_INP = "dZI";

            /// <summary>
            /// Identifier for "Obs" dispo filter
            /// </summary>
            public const string FILTER_DISPO_OBS = "dZO";
        }
    }
}