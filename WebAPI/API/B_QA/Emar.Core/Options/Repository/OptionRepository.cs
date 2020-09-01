using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Data;

namespace Emar.Core.Options.Repository
{
    public class OptionRepository : IOptionRepository
    {
        private readonly EmarContext _context;
        private readonly Dictionary<string, string> _siteOptionCache = new Dictionary<string, string>();


        public OptionRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }

        public string GetOption(int siteId, string optionName)
        {
            if (!_siteOptionCache.TryGetValue($"{siteId}|{optionName}", out var optionValue))
            {
                optionValue = _context.SiteOptions
                .FirstOrDefault(so => so.SiteId == siteId &&
                                      so.OptionId == _context.Options
                                          .FirstOrDefault(o => o.Name == optionName)
                                          .Id)
                .OptionValue;

                _siteOptionCache.Add($"{siteId}|{optionName}", optionValue);
            }

            return optionValue;
        }
    }
}
