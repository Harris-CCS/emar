using System;
using System.Collections.Generic;

namespace PulseCheck.Domain
{
    public class Allergy
    {
        public string Name { get; set; }
        public Identifier Reaction { get; set; }
        public Identifier Severity { get; set; }
        public Identifier Source { get; set; }
        public string Comment { get; set; }
        public Status Status { get; set; }
        public Status ActionStatus { get; set; }
        public MinimalUser User { get; set; }
        public MinimalUser UserChg { get; set; }
        public DateTime? DateAdd { get; set; }
        public DateTime? DateChg { get; set; }
        public List<Identifier> Codes { get; set; }
    }
}