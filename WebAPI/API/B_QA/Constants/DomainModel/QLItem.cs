using System.Collections.Generic;

namespace DomainModel
{
    public class QLItem
    {
        /// <summary>
        /// Quicklist Item identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// String used for grouping QuickList Items together
        /// </summary>
        public string GroupKey { get; set; }

        /// <summary>
        /// Quicklist Item name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Quicklist Item type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Quicklist Item Dose ID
        /// </summary>
        public string Dose { get; set; }

        /// <summary>
        /// Quicklist Item Unit ID
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Quicklist Item Route ID
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Quicklist Item Schedule string
        /// </summary>
        public string Schedule { get; set; }

        /// <summary>
        /// Quicklist Item Repeat string
        /// </summary>
        public string Repeat { get; set; }

        /// <summary>
        /// Quicklist Item notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Interactions associated with this Quicklist Item
        /// </summary>
        public List<Dictionary<string, string>> Interactions { get; set; }

        /// <summary>
        /// Reactions associated with this Quicklist Item
        /// </summary>
        public List<Dictionary<string, string>> Reactions { get; set; }

        /// <summary>
        /// Flag indicating the Quicklist Item needs a medication indication
        /// </summary>
        public bool HasIndication { get; set; }

        /// <summary>
        /// Default empty object
        /// </summary>
        public QLItem()
        {
            Interactions = new List<Dictionary<string, string>>();
            Reactions = new List<Dictionary<string, string>>();
        }
    }
}