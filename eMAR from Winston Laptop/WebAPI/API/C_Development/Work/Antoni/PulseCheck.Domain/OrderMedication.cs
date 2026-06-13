using System.Collections.Generic;

namespace PulseCheck.Domain
{
    /// <summary>
    /// Class to represent a medication being ordered
    /// </summary>
    public class OrderMedication
    {
        /// <summary>
        /// Identifier for the source of this order (group ID, NDC, etc.)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Medication name (used when ordering freetext meds)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ordered dose
        /// </summary>
        public string Dose { get; set; }

        /// <summary>
        /// Ordered unit
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Ordered route
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Ordered time (schedule?)
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// Ordered notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Ordered repeat instructions
        /// </summary>
        public string Repeat { get; set; }

        /// <summary>
        /// Ordered rate
        /// </summary>
        public string Rate { get; set; }

        /// <summary>
        /// Ordered rate unit
        /// </summary>
        public string RateUnit { get; set; }

        /// <summary>
        /// List of reaction/interaction overrides rationales for this medication
        /// </summary>
        public List<OverrideRationale> Overrides { get; set; }

        /// <summary>
        /// Antimicrobial stewardship indication
        /// </summary>
        public string Indication { get; set; }
    }
}