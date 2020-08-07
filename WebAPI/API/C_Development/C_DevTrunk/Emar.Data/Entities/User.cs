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
            OrderAdministrationsAcknowledgeUser = new HashSet<OrderAdministration>();
            OrderAdministrationAdministeringUser = new HashSet<OrderAdministration>();
            OrderAdministrationStopUser = new HashSet<OrderAdministration>();
            PatientCartOrders = new HashSet<PatientCartOrder>();
            PatientOrdersAddUser = new HashSet<PatientOrder>();
            PatientOrdersOrderPhysicianUser = new HashSet<PatientOrder>();
            UserQuickListItems = new HashSet<UserQuickListItem>();
        }

        [Key]
        [Column("id", TypeName = "int")]
        public int Id { get; set; }

        [Column("site_id", TypeName = "int"), Required]
        public long SiteId { get; set; }

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
        public bool? DisplayInitialsIndicator { get; set; }

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

        [ForeignKey(nameof(SiteId))]
        [InverseProperty(nameof(Entities.Site.Users))]
        public virtual Site Site { get; set; }

        [InverseProperty(nameof(OrderAdministration.AcknowledgeUser))]
        public virtual ICollection<OrderAdministration> OrderAdministrationsAcknowledgeUser { get; set; }

        [InverseProperty(nameof(OrderAdministration.AdministeringUser))]
        public virtual ICollection<OrderAdministration> OrderAdministrationAdministeringUser { get; set; }

        [InverseProperty(nameof(OrderAdministration.StopUser))]
        public virtual ICollection<OrderAdministration> OrderAdministrationStopUser { get; set; }


        [InverseProperty(nameof(PatientOrder.AddUser))]
        public virtual ICollection<PatientOrder> PatientOrdersAddUser { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        [InverseProperty(nameof(PatientOrder.OrderPhysicianUser))]
        public virtual ICollection<PatientOrder> PatientOrdersOrderPhysicianUser { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<UserQuickListItem> UserQuickListItems { get; set; }
    }
}
