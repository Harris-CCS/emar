using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PulseCheck.Archive.Data;
using PulseCheck.Archive.Domain;

namespace PulseCheck.Archive.Logic
{
    public class IbexArchiveManager : Manager, IArchiveManager
    {
        private IIbexRepository _ibexRepository;
        private IIbexArchiveRepository _ibexArchiveRepository;

        public IbexArchiveManager(IIbexRepository ibexRepository, IIbexArchiveRepository ibexArchiveRepository)
        {
            _ibexRepository = ibexRepository;
            _ibexArchiveRepository = ibexArchiveRepository;
        }


        public void ArchiveOrdResults(int saveDays, int batchCnt = 1000)
        {
            _ibexArchiveRepository.ArchiveOrdResults(saveDays, batchCnt);
        }
    }
}
