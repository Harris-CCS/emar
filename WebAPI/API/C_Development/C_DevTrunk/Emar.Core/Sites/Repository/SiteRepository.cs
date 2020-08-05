using System.Linq;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Sites.Repository
{
    public class SiteRepository : ISiteRepository
    {
        private EmarContext _context;

        public SiteRepository(EmarContext context)
        {
            _context = context;
        }

        public Site GetSite(int siteId)
        {
            var site = _context.Sites.FirstOrDefault(s => s.Id == siteId);
            if (site?.Name == null) return null;

            return site;
        }
    }
}
