using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Emar.Data.Entities;

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
    }
}
