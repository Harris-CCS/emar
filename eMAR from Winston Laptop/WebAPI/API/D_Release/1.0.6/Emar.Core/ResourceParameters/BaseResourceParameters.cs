namespace Emar.Core.ResourceParameters
{
    public class BaseResourceParameters : PageResource
    {
        /// <summary>
        /// eMAR unique site identifier.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// eMAR unique user identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// eMAR unique patient identifier.
        /// </summary>
        public long PatientId { get; set; }
    }
}
