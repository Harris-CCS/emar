using System.Collections.Generic;
using System.Linq;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Core.Medications.Repository
{
    public class DoseRangeCheckingInfoRepository : IDoseRangeCheckingInfoRepository
    {
        EmarContext _context;
        public DoseRangeCheckingInfoRepository(EmarContext context)
        {
            _context = context;
        } 
        public IEnumerable<DoseRangeCheckingInfo> RetrieveDoseRangeCheckingInfo(int medid)
        {
            return _context.DoseRangeCheckingInfos.FromSqlInterpolated($"exec pc_fdb_get_drc_info {medid}").ToList();
        }
    }
}