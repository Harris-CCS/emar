using System;
using System.Collections.Generic;

namespace Emar.Core.OutboundChart.Model
{
    public interface IPatient
    {
        string Ibex { get; set; }
        string Department { get; set; }
        string Ward { get; set; }
        string Ward2 { get; set; }
        string Bed { get; set; }
        List<Object> Providers { get; set; }
    }
}

