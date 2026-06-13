using System;
using System.Collections.Generic;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;

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
        public int SiteId { get; set; }

        public int? DurationInMinutes { get; set; }

        public byte? Priority { get; set; }

        public IEnumerable<HateOasLinkDto> Links;

        public string? Ndc { get; set; }
    }
}
