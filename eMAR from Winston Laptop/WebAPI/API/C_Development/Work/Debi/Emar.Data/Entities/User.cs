using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("users")]
    public class User
    {
        public User()
        {
            MedicationInteractions = new HashSet<MedicationInteraction>();
            OrderAdministrationsAcknowledgeUser = new HashSet<OrderAdministration>();
            OrderAdministrationAdministeringUser = new HashSet<OrderAdministration>();
            OrderAdministrationStopUser = new HashSet<OrderAdministration>();
            // For Foreign Key: fk__order_events__users
            OrderEvents = new HashSet<OrderEvent>(); PatientAllergiesAddUser = new HashSet<PatientAllergy>();
            OrderReactions = new HashSet<OrderReaction>();
            PatientAllergiesAddUser = new HashSet<PatientAllergy>();
            PatientAllergiesChangeUser = new HashSet<PatientAllergy>();
            PatientCartOrders = new HashSet<PatientCartOrder>();
            PatientHomeMedicationsAddUser = new HashSet<PatientHomeMedication>();
            PatientHomeMedicationsChangeUser = new HashSet<PatientHomeMedication>();
            PatientOrdersAddUser = new HashSet<PatientOrder>();
            PatientOrdersOrderPhysicianUser = new HashSet<PatientOrder>();

            //For foreign key fk__print_history__users
            PrintHistorys = new HashSet<PrintHistory>();

            // For Foreign Key: fk__notifications__users__recipient_user_id
            UserNotifications = new HashSet<Notification>();
            // For Foreign Key: fk__user_patients__users
            UserPatients = new HashSet<UserPatient>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
            // For Foreign Key: fk__user_settings__users
            UserSettings = new HashSet<UserSetting>();
        }

        [Key]
        [Column("id", TypeName = "int")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public int SiteId { get; set; }

        [Required]
        [Column("type")]
        [StringLength(1)]
        public string Type { get; set; }

        [Column("is_active", TypeName = "bit"), Required]
        public bool IsActive { get; set; }

        [Required]
        [Column("initials_display", TypeName = "nvarchar(4)")]
        public string UserInitials { get; set; }

        [Required]
        [Column("first_name", TypeName = "nvarchar(35)")]
        public string FirstName { get; set; }

        [Required]
        [Column("last_name", TypeName = "nvarchar(35)")]
        public string LastName { get; set; }

        [Column("middle_name", TypeName = "nvarchar(35)")]
        public string MiddleName { get; set; }

        [Column("name_suffix", TypeName = "nvarchar(25)")]
        public string NameSuffix { get; set; }

        [Column("ordering_only_physician", TypeName = "bit")]
        public bool? OrderingOnlyPhysician { get; set; }

        [Column("name_display_initials", TypeName = "bit")]
        public bool DisplayInitialsIndicator { get; set; }

        [Required]
        [Column("login_name", TypeName = "varchar(255)")]
        public string LoginName { get; set; }

        [Column("login_password", TypeName = "varchar(255)"), Required]
        public string LoginPassword { get; set; }

        [Column("salt", TypeName = "binary(16)"), Required]
        public byte[] Salt { get; set; }

        [Column("last_login_time", TypeName = "datetimeoffset")]
        public DateTimeOffset? LastLoginTime { get; set; }

        [Column("failed_login_attempts", TypeName = "int"), Required]
        public int FailedLoginAttempts { get; set; }


        /// <summary>
        /// The following properties are provided so that we don't have to go through a generic object
        /// </summary>
        [NotMapped]
        public string ExternalId { get; set; }


        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.Users))]
        public virtual Site Site { get; set; }

        [InverseProperty(nameof(OrderAdministration.AcknowledgeUser))]
        public virtual ICollection<OrderAdministration> OrderAdministrationsAcknowledgeUser { get; set; }

        [InverseProperty(nameof(OrderAdministration.AdministeringUser))]
        public virtual ICollection<OrderAdministration> OrderAdministrationAdministeringUser { get; set; }

        [InverseProperty(nameof(OrderAdministration.StopUser))]
        public virtual ICollection<OrderAdministration> OrderAdministrationStopUser { get; set; }

        [InverseProperty("OverrideReasonUser")]
        public virtual ICollection<OrderReaction> OrderReactions { get; set; }

        [InverseProperty(nameof(PatientAllergy.AddUser))]
        public virtual ICollection<PatientAllergy> PatientAllergiesAddUser { get; set; }

        [InverseProperty(nameof(PatientAllergy.ChangeUser))]
        public virtual ICollection<PatientAllergy> PatientAllergiesChangeUser { get; set; }

        [InverseProperty(nameof(PatientOrder.AddUser))]
        public virtual ICollection<PatientOrder> PatientOrdersAddUser { get; set; }

        [InverseProperty("OverrideReasonUser")]
        public virtual ICollection<MedicationInteraction> MedicationInteractions { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<OrderEvent> OrderEvents { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        [InverseProperty(nameof(PatientOrder.OrderPhysicianUser))]
        public virtual ICollection<PatientOrder> PatientOrdersOrderPhysicianUser { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }

        [InverseProperty(nameof(PatientHomeMedication.AddUser))]
        public virtual ICollection<PatientHomeMedication> PatientHomeMedicationsAddUser { get; set; }

        [InverseProperty(nameof(PatientHomeMedication.ChangeUser))]
        public virtual ICollection<PatientHomeMedication> PatientHomeMedicationsChangeUser { get; set; }

        [InverseProperty("OverrideReasonUser")]
        public virtual ICollection<AllergyReactionView> AllergyReactionsView { get; set; }

        [InverseProperty("OverrideReasonUser")]
        public virtual ICollection<DrugInteractionView> DrugInteractionsView { get; set; }

        // For Foreign key: fk__notifications__users__recipient_user_id
        [InverseProperty("User")]
        public virtual ICollection<Notification> UserNotifications { get; set; }

        // For Foreign Key: fk__user_patients__users
        [InverseProperty("User")]
        public virtual ICollection<UserPatient> UserPatients { get; set; }

        // For Foreign Key: fk__user_settings__users
        [InverseProperty("User")]
        public virtual ICollection<UserSetting> UserSettings { get; set; }

        //For foreign key fk__print_history__users
        [InverseProperty("User")]
        public virtual ICollection<PrintHistory> PrintHistorys { get; set; }


    }
}