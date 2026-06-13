namespace Emar.Core.Orders.Model
{
    public class OrderEventDetailDto
    {
        public long EventDetailId { get; set; }
        public int PromptId { get; set; }
        public string PromptText { get; set; }
        public string UserResponse { get; set; }
    }
}
