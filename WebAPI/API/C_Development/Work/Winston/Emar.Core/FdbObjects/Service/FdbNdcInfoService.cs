using Emar.Core.FdbObjects.Repository;

namespace Emar.Core.FdbObjects.Service
{
    public  class FdbNdcInfoService: IFdbNdcInfoService
    {
        private readonly IFdbNdcInfoRepository _fdbNdcInfoRepository;

        public FdbNdcInfoService(IFdbNdcInfoRepository fdbNdcInfoRepository)
        {
            _fdbNdcInfoRepository = fdbNdcInfoRepository;
        }

    }
}
