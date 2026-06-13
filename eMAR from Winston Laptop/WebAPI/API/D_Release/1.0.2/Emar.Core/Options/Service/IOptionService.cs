using Emar.Core.Options.Model;
using System.Collections.Generic;

namespace Emar.Core.Options.Service
{
    public interface IOptionService
    {
        public string GetOption(int siteId, OptionNames optionName);

        public Dictionary<string, string> GetSiteOptions(int siteId, string optionsList);
    }
}
