using Emar.Data.Entities;

namespace Emar.Core.Sites.Repository
{
    public interface ISiteRepository  
    {
        Site GetSite(int siteId);
    }
}