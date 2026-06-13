using System.Collections.Generic;
using Emar.Core.Medications.Model;

namespace Emar.Core.Medications.Service
{
    public interface IDoseRangeCheckingInfoService
    {
        IEnumerable<DoseRangeCheckingInfoDto> DoseRangeCheckInfos(int medid);
    }
}