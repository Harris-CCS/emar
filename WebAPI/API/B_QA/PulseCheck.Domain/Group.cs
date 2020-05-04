using System.Collections.Generic;

namespace PulseCheck.Domain
{
    /// <summary>
    /// Medication group/pathway
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Group type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Group name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Unique identifier for group
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// Group style
        /// </summary>
        public Style Style { get; set; }

        /// <summary>
        /// Group Altcode
        /// </summary>
        public string AltCode { get; set; }

        /// <summary>
        /// Medications in a group
        /// </summary>
        public List<Medication> Medications { get; set; }

        /// <summary>
        /// Queries in a group
        /// </summary>
        public List<Service> Services { get; set; } = new List<Service>();
    }
}