using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("recipient_user_id", TypeName = "int"), Required]
        public int RecipientUserId { get; set; }

        [Column("patient_order_id", TypeName = "bigint")]
        public long? PatientOrderId { get; set; }

        [Column("order_administration_id", TypeName = "bigint")]
        public long? OrderAdministrationId { get; set; }

        [Column("title", TypeName = "nvarchar(255)"), Required]
        public string Title { get; set; }

        [Column("body", TypeName = "nvarchar(1000)")]
        public string Body { get; set; }

        [Column("category_code", TypeName = "varchar(20)")]
        public string CategoryCode { get; set; }

        [Column("event_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? EventDateTime { get; set; }

        [Column("generated_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset GeneratedDateTime { get; set; }

        [Column("sent_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? SentDateTime { get; set; }

        [Column("acknowledged_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? AcknowledgedDateTime { get; set; }

        [ForeignKey(nameof(RecipientUserId))]
        [InverseProperty(nameof(Entities.User.UserNotifications))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(CategoryCode))]
        public virtual NotificationCategory Category { get; set; }

        [ForeignKey(nameof(PatientOrderId))]
        public virtual PatientOrder PatientOrder { get; set; }
    }
}