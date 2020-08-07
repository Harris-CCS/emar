using System.Collections.Generic;
using Emar.Core.Helpers;

namespace Emar.Core.Orders.Model
{
    public class UserQuickListItemDto : OrderBase
    {
        /// <summary>
        /// Unique User identifier
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// User Quick Lists are site-specific
        /// </summary>
        public long SiteId { get; set; }

        public IEnumerable<HateOasLinkDto> Links;
    }
}
