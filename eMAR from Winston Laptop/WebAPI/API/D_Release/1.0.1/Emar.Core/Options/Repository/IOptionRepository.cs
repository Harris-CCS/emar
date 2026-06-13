using Emar.Core.Options.Model;
using System.Collections.Generic;

namespace Emar.Core.Options.Repository
{
    public interface IOptionRepository
    {
        public string GetOption(int siteId, OptionNames optionName, string defaultValue = null);
        bool GetOptionBool(int siteId, OptionNames optionName, bool? defaultValue = null);
        Dictionary<string, string> GetSiteOptions(int siteId, string optionsList);
    }
}
