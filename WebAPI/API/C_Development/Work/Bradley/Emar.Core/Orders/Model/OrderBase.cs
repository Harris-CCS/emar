namespace Emar.Core.Orders.Model
{
    public class OrderBase
    {
        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identifier for the source of this order (group ID, NDC, etc.)
        /// </summary>
        public string MedicationId { get; set; }

        /// <summary>
        /// Indicates whether the order is PRN.
        /// </summary>
        public bool Prn { get; set; }

        /// <summary>
        /// Indicates whether the order is Point-In-Time.
        /// </summary>
        public bool PointInTime { get; set; }

        /// <summary>
        /// Unique order frequency identifier.
        /// </summary>
        public int FrequencyId { get; set; }

        /// <summary>
        /// Unique medication route identifier.
        /// </summary>
        public int MedicationRouteId { get; set; }

        /// <summary>
        /// Name of the Medication Route
        /// </summary>
        public string MedicationRoute { get; set; }

        /// <summary>
        /// Order notes.
        /// </summary>
        public string OrderNotes { get; set; }

        #region Constants

        /// <summary>
        /// Order types
        /// </summary>
        public enum OrderTypes
        {
            Stat = 1,
            Prn = 2,
            Continuous = 3,
            Scheduled = 4
        }

        /// <summary>
        /// Order priorities
        /// </summary>
        public enum OrderPriorities
        {
            Stat = 2,
            Routine = 4
        }

        /// <summary>
        /// Order statuses
        /// </summary>
        public enum OrderStatuses
        {
            Pending = 1,
            Cancelled = 2,
            OnGoing = 3,
            OnHold = 4,
            PendingDiscontinue = 5,
            Discontinued = 6,
            Completed = 7
        }

        #endregion
    }
}