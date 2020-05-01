using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCheck.Archive.Domain
{
    public interface IArchiveManager
    {
        void ArchiveOrdResults(int saveDays, int batchCnt = 1000);
    }
}
