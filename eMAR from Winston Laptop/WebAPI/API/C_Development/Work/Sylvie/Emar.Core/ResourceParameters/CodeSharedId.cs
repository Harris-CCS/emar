using Emar.Core.Orders.Repository;

namespace Emar.Core.ResourceParameters
{
    public class CodeSharedId
    {
        public int SiteId { get; set; }
        public OrderRepository.CodeShareEntity Entity { get; set; }
        public int SharedSiteId { get; set; }
    }
}