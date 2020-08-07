using System.Collections.Generic;

namespace Emar.Core.Orders.Model
{
    public class UserQuickListFrameworkDto
    {
        public UserQuickListTabDto CurrentTab { get; set; }
        public IEnumerable<UserQuickListItemDto> CurrentTabContents;
        public List<UserQuickListTabDto> TabListing;

        public UserQuickListFrameworkDto(IEnumerable<UserQuickListItemDto> firstTabOrders, List<string> tabListing,
            string linkBase)
        {
            CurrentTabContents = firstTabOrders;
            CurrentTab = new UserQuickListTabDto(tabListing[0], linkBase);

            TabListing = new List<UserQuickListTabDto>();

            foreach (var tab in tabListing)
            {
                TabListing.Add(new UserQuickListTabDto(tab, linkBase));
            }
        }
    }
}