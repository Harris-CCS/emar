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

            var siteDto = new SiteDto
            {
                Id = site.Id,
                Name = site.Name,
                Active = site.IsActive,
                TimeZoneName = site.TimeZoneName,
                TimeZoneOffset = site.TimeZoneOffset
            };

            return siteDto;
        }
    }
}
