using Emar.Data.Entities;

namespace Emar.Core.Sites.Model.Mappings
{
    public static class SiteMapper
    {
        public static SiteDto MapSite(Site site)
        {
            if (site == null)
            {
                return null;
            }

            SiteDto siteDto = new SiteDto
            {
                Id = site.Id,
                Name = site.Name,
                Active = site.Active
            };

            return siteDto;
        }
    }
}
