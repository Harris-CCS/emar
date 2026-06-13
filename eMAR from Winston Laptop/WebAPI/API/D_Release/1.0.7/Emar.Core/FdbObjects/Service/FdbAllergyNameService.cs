using Emar.Core.FdbObjects.Repository;

namespace Emar.Core.FdbObjects.Service
{
    public class FdbAllergyNameService: IFdbAllergyNameService
    {
        private readonly IFdbAllergyNameRepository _fdbAllergyNameRepository;

        public FdbAllergyNameService(IFdbAllergyNameRepository fdbAllergyNameRepository)
        {
            _fdbAllergyNameRepository = fdbAllergyNameRepository;
        }

    }
}
