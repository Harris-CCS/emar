using System.Collections.Generic;

namespace Emar.Core.Orders.Model
{
    public class DepartmentPreferredFrameworkDto
    {
        public DepartmentPreferredTabDto CurrentTab { get; set; }
        public IEnumerable<DepartmentPreferredItemDto> CurrentTabContents;
        public List<DepartmentPreferredTabDto> TabListing;

        public DepartmentPreferredFrameworkDto(IEnumerable<DepartmentPreferredItemDto> firstTabOrders, List<KeyValuePair<string, int>> tabListing,
            string linkBase)
        {
            CurrentTabContents = firstTabOrders;
            CurrentTab = new DepartmentPreferredTabDto(tabListing[0], linkBase);

            TabListing = new List<DepartmentPreferredTabDto>();

            foreach (var tab in tabListing)
            {
                TabListing.Add(new DepartmentPreferredTabDto(tab, linkBase));
            }
        }
    }
}
