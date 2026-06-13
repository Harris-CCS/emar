namespace Emar.Core.Notifications.Model
{
    public class NotificationCategoryEnum
    {
        private NotificationCategoryEnum(string value) { Value = value; }

        public string Value { get; set; }

        public static NotificationCategoryEnum FollowUp { get { return new NotificationCategoryEnum("FU"); } }
        public static NotificationCategoryEnum Pending { get { return new NotificationCategoryEnum("PENDING");  } }
        public static NotificationCategoryEnum PossibleOverdue { get { return new NotificationCategoryEnum("PO");  } }
    }
}