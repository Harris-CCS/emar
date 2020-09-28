using Emar.Core.Medications.Model;
using System.Collections.Generic;

namespace Emar.Core.Medications.Service
{
    public interface IDoseRangeCheckingInfoService
    {
        IEnumerable<DoseRangeCheckingInfoDto> DoseRangeCheckInfos(string medid);
    }
}