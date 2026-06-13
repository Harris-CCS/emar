using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.OutboundChart.Model
{
    public class OcsVitals
    {
        public string enteredDatetime { get; set; }
        public string BPSystolic { get; set; }
        public string BPDiastolic { get; set; }
        public string PULSE { get; set; }
        public string TEMPERATURE { get; set; }
        public string O2SAT { get; set; }
        public string MAP { get; set; }
        public string RESPIRATORY { get; set; }
        public string PAIN { get; set; }
        public string ENDTIDALCO2 { get; set; }
        public string bpCondition { get; set; }
        public string bpSite { get; set; }
        public string pulseSelect1 { get; set; }
        public string pulseSelect2 { get; set; }
        public string temperatureSelect1 { get; set; }
        public string temperatureSelect2 { get; set; }
        public string on { get; set; }
        public string mapSelect1 { get; set; }
        public string mapSelect2 { get; set; }
        public string respiratorySelect1 { get; set; }
        public string respiratorySelect2 { get; set; }
        public string painSelect1 { get; set; }
        public string painSelect2 { get; set; }
        public string endtidalCo2Select1 { get; set; }
        public string endtidalCo2Select2 { get; set; }
    }
}
