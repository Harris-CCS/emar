using System.Linq;
using Emar.Core.Sites.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Options.Model.Mappings
{
    public static class OptionMapper
    {
        public static OptionDto MapOption(Option option)
        {
            if (option == null)
            {
                return null;
            }

            var ret = new OptionDto
            {
                Id = option.Id,
                Name = option.Name,
                Description = option.Description,
                SiteOptions = option.SiteOptions?.Select(MapSiteOption).Where(o => o.OptionId == option.Id).ToList()
            };

            return ret;
        }

        public static SiteOptionDto MapSiteOption(SiteOption siteOption)
        {
            if (siteOption == null)
            {
                return null;
            }

            var ret = new SiteOptionDto
            {
                Id = siteOption.Id,
                SiteId = siteOption.SiteId,
                OptionId = siteOption.OptionId,
                OptionValue = siteOption.OptionValue,
                Site = SiteMapper.MapSite(siteOption.Site)
            };

            return ret;
        }
    }
}
