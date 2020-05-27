using System;
using System.Collections.Generic;
using System.Text;
using PulseCheck.IDomain;
using PulseCheck.Utilities;

namespace PulseCheck.Domain
{
    /// <summary>
    /// Class to represent order results
    /// </summary>
    public class OrderResult : ICloneable
    {
        public string Name { get; set; }
        public DateTime? ObservationDate { get; set; }
        public string OrderNumber { get; set; }
        public int FirstLineNum { get; set; }
        public int LineNum { get; set; }
        public int LastLineNum { get; set; }
        public List<Component> Components { get; set; } = new List<OrderResult.Component>();
        private StringBuilder _text;
        public string Text
        {
            get { return this._text != null ? this._text.ToString() : ""; }
        }
        public string AlternateText { get; set; }
        public string OrderingProviderFName { get; set; }
        public string OrderingProviderLName { get; set; }
        public DateTime? SpecimenDate { get; set; }
        public string Department { get; set; }
        public string StatusDescription { get; set; }
        public string Status { get; set; }

        public string Source { get; set; }
        public string LineFeed { get; set; } = "~";
        public string OrderNumber1 { get; set; }
        public string Code { get; set; }
        public string AlternateCode { get; set; }
        public string AlternateCodeSystemName { get; set; }

        public void set(string name, string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return;
            }
            switch (name)
            {
                case "name":
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        Name = val;
                    }
                    break;
                case "observation_date":
                    if (!ObservationDate.HasValue && !string.IsNullOrWhiteSpace(val))
                    {
                        if (val.Length > 14)
                            val = val.Substring(0, 14);
                        ObservationDate = Time.DateTimeFromString(val);
                    }
                    break;
                case "order_number":
                    if (string.IsNullOrWhiteSpace(OrderNumber))
                    {
                        OrderNumber = val;
                    }
                    break;
                case "order_number_1":
                    if (string.IsNullOrWhiteSpace(OrderNumber1))
                    {
                        OrderNumber1 = val;
                    }
                    break;
                case "ordering_provider_fname":
                    if (string.IsNullOrWhiteSpace(OrderingProviderFName))
                    {
                        OrderingProviderFName = val;
                    }
                    break;
                case "ordering_provider_lname":
                    if (string.IsNullOrWhiteSpace(OrderingProviderLName))
                    {
                        OrderingProviderLName = val;
                    }
                    break;
                case "specimen_date":
                    if (!SpecimenDate.HasValue && !string.IsNullOrWhiteSpace(val))
                    {
                        if (val.Length > 14)
                            val = val.Substring(0, 14);
                        SpecimenDate = Time.DateTimeFromString(val);
                    }
                    break;
                case "status_description":
                    if (string.IsNullOrWhiteSpace(StatusDescription))
                    {
                        StatusDescription = val;
                    }
                    break;
                case "status":
                    if (string.IsNullOrWhiteSpace(Status))
                    {
                        Status = val;
                    }
                    break;
                case "line_num":
                    if (LineNum == 0)
                    {
                        LineNum = Convert.ToInt32(val);
                    }
                    break;
                case "alternate_code":
                    if (string.IsNullOrWhiteSpace(AlternateCode))
                    {
                        AlternateCode = val;
                    }
                    break;
                case "code":
                    if (string.IsNullOrWhiteSpace(Code))
                    {
                        Code = val;
                    }
                    break;
                case "department":
                    if (string.IsNullOrWhiteSpace(Department))
                    {
                        Department = val;
                    }
                    break;
                case "alternate_text":
                    if (string.IsNullOrWhiteSpace(AlternateText))
                    {
                        AlternateText = val;
                    }
                    break;
                case "alternate_code_system_name":
                    if (string.IsNullOrWhiteSpace(AlternateCodeSystemName))
                    {
                        AlternateCodeSystemName = val;
                    }
                    break;
                default:
                    break;
            }
        }

        public OrderResult()
        {
            Components = new List<OrderResult.Component>();
        }

        public void AddText(string text)
        {
            if (_text == null)
                _text = new StringBuilder();

            _text.Append(text.Replace(LineFeed, "<br>"));
        }

        public OrderResult Clone()
        {
            return (OrderResult)this.MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public class Component : ICloneable
        {
            public string Source { get; set; }
            public string Status { get; set; }
            public string LineFeed { get; set; } = "~";
            public string LevelClass { get; set; }
            public bool CanPostToChart { get; set; }
            public bool IsAbnormal { get; set; }
            public string AbnormalType { get; set; }
            public bool IsCritical { get; set; }
            public string CriticalType { get; set; }
            public int LineNum { get; set; }
            private StringBuilder _text;
            public string Text
            {
                get { return this._text != null ? this._text.ToString() : ""; }
            }
            private StringBuilder _notes = new StringBuilder("\n");
            public string Notes
            {
                get { return _notes.ToString(); }
            }
            public int FirstLineNum { get; set; }
            public int LastLineNum { get; set; }
            public TestFields Fields { get; set; }

            public Component()
            {
                Fields = new TestFields();
            }

            public void AddText(string text)
            {
                if (_text == null)
                    _text = new StringBuilder();

                _text.Append(text.Replace(LineFeed, "<br>"));
            }

            public void AddNotes(string note)
            {
                _notes.Append(note.Replace(LineFeed, "\n"));
            }

            public void set(string name, string val)
            {
                if (string.IsNullOrWhiteSpace(val))
                {
                    return;
                }
                switch (name)
                {
                    case "status":
                        if (string.IsNullOrWhiteSpace(Status))
                        {
                            Status = val;
                        }
                        break;
                    case "line_num":
                        if (LineNum == 0)
                        {
                            LineNum = Convert.ToInt32(val);
                        }
                        break;
                    default:
                        break;
                }
            }

            public Component Clone()
            {
                return (Component)this.MemberwiseClone();
            }

            object ICloneable.Clone()
            {
                return Clone();
            }

            public class TestFields : ITestFields, ICloneable
            {
                public string Name { get; set; } = "";
                public DateTime? Date { get; set; } = new DateTime?();
                public string Flag { get; set; } = "";
                public string Range { get; set; } = "";
                public string Status { get; set; } = "";                                
                public string Comment { get; set; } = "";
                public string Units { get; set; } = "";
                public string LOINC { get; set; } = "";
                public string Link { get; set; } = "";
                public string AlternateDescription { get; set; } = "";
                public string Code { get; set; } = "";
                public string CodeSystemName { get; set; } = "";
                public string AlternateCode { get; set; } = "";
                public string AlternateCodeSystemName { get; set; } = "";
                public string OrganizationName { get; set; } = "";
                public string OrganizationStreet1 { get; set; } = "";
                public string OrganizationStreet2 { get; set; } = "";
                public string OrganizationCity { get; set; } = "";
                public string OrganizationState { get; set; } = "";
                public string OrganizationZip { get; set; } = "";
                public int LineNum { get; set; } = 0;
                public string LineFeed { get; set; } = "~";

                private string _result = "";
                public string Result { get { return _result; } set { _result = value.Replace(LineFeed, "<br>"); } }

                private string _text = "";
                public string Text { get { return _text; } set { _text = value.Replace(LineFeed, "<br>"); } }

                public TestFields()
                {

                }

                public void set(string name, string val)
                {
                    name = name.Replace("tests_", "");
                    switch (name)
                    {
                        case "line_num":
                            if (!string.IsNullOrWhiteSpace(val))
                            {
                                LineNum = Convert.ToInt32(val);
                            }
                            break;
                        case "comment":
                            Comment = val;
                            break;
                        case "date":
                            if (!string.IsNullOrWhiteSpace(val))
                            {
                                Date = Time.DateTimeFromString(val);
                            }
                            break;
                        case "flag":
                            Flag = val;
                            break;
                        case "name":
                            Name = val;
                            break;
                        case "range":
                            Range = val;
                            break;
                        case "result":
                            Result = val;
                            break;
                        case "status":
                            Status = val;
                            break;
                        case "text":
                            Text = val;
                            break;
                        case "units":
                            Units = val;
                            break;
                        case "link":
                            Link = val;
                            break;
                        case "alternate_description":
                            AlternateDescription = val;
                            break;
                        case "organization_city":
                            OrganizationCity = val;
                            break;
                        case "code":
                            Code = val;
                            break;
                        case "alternate_code_system_name":
                            AlternateCodeSystemName = val;
                            break;
                        case "organization_street1":
                            OrganizationStreet1 = val;
                            break;
                        case "organization_state":
                            OrganizationState = val;
                            break;
                        case "organization_street2":
                            OrganizationStreet2 = val;
                            break;
                        case "alternate_code":
                            AlternateCode = val;
                            break;
                        case "code_system_name":
                            CodeSystemName = val;
                            break;
                        case "organization_name":
                            OrganizationName = val;
                            break;
                        case "loinc":
                            LOINC = val;
                            break;
                        case "organization_zip":
                            OrganizationZip = val;
                            break;
                        default:
                            break;
                    }
                }

                public TestFields Clone()
                {
                    return (TestFields)this.MemberwiseClone();
                }

                object ICloneable.Clone()
                {
                    return Clone();
                }
            }
        }

        /// <summary>
        /// Simple class for assembling data for posting a result to the chart
        /// </summary>
        public class ResultForChart
        {
            public string ComponentName { get; set; }
            public string ResultText { get; set; }
            public string Units { get; set; }
            public string Range { get; set; }
            public string ParentName { get; set; }
            public int LineCT { get; set; }
            public bool IsAbnormal { get; set; }
            public string DateString { get; set; }
            public string Comment { get; set; }
            public string TestType { get; set; }
            public string DeptName { get; set; }
            public string OrderNumber { get; set; }
            public string Status { get; set; }

            public ResultForChart()
            {

            }
        }

        /// <summary>
        /// Constants for OrderResult
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// Fields that should be assigned, per segment name
            /// </summary>
            public static readonly Dictionary<string, List<string>> AssignmentFields = new Dictionary<string, List<string>>
            {
                { "OBR", new List<string>
                {
                  "name", "observation_date", "order_number", "order_number_1",
                  "ordering_provider_fname", "ordering_provider_lname", "specimen_date",
                  "status_description", "line_num", "alternate_code", "code", "department",
                  "alternate_text", "alternate_code_system_name"
                } },
                { "ORC", new List<string>
                {
                    "status", "line_num"
                } }
            };

            /// <summary>
            /// Message type for actual results.  Found in the MSH segment.
            /// </summary>
            public const string ORDER_RESULT_MESSAGE = "ORU";
        }
    }
}