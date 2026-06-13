using System;
using Emar.Data;

namespace Emar.Core.FdbObjects.Repository
{
    public  class FdbAllergyNameRepository: IFdbAllergyNameRepository
    {
        private readonly EmarContext _context;

        public FdbAllergyNameRepository()
        {

        }

        public FdbAllergyNameRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }

    }
}
