using Emar.Core.Options.Repository;

namespace Emar.Core.Options.Service
{
    public partial class OptionService : IOptionService
    {
        private readonly IOptionRepository _optionRepository;

        public OptionService(IOptionRepository optionRepository)
        {
            _optionRepository = optionRepository;
        }

        public string GetOption(int siteId, string optionName)
        {
            return _optionRepository.GetOption(siteId, optionName);
        }
    }
}
