using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_cart_orders")]
    public class PatientCartOrder
    {
        public PatientCartOrder()
        {
            CartOrderAdministrations = new HashSet<CartOrderAdministration>();
        }

        [Key]
        [Column("id", TypeName = "bigint")]
        public long Id { get; set; }

        [Column("patient_id", TypeName = "bigint"), Required]
        public long PatientId { get; set; }

        [Column("user_id", TypeName = "int"), Required]
        public int UserId { get; set; }

        [Column("add_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset AddDatetime { get; set; }

        [Column("ndc")]
        [StringLength(32)]
        public string Ndc { get; set; }

        [Column("drug_id", TypeName = "varchar(32)"), Required]
        public string DrugId { get; set; }

        [Column("brand_name", TypeName = "nvarchar(255)"), Required]
        public string BrandName { get; set; }

        [Column("dose", TypeName = "decimal(11,2)")]
        public decimal? Dose { get; set; }

        [Column("medication_unit_id")]
        public int? MedicationUnitId { get; set; }

        [Column("medication_route_id", TypeName = "int")]
        public int? MedicationRouteId { get; set; }

        [Column("priority", TypeName = "tinyint"), Required]
        public byte Priority { get; set; }

        [Column("frequency_id", TypeName = "int")]
        public int? FrequencyId { get; set; }

        [Column("prn", TypeName = "bit"), Required]
        public bool Prn { get; set; }

        [Column("point_in_time", TypeName = "bit"), Required]
        public bool PointInTime { get; set; }

        [Column("begin_datetime", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset BeginDatetime { get; set; }

        [Column("end_datetime", TypeName = "datetimeoffset")]
        public DateTimeOffset? EndDatetime { get; set; }

        [Column("order_notes", TypeName = "nvarchar(MAX)")]
        public string OrderNotes { get; set; }

        [Column("user_quick_list_item_id", TypeName = "int")]
        public int? UserQuickListItemId { get; set; }

        [ForeignKey(nameof(MedicationRouteId))]
        [InverseProperty(nameof(Entities.MedicationRoute.PatientCartOrders))]
        public virtual MedicationRoute MedicationRoute { get; set; }

        [ForeignKey(nameof(MedicationUnitId))]
        [InverseProperty(nameof(Entities.MedicationUnit.PatientCartOrders))]
        public virtual MedicationUnit MedicationUnit { get; set; }

        [ForeignKey(nameof(PatientId))]
        [InverseProperty(nameof(Entities.Patient.PatientCartOrders))]
        public virtual Patient Patient { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Entities.User.PatientCartOrders))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(UserQuickListItemId))]
        [InverseProperty(nameof(Entities.UserQuickListItem.PatientCartOrders))]
        public virtual UserQuickListItem UserQuickListItem { get; set; }

        [InverseProperty("PatientCartOrder")]
        public virtual ICollection<CartOrderAdministration> CartOrderAdministrations { get; set; }
    }
}
