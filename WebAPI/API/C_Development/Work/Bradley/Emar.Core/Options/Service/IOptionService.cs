using Emar.Core.Options.Model;

namespace Emar.Core.Options.Service
{
    public interface IOptionService
    {
        public string GetOption(int siteId, OptionNames optionName);
    }
}
