using System;
using System.Collections.Generic;

namespace Interfaces.DomainModel
{
    public interface IPatient
    {
        string Ibex { get; set; }
        string Department { get; set; }
        string Ward { get; set; }
        string Ward2 { get; set; }
        string Bed { get; set; }
        string AcctNum { get; set; }
        List<Object> Providers { get; set; }
    }
}
