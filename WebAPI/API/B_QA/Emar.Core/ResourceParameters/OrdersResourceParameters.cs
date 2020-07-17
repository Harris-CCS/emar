namespace Emar.Core
{
    public class OrdersResourceParameters : BaseResourceParameters
    {
        /// <summary>
        /// eMAR unique patient identifier.
        /// </summary>
        public long? PatientId { get; set; }
    }
}
