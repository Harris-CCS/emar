using System;
using Emar.Data;

namespace Emar.Core.FdbObjects.Repository
{
    public   class FdbBrandNameRepository: IFdbBrandNameRepository
    {
        private readonly EmarContext _context;

        public FdbBrandNameRepository()
        {

        }

        public FdbBrandNameRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }

    }
}
