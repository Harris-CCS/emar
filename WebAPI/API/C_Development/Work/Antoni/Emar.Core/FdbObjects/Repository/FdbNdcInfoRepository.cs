using System;
using Emar.Data;

namespace Emar.Core.FdbObjects.Repository
{
    public class FdbNdcInfoRepository: IFdbNdcInfoRepository
    {
        private readonly EmarContext _context;

        public FdbNdcInfoRepository()
        {

        }

        public FdbNdcInfoRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }

    }
}
