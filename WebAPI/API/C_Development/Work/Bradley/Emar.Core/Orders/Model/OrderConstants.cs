namespace Emar.Core.Orders.Model
{
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
        Completed = 7,
        Deleted = 8
    }

    /// <summary>
    /// Order administration statuses
    /// </summary>
    public enum OrderAdministrationStatus
    {
        Pending,
        Acknowledged,
        OnGoing,
        OnHold,
        Given,
        Late
    }

    public static class Constants
    {
        #region UserQuickList Constants
        public const string MostUsedTabTitle = "Most Used";
        #endregion
    }
}