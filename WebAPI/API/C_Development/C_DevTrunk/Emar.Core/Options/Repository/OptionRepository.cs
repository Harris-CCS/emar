using System;
using System.Linq;
using Emar.Data;

namespace Emar.Core.Options.Repository
{
    public class OptionRepository : IOptionRepository
    {
        private readonly EmarContext _context;

        public OptionRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }

        public string GetOption(int siteId, string optionName)
        {
            return _context.SiteOptions
                    .FirstOrDefault(so => so.SiteId == siteId &&
                                    so.OptionId == _context.Options
                                                    .FirstOrDefault(o => o.Name == optionName)
                                                    .Id)
                    .OptionValue;
        }
    }
}