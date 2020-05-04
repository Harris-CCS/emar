using System;

namespace DomainModel
{
    /// <summary>
    /// Object that represents an event in the system
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Event description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Event type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// User associated with the event
        /// </summary>
        public MinimalUser User { get; set; } = new MinimalUser();

        /// <summary>
        /// Date/time that the event occurred
        /// </summary>
        public DateTime DateTime { get; set; }
    }
}