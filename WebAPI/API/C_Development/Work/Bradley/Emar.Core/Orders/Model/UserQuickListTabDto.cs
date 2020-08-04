using Emar.Core.Helpers;

namespace Emar.Core.Orders.Model
{
    public class UserQuickListTabDto
    {
        public string TabName;

        public HateOasLinkDto Link;
        public UserQuickListTabDto(string tabName, string linkBase)
        {
            TabName = tabName;

            switch (tabName)
            {
                case "#":
                    linkBase += "%23";
                    break;
                default:
                    linkBase += tabName;
                    break;
            }

            Link = new HateOasLinkDto(linkBase, "retrieve_tab_content", "GET");
        }
    }
}