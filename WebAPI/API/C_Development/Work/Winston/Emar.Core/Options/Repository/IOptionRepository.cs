using Emar.Core.Options.Model;

namespace Emar.Core.Options.Repository
{
    public interface IOptionRepository
    {
        public string GetOption(int siteId, OptionNames optionName);
    }
}
