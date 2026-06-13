using System.Collections.Generic;

namespace Emar.Core.OutboundChart.Model
{
    /// <summary>
    /// Interaction/reaction override rationale for medications
    /// </summary>
    public class OverrideRationale
    {
        /// <summary>
        /// The dnum for the drug that these overrides apply to
        /// </summary>
        public string Dnum { get; set; }

        /// <summary>
        /// List of overrides for this rationale set
        /// </summary>
        public List<string> Overrides { get; set; }

        /// <summary>
        /// Boolean flag for whether this override rationale should be applied to all medications that follow
        /// </summary>
        public bool ApplyToAll { get; set; }
    }
}
