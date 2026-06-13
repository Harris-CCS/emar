using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using System;
using System.Collections.Generic;

namespace Emar.Core.Options.Service
{
    public partial class OptionService : IOptionService
    {
        private readonly IOptionRepository _optionRepository;

        public OptionService(IOptionRepository optionRepository)
        {
            _optionRepository = optionRepository;
        }

        public string GetOption(int siteId, OptionNames optionName)
        {
            return _optionRepository.GetOption(siteId, optionName);
        }

        public Dictionary<string, string> GetSiteOptions(int siteId, string optionsList)
        {
            return _optionRepository.GetSiteOptions(siteId, optionsList);
        } //end function GetSiteOptions
    }
}
