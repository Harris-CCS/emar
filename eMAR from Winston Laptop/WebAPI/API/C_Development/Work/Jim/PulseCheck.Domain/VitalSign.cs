using System;

namespace PulseCheck.Domain
{
    public class VitalSign
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string Attribute { get; set; }
        //public List<Code> Codes { get; set; }
        public Status Status { get; set; }
        public DateTime? DateTime { get; set; }
        public Object User { get; set; }
        public Style Style { get; set; }
    }
}