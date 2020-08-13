namespace Emar.Core.Orders.Model
{
    /// <summary>
    /// Constants used in the Orders domain model
    /// </summary>
    public static class Constants
    {
        #region Order Type constants
        ///////// <summary>
        ///////// "Continuous" order type code
        ///////// </summary>
        //////public const string ContinuousCode = "CONTINUOUS";

        ///////// <summary>
        ///////// "STAT" order type code
        ///////// </summary>
        //////public const string StatCode = "STAT";

        ///////// <summary>
        ///////// "PRN" order type code
        ///////// </summary>
        //////public const string PrnCode = "PRN";

        ///////// <summary>
        ///////// <summary>
        ///////// "Scheduled" order type code
        ///////// </summary>
        //////public const string ScheduledCode = "SCHEDULED";

        ///////// CONTINUOUS: Administration of a drug, usually in the form of intravenous (IV) infusion, over several hours to days.
        ///////// </summary>
        //////public static OrderType CONTINUOUS_ORDERTYPE = new OrderType()
        //////{
        //////    Code = CONTINUOUS_CODE,
        //////    Description = "Continuous"
        //////};

        ///////// <summary>
        ///////// STAT: A common medical abbreviation for urgent or rush. From the Latin word 'statum', meaning 'immediately'.
        ///////// </summary>
        //////public static OrderType STAT_ORDERTYPE = new OrderType()
        //////{
        //////    Code = STAT_CODE,
        //////    Description = "STAT"
        //////};

        ///////// <summary>
        ///////// PRN: A common medical abbreviation for as needed. From the Latin term 'pro re nata', meaning 'as the thing is needed'.
        ///////// </summary>
        //////public static OrderType PRN_ORDERTYPE = new OrderType()
        //////{
        //////    Code = PRN_CODE,
        //////    Description = "PRN"
        //////};

        ///////// <summary>
        ///////// SCHEDULED
        ///////// </summary>
        //////public static OrderType SCHEDULED_ORDERTYPE = new OrderType()
        //////{
        //////    Code = SCHEDULED_CODE,
        //////    Description = "Scheduled"
        //////};

        ///////// <summary>
        ///////// List of the Order Types
        ///////// </summary>
        //////public static List<OrderType> OrderTypes => new List<OrderType>()
        //////{
        //////    CONTINUOUS_ORDERTYPE,
        //////    STAT_ORDERTYPE,
        //////    PRN_ORDERTYPE,
        //////    SCHEDULED_ORDERTYPE
        //////};

        ///////// <summary>
        ///////// Order types
        ///////// </summary>
        //////public static readonly Dictionary<string, string> ORDER_TYPE_CODES = new Dictionary<string, string> {
        //////    { CONTINUOUS_CODE, "Continuous" },
        //////    { STAT_CODE, "STAT" },
        //////    { PRN_CODE, "PRN" },
        //////    { SCHEDULED_CODE, "Scheduled" }
        //////};
        #endregion

        #region Order & Administration Status constants
        ///////// <summary>
        ///////// "Pending" administration status code
        ///////// </summary>
        //////public const string PENDING_CODE = "PENDING";

        ///////// <summary>
        ///////// "Ordered" order status code
        ///////// </summary>
        //////public const string ORDERED_CODE = "ORDERED";

        ///////// <summary>
        ///////// "Acknowledged" administration status code
        ///////// </summary>
        //////public const string ACKNOWLEDGED_CODE = "ACKNOWLEDGED";

        ///////// <summary>
        ///////// "Given" administation status code
        ///////// </summary>
        //////public const string GIVEN_CODE = "GIVEN";

        ///////// <summary>
        ///////// "Ongoing" administation status code
        ///////// </summary>
        //////public const string ONGOING_CODE = "ONGOING";

        ///////// <summary>
        ///////// "Late" administation status code
        ///////// </summary>
        //////public const string LATE_CODE = "LATE";

        ///////// <summary>
        ///////// "Cancelled" order status code
        ///////// </summary>
        //////public const string CANCELLED_CODE = "CANCELLED";

        ///////// <summary>
        ///////// "Deleted" order status code
        ///////// </summary>
        //////public const string DELETED_CODE = "DELETED";

        ///////// <summary>
        ///////// "On Hold" order status code
        ///////// </summary>
        //////public const string ON_HOLD_CODE = "ON_HOLD";

        ///////// <summary>
        ///////// "Pending Discontinue" order status code
        ///////// </summary>
        //////public const string PENDING_DISCONTINUE_CODE = "PENDING_DISCONTINUE";

        ///////// <summary>
        ///////// "Discontinued" order status code
        ///////// </summary>
        //////public const string DISCONTINUED_CODE = "DISCONTINUED";

        ///////// <summary>
        ///////// "Completed" order status code
        ///////// </summary>
        //////public const string COMPLETED_CODE = "COMPLETED";

        ///////// <summary>
        ///////// "Pending" Administration Status
        ///////// </summary>
        //////public static OrderStatus PENDING_ORDERSTATUS = new OrderStatus()
        //////{
        //////    Code = PENDING_CODE,
        //////    Description = PENDING_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// "Ordered" Order Status
        ///////// </summary>
        //////public static OrderStatus ORDERED_ORDERSTATUS = new OrderStatus()
        //////{
        //////    Code = ORDERED_CODE,
        //////    Description = ORDERED_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// "Cancelled" Order Status
        ///////// </summary>
        //////public static OrderStatus CANCELLED_ORDERSTATUS = new OrderStatus()
        //////{
        //////    Code = CANCELLED_CODE,
        //////    Description = CANCELLED_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// "Deleted" Order Status
        ///////// </summary>
        //////public static OrderStatus DELETED_ORDERSTATUS = new OrderStatus()
        //////{
        //////    Code = DELETED_CODE,
        //////    Description = DELETED_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// "On Hold" Order and Administration Status
        ///////// </summary>
        //////public static OrderStatus ON_HOLD_ORDERSTATUS = new OrderStatus()
        //////{
        //////    Code = ON_HOLD_CODE,
        //////    Description = ON_HOLD_CODE.ToTitleCase().Replace("_", " ")
        //////};

        ///////// <summary>
        ///////// "Pending Discontinue" Order Status
        ///////// </summary>
        //////public static OrderStatus PENDING_DISCONTINUE_ORDERSTATUS = new OrderStatus()
        //////{
        //////    Code = PENDING_DISCONTINUE_CODE,
        //////    Description = PENDING_DISCONTINUE_CODE.ToTitleCase().Replace("_", " ")
        //////};

        ///////// <summary>
        ///////// "Discontinued" Order Status
        ///////// </summary>
        //////public static OrderStatus DISCONTINUED_ORDERSTATUS = new OrderStatus()
        //////{
        //////    Code = DISCONTINUED_CODE,
        //////    Description = DISCONTINUED_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// "Completed" Order Status
        ///////// </summary>
        //////public static OrderStatus COMPLETED_ORDERSTATUS = new OrderStatus()
        //////{
        //////    Code = COMPLETED_CODE,
        //////    Description = COMPLETED_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// List of the Order Statuses
        ///////// </summary>
        //////public static List<OrderStatus> OrderStatuses => new List<OrderStatus>()
        //////{
        //////    ORDERED_ORDERSTATUS,
        //////    CANCELLED_ORDERSTATUS,
        //////    DELETED_ORDERSTATUS,
        //////    ON_HOLD_ORDERSTATUS,
        //////    PENDING_DISCONTINUE_ORDERSTATUS,
        //////    DISCONTINUED_ORDERSTATUS,
        //////    COMPLETED_ORDERSTATUS
        //////};

        ///////// <summary>
        ///////// Order statuses
        ///////// </summary>
        //////public static readonly Dictionary<string, string> ORDER_STATUS_CODES = new Dictionary<string, string> {
        //////    { ORDERED_CODE, ORDERED_CODE.ToTitleCase() },
        //////    { CANCELLED_CODE, CANCELLED_CODE.ToTitleCase() },
        //////    { DELETED_CODE, DELETED_CODE.ToTitleCase() },
        //////    { ON_HOLD_CODE, ON_HOLD_CODE.ToTitleCase().Replace("_", " ") },
        //////    { PENDING_DISCONTINUE_CODE, PENDING_DISCONTINUE_CODE.ToTitleCase().Replace("_", " ") },
        //////    { DISCONTINUED_CODE, DISCONTINUED_CODE.ToTitleCase() },
        //////    { COMPLETED_CODE, COMPLETED_CODE.ToTitleCase() }
        //////};


        ///////// <summary>
        ///////// "Acknowledged" administration status
        ///////// </summary>
        //////public static OrderStatus ACKNOWLEDGED_ADMINSTATUS = new OrderStatus()
        //////{
        //////    Code = ACKNOWLEDGED_CODE,
        //////    Description = ACKNOWLEDGED_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// "Pending" administration status
        ///////// </summary>
        //////public static OrderStatus PENDING_ADMINSTATUS = new OrderStatus()
        //////{
        //////    Code = PENDING_CODE,
        //////    Description = PENDING_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// "Given" administration status
        ///////// </summary>
        //////public static OrderStatus GIVEN_ADMINSTATUS = new OrderStatus()
        //////{
        //////    Code = GIVEN_CODE,
        //////    Description = GIVEN_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// "Ongoing" administration status
        ///////// </summary>
        //////public static OrderStatus ONGOING_ADMINSTATUS = new OrderStatus()
        //////{
        //////    Code = ONGOING_CODE,
        //////    Description = ONGOING_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// "On Hold" administration status
        ///////// </summary>
        //////public static OrderStatus ON_HOLD_ADMINSTATUS = new OrderStatus()
        //////{
        //////    Code = ON_HOLD_CODE,
        //////    Description = ON_HOLD_CODE.ToTitleCase().Replace("_", " ")
        //////};

        ///////// <summary>
        ///////// "Late" administration status
        ///////// </summary>
        //////public static OrderStatus LATE_ADMINSTATUS = new OrderStatus()
        //////{
        //////    Code = LATE_CODE,
        //////    Description = LATE_CODE.ToTitleCase()
        //////};

        ///////// <summary>
        ///////// List of the Order Administration Statuses
        ///////// </summary>
        //////public static List<OrderStatus> AdminStatuses => new List<OrderStatus>()
        //////{
        //////    ACKNOWLEDGED_ADMINSTATUS,
        //////    PENDING_ADMINSTATUS,
        //////    GIVEN_ADMINSTATUS,
        //////    ONGOING_ADMINSTATUS,
        //////    ON_HOLD_ADMINSTATUS,
        //////    LATE_ADMINSTATUS
        //////};

        ///////// <summary>
        ///////// Order administration statuses
        ///////// </summary>
        //////public static readonly Dictionary<string, string> ORDER_ADMIN_CODES = new Dictionary<string, string> {
        //////    { ACKNOWLEDGED_CODE, ACKNOWLEDGED_CODE.ToTitleCase() },
        //////    { PENDING_CODE, PENDING_CODE.ToTitleCase() },
        //////    { GIVEN_CODE, GIVEN_CODE.ToTitleCase() },
        //////    { ONGOING_CODE, ONGOING_CODE.ToTitleCase() },
        //////    { ON_HOLD_CODE, ON_HOLD_CODE.ToTitleCase().Replace("_", " ") },
        //////    { LATE_CODE, LATE_CODE.ToTitleCase() }
        //////};

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
        #endregion

        #region Order Action constants
        /// <summary>
        /// "Acknowledge" action code
        /// </summary>
        public const string ACKNOWLEDGE_CODE = "ACKNOWLEDGE";

        /// <summary>
        /// "Cancel" action code
        /// </summary>
        public const string CANCEL_CODE = "CANCEL";

        /// <summary>
        /// "Delete" action code
        /// </summary>
        public const string DELETE_CODE = "DELETE";

        /// <summary>
        /// "Hold" action code
        /// </summary>
        public const string HOLD_CODE = "HOLD";

        /// <summary>
        /// "UnHold" action code
        /// </summary>
        public const string UNHOLD_CODE = "UNHOLD";

        /// <summary>
        /// "Given" action code
        /// </summary>
        public const string GIVE_CODE = "GIVE";

        /// <summary>
        /// "Discontinue" action code
        /// </summary>
        public const string DISCONTINUE_CODE = "DISCONTINUE";

        /// <summary>
        /// "Discontinued" action code
        /// </summary>
        //public const string DISCONTINUED_CODE = "DISCONTINUED";

        /// <summary>
        /// "Cosign" action code
        /// </summary>
        public const string COSIGN_CODE = "COSIGN";

        /// <summary>
        /// "Repeat" action code
        /// </summary>
        public const string REPEAT_CODE = "REPEAT";

        ///// <summary>
        ///// "Acknowledge" action
        ///// </summary>
        //public static OrderActionDto ACKNOWLEDGE_ACTION = new OrderActionDto()
        //{
        //    Code = ACKNOWLEDGE_CODE,
        //    Description = ACKNOWLEDGE_CODE.ToTitleCase()
        //};

        ///// <summary>
        ///// "Cancel" action
        ///// </summary>
        //public static OrderActionDto CANCEL_ACTION = new OrderActionDto()
        //{
        //    Code = CANCEL_CODE,
        //    Description = CANCEL_CODE.ToTitleCase()
        //};

        ///// <summary>
        ///// "Delete" action
        ///// </summary>
        //public static OrderActionDto DELETE_ACTION = new OrderActionDto()
        //{
        //    Code = DELETE_CODE,
        //    Description = DELETE_CODE.ToTitleCase()
        //};

        ///// <summary>
        ///// "Hold" action
        ///// </summary>
        //public static OrderActionDto HOLD_ACTION = new OrderActionDto()
        //{
        //    Code = HOLD_CODE,
        //    Description = HOLD_CODE.ToTitleCase()
        //};

        ///// <summary>
        ///// "Unhold" action
        ///// </summary>
        //public static OrderActionDto UNHOLD_ACTION = new OrderActionDto()
        //{
        //    Code = UNHOLD_CODE,
        //    Description = UNHOLD_CODE.ToTitleCase()
        //};

        ///// <summary>
        ///// "Give" action
        ///// </summary>
        //public static OrderActionDto GIVE_ACTION = new OrderActionDto()
        //{
        //    Code = GIVE_CODE,
        //    Description = GIVE_CODE.ToTitleCase()
        //};

        ///// <summary>
        ///// "Discontinue" action
        ///// </summary>
        //public static OrderActionDto DISCONTINUE_ACTION = new OrderActionDto()
        //{
        //    Code = DISCONTINUE_CODE,
        //    Description = DISCONTINUE_CODE.ToTitleCase()
        //};

        /// <summary>
        /// "Discontinued" action
        /// </summary>
        //public static OrderActionDto DISCONTINUED_ACTION = new OrderActionDto()
        //{
        //    Code = DISCONTINUED_CODE,
        //    Description = DISCONTINUED_CODE.ToTitleCase()
        //};

        ///// <summary>
        ///// "Cosign" action
        ///// </summary>
        //public static OrderActionDto COSIGN_ACTION = new OrderActionDto()
        //{
        //    Code = COSIGN_CODE,
        //    Description = COSIGN_CODE.ToTitleCase()
        //};

        ///// <summary>
        ///// "Repeat" action
        ///// </summary>
        //public static OrderActionDto REPEAT_ACTION = new OrderActionDto()
        //{
        //    Code = REPEAT_CODE,
        //    Description = REPEAT_CODE.ToTitleCase()
        //};

        /// <summary>
        /// List of the Order Actions
        /// </summary>
        //    public static List<OrderActionDto> OrderActions => new List<OrderActionDto>()
        //    {
        //             ACKNOWLEDGE_ACTION,
        //             CANCEL_ACTION,
        //             DELETE_ACTION,
        //             HOLD_ACTION,
        //             UNHOLD_ACTION,
        //             GIVE_ACTION,
        //             DISCONTINUE_ACTION,
        //             DISCONTINUED_ACTION,
        //             COSIGN_ACTION,
        //             COMPLETE_ACTION
        //    };

        //    /// <summary>
        //    /// Order action codes
        //    /// </summary>
        //    public static readonly Dictionary<string, string> ORDER_ACTION_CODES = new Dictionary<string, string> {
        //            { ACKNOWLEDGE_CODE, ACKNOWLEDGE_CODE.ToTitleCase() },
        //            { CANCEL_CODE, CANCEL_CODE.ToTitleCase() },
        //            { DELETE_CODE, DELETE_CODE.ToTitleCase() },
        //            { HOLD_CODE, HOLD_CODE.ToTitleCase() },
        //            { UNHOLD_CODE, UNHOLD_CODE.ToTitleCase() },
        //            { GIVE_CODE, GIVE_CODE.ToTitleCase() },
        //            { DISCONTINUE_CODE, DISCONTINUE_CODE.ToTitleCase() },
        //            { DISCONTINUED_CODE, DISCONTINUED_CODE.ToTitleCase() },
        //            { COSIGN_CODE, COSIGN_CODE.ToTitleCase() },
        //            { REPEAT_CODE, REPEAT_CODE.ToTitleCase() },
        //            { COMPLETE_CODE, COMPLETE_CODE.ToTitleCase() }
        //        };

        #endregion


        #region UserQuickList Constants

        public const string MostUsedTabTitle = "Most Used";

        #endregion
    }
}