using System;
using System.Collections.Generic;

namespace DomainModel
{
    public class CurrentMedication
    {
        public string Name { get; set; }
        public string Dose { get; set; }
        public Identifier Unit { get; set; }
        public Identifier Route { get; set; }
        public Identifier Schedule { get; set; }
        public string LastTaken { get; set; }
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