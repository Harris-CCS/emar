using Emar.Data.Entities;
using System.Collections.Generic;

namespace Emar.Core.Medications.Repository
{
    public interface IDoseRangeCheckingInfoRepository
    {
        IEnumerable<DoseRangeCheckingInfo> RetrieveDoseRangeCheckingInfo(string ndc);
    }
}