using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Repository
{
    public interface IDoseRangeCheckingInfoRepository
    {
        IEnumerable<DoseRangeCheckingInfo> RetrieveDoseRangeCheckingInfo(int medid);
    }
}