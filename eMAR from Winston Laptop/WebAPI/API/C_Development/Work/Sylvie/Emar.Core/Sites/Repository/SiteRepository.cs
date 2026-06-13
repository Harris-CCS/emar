using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Emar.Core.Sites.Repository
{
    public class SiteRepository : ISiteRepository
    {
        private readonly EmarContext _context;
        private readonly MemoryCache _cache;

        public SiteRepository(EmarContext context, EmarMemoryCache cache)
        {
            _context = context;
            _cache = cache.Cache;
        }

        public Site GetSite(int siteId)
        {
            var site = _context.Sites.FirstOrDefault(s => s.Id == siteId);

            return site?.Name == null
                ? null
                : site;
        }

        public string GetSiteTimeZone(int siteId)
        {
            return _cache.GetOrCreate(siteId + CacheKeys.SiteTimeZone, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                entry.Size = 1;
                var site = GetSite(siteId);
                return site.TimeZoneName;
            });
        }

        public int GetInternalSiteId(int externalId)
        {
            var siteDict = _cache.GetOrCreate("All" + CacheKeys.SiteExternalToInternal, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                var dict =
                    _context.ExternalIds.Where(i => i.Vendor == "pulsecheck" && i.Entity == "sites")
                        .Select(i => new KeyValuePair<int, int>(int.Parse(i.ExternalId), (int)i.InternalId))
                        .ToDictionary(record => record.Key, record => record.Value);
                entry.Size = dict.Count;
                return dict;
            });
            return siteDict[externalId];
        }

        public DateTimeOffset DateTimeOffsetNow(int siteId)
        {
            return GetSiteTimeZone(siteId).NowWithTimeZoneOffset();
        }
    }
}