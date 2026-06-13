using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.OutboundChart.Model
{
    public class OcsVitalsRangeData
    {
        public decimal rangeValue { get; set; }
        public string typeName { get; set; }
        public int rangeTypeId { get; set; }
    }
}
