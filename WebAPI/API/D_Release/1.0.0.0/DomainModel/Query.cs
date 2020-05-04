using System.Collections.Generic;

namespace DomainModel
{
    /// <summary>
    /// Order entry query object
    /// </summary>
    public class Query
    {
        public Query() { }

        protected Query(Query copy)
        {
            foreach (System.Reflection.PropertyInfo propertyInfo in copy.GetType().GetProperties())
            {
                if (propertyInfo.Name == "Error")
                    continue;

                if (propertyInfo.GetSetMethod(true) != null)
                    propertyInfo.SetValue(this, propertyInfo.GetValue(copy));
            }
        }

        /// <summary>
        /// Boolean indicating the query should just be displayed once when doing orders
        /// </summary>
        public bool DisplayOnce { get; set; }

        /// <summary>
        /// Query mnemonic/identifier
        /// </summary>
        private string _mnemonic;
        public string Mnemonic
        {
            get { return this._mnemonic.Trim(); }
            set { this._mnemonic = value.Trim(); }
        }

        private string _error;
        /// <summary>
        /// The error that happened with the query
        /// </summary>
        public string Error {
            get { return _error; }
            protected set
            {
                _error = string.Format(value, Value, Description);
            }
        }

        /// <summary>
        /// Query type (see Query.Constants)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Query description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Query default value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Sequence the query should appear in
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Representation of how the value should be displayed
        /// </summary>
        public virtual string DisplayValue { get { return Value; } }

        /// <summary>
        /// Flag for whether query is required
        /// </summary>
        public bool Required { get; set; }

        public Order Order { get; set; }

        /// <summary>
        /// Action for the query to take when it's entered
        /// </summary>
        public virtual void Action() { }

        /// <summary>
        /// Validate that an query has everything filled out correctly
        /// </summary>
        /// <returns>Flag indicating query is good to save</returns>
        public virtual bool Validate() {
            return true;
        }

        public static Query ConvertToProperType(Query query)
        {
            Query convertedQuery;
            switch(query.Type) {
                case Constants.TYPE_ALPHA:
                    convertedQuery = new AlphaQuery(query);
                    break;
                case Constants.TYPE_DATE:
                    convertedQuery = new DateQuery(query);
                    break;
                case Constants.TYPE_DATETIME:
                    convertedQuery = new DateTimeQuery(query);
                    break;
                case Constants.TYPE_DROPDOWN:
                    convertedQuery = new DropdownQuery(query);
                    break;
                case Constants.TYPE_DROPDOWNOO:
                    convertedQuery = new DropdownOrOtherQuery(query);
                    break;
                case Constants.TYPE_HIDDEN:
                    convertedQuery = new HiddenQuery(query);
                    break;
                case Constants.TYPE_INSTRUCTION:
                    convertedQuery = new InstructionQuery(query);
                    break;
                case Constants.TYPE_NUMERIC:
                    convertedQuery = new NumericQuery(query);
                    break;
                case Constants.TYPE_TEXT:
                    convertedQuery = new TextQuery(query);
                    break;
                case Constants.TYPE_TIME:
                    convertedQuery = new TimeQuery(query);
                    break;
                default:
                    convertedQuery = query;
                    break;
            }
            
            return convertedQuery;
        }
        /// <summary>
        /// Constants used in Query objects
        /// </summary>
        public static class Constants
        {
            #region query type identifiers
            /// <summary>
            /// Alpha pager query type
            /// </summary>
            public const string TYPE_ALPHA = "email";

            /// <summary>
            /// Date-only query type
            /// </summary>
            public const string TYPE_DATE = "dateonly";

            /// <summary>
            /// Date and time query type
            /// </summary>
            public const string TYPE_DATETIME = "time";

            /// <summary>
            /// Dropdown query type
            /// </summary>
            public const string TYPE_DROPDOWN = "code";

            /// <summary>
            /// "Dropdown or other" query type
            /// </summary>
            public const string TYPE_DROPDOWNOO = "other";

            /// <summary>
            /// Hidden query type
            /// </summary>
            public const string TYPE_HIDDEN = "hidden";

            /// <summary>
            /// Instruction query type
            /// </summary>
            public const string TYPE_INSTRUCTION = "instruction";

            /// <summary>
            /// Numeric query type
            /// </summary>
            public const string TYPE_NUMERIC = "number";

            /// <summary>
            /// Text query type
            /// </summary>
            public const string TYPE_TEXT = "text";

            /// <summary>
            /// Time-only query type
            /// </summary>
            public const string TYPE_TIME = "timeonly";
            #endregion

            #region error strings
            /// <summary>
            /// Text value is longer than allowable length
            /// </summary>
            public const string ERROR_MAX_LENGTH = "Maximum length of {0} exceeded for {1}";

            /// <summary>
            /// Numeric value is greater than allowable value
            /// </summary>
            public const string ERROR_MAX_VALUE = "Maximum value of {0} exceeded for {1}";

            /// <summary>
            /// Code not found in the code set
            /// </summary>
            public const string ERROR_BAD_CODE = "Invalid code of {0} for {1}";
            #endregion

            internal static readonly Dictionary<string, System.Type> CONVERSION_MAPPING = new Dictionary<string, System.Type>
            {
                { TYPE_ALPHA, typeof(AlphaQuery) },
                { TYPE_DATE, typeof(DateQuery) },
                { TYPE_DATETIME, typeof(DateTimeQuery) },
                { TYPE_DROPDOWN, typeof(DropdownQuery) },
                { TYPE_DROPDOWNOO, typeof(DropdownOrOtherQuery) },
                { TYPE_HIDDEN, typeof(HiddenQuery) },
                { TYPE_INSTRUCTION, typeof(InstructionQuery) },
                { TYPE_NUMERIC, typeof(NumericQuery) },
                { TYPE_TEXT, typeof(TextQuery) },
                { TYPE_TIME, typeof(TimeQuery) },
            };
        }
    }
}