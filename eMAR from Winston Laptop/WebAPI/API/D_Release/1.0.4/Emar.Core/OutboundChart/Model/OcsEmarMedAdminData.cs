using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.OutboundChart.Model
{
    public class OcsEmarMedAdminData
    {
        public int losecs { get; set; }
        public string medAdminType { get; set; }
        public int medAdminUser { get; set; }
        public string medAdminDate { get; set; }
        public string medAdminSysDate { get; set; }
        public long patientOrderId { get; set; }
    }
}