using Emar.Core.FdbObjects.Repository;

namespace Emar.Core.FdbObjects.Service
{
    public class FdbBrandNameService : IFdbBrandNameService
    {
        private readonly IFdbBrandNameRepository _fdbBrandNameRepository;

        public FdbBrandNameService(IFdbBrandNameRepository fdbBrandNameRepository)
        {
            _fdbBrandNameRepository = fdbBrandNameRepository;
        }

    }
}
