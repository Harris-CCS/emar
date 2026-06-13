using Microsoft.Extensions.Caching.Memory;

namespace Emar.Core.Helpers
{
    public class EmarMemoryCache
    {
        public MemoryCache Cache { get; set; }

        public EmarMemoryCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 4096
            });
        }
    }

    public static class CacheKeys
    {
        public static string OrderActions => "_OrderActions";
        public static string OrderAdministrationActions => "_OrderAdministrationActions";
        public static string OrderInstructions => "_OrderInstructions";
        public static string FrequencySchedules => "_FrequencySchedules";
        public static string NotificationCategories => "_NotificationCategories";
        public static string Routes => "_Routes";
        public static string RouteSites => "_RouteSites";
        public static string Units => "_Units";
        public static string UnitSites => "_UnitSites";
        public static string DurationUnits => "_DurationUnits";
        public static string SiteOptions => "_SiteOptions";
        public static string ActionRouteTemplates => "_ActionRouteTemplates";
        public static string SiteTimeZone => "_SiteTimeZone";
        public static string Prompts => "_Prompts";
        public static string GlobalOptions => "_GlobalOptions";
        public static string SiteExternalToInternal => "_SiteExternalIds";
        public static string UserSettingKeys => "_UserSettingKeys";
        public static string UserExternalIdKeys => "_UserExternalIdKeys";
        public static string TemplateDatetimePromptIds => "_TemplateDatetimePromptIds";
    }
}