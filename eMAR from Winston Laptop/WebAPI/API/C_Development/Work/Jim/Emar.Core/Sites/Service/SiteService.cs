using Emar.Core.Sites.Model;
using Emar.Core.Sites.Model.Mappings;
using Emar.Core.Sites.Repository;
using Emar.Data.Entities;

namespace Emar.Core.Sites.Service
{
    public class SiteService : ISiteService
    {
        private readonly ISiteRepository _siteRepository;

        public SiteService(ISiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }
        
        public SiteDto GetSite(in int siteId)
        {
            Site site = _siteRepository.GetSite(siteId);
            if (site == null) return null;
            return SiteMapper.MapSite(site);
        }

        //public int GetSiteIdByName(string siteName)
        //{
        //    return _siteRepository.GetSiteIdByName(siteName);
        //}
    }
}
