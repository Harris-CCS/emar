using System.Web;
using System.Web.Mvc;

namespace PulseCheck.API
{
    /// <summary>
    /// Filter configuration
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Register all filters
        /// </summary>
        /// <param name="filters"></param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
