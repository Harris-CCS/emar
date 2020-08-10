using Emar.Core.Sites.Model;

namespace Emar.Core.Sites.Service
{
    public interface ISiteService
    {
        SiteDto GetSite(in int siteId);
    }
}