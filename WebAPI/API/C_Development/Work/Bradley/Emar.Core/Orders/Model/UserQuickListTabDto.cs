using System.Collections.Generic;
using Emar.Core.Helpers;

namespace Emar.Core.Orders.Model
{
    public class UserQuickListTabDto
    {
        public string TabName;
        public int NumberItems { get; set; }

        public HateOasLinkDto Link;

        public UserQuickListTabDto(KeyValuePair<string, int> tab, string linkBase)
        {
            TabName = tab.Key;
            NumberItems = tab.Value;

            switch (TabName)
            {
                case "#":
                    linkBase += "%23";
                    break;
                default:
                    linkBase += TabName;
                    break;
            }

            Link = new HateOasLinkDto(linkBase, "retrieve_tab_content", "GET");
        }
    }
}