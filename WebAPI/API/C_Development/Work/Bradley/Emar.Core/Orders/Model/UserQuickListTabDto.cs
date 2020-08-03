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

            string linkHref = linkBase + "/tabs/";
            switch (tabName)
            {
                case "#":
                    linkHref += "%23";
                    break;
                default:
                    linkHref += tabName;
                    break;
            }

            Link = new HateOasLinkDto(linkHref, "TabContentRetrieve", "GET");
        }
    }
}