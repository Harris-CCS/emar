using System.Collections.Generic;
using Emar.Core.Helpers;

namespace Emar.Core.Orders.Model
{
    public class GroupsRememberedOrdersDto
    {
        public List<RememberedGroupDto> Groups { get; set; }

        public GroupsRememberedOrdersDto()
        {
            Groups = new List<RememberedGroupDto>();
        }
    }

    public class RememberedGroupDto
    {
        public string GroupName { get; set; }
        public IEnumerable<GroupListItemDto> Orders { get; set; } 

        //public RememberedGroupDto()
        //{
        //    Orders = new List<GroupListItemDto>();
        //}
    }

    public class GroupListItemDto : OrderBase
    {
        public int SiteId { get; set; }

        public string DepartmentCode { get; set; }

        public string GroupName { get; set; }

        public int? DurationInMinutes { get; set; }

        public byte? Priority { get; set; }

        public IEnumerable<HateOasLinkDto> Links;

        public string? Ndc { get; set; }
    }
}