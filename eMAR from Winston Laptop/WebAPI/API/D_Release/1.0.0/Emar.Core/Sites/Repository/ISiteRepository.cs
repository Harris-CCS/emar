using System;
using Emar.Data.Entities;

namespace Emar.Core.Sites.Repository
{
    public interface ISiteRepository  
    {
        Site GetSite(int siteId);
        string GetSiteTimeZone(int siteId);
        DateTimeOffset DateTimeOffsetNow(int siteId);

        int GetInternalSiteId(int externalId);

    }
}