using System.Collections.Generic;

namespace Emar.Core.Orders.Model
{
    public class UserQuickListFrameworkDto
    {
        public string CurrentTabName { get; set; }
        public List<UserQuickListItemDto> CurrentTabContents = new List<UserQuickListItemDto>();
        public List<string> TabListing = new List<string>();
    }
}