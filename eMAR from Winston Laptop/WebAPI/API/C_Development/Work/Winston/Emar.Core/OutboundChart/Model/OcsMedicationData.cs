using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.OutboundChart.Model
{
    public class OcsMedicationData
    {
        public int losecs { get; set; }
        public string orderDate { get; set; }
        public int orderUser { get; set; }
        public string giveDate { get; set; }
        public int? giveUser { get; set; }
        public string giveSysDate { get; set; }
        public long emarPatientOrderId { get; set; }
        public string status { get; set; }
    }
}
