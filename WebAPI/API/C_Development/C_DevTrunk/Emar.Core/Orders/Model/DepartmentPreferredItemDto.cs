using System.Collections.Generic;
using Emar.Core.Helpers;

namespace Emar.Core.Orders.Model
{
    public class DepartmentPreferredItemDto : OrderBase
    {
        /// <summary>
        /// User Quick Lists are site-specific
        /// </summary>
        public int SiteId { get; set; }

        public string DepartmentCode { get; set; }

        public IEnumerable<HateOasLinkDto> Links;
    }
}