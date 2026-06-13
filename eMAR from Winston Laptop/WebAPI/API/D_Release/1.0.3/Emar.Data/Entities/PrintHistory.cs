using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("print_history")]
    public partial class PrintHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("print_user_id")]
        public int PrintUserId { get; set; }

        [Column("device_id")]
        public int DeviceId { get; set; }

        [Column("patient_id")]
        public long PatientId { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("document_type")]
        public string DocumentType { get; set; }

        [Column("file_name")]
        public string FileName { get; set; }

        [Column("file_format")]
        public string FileFormat { get; set; }

        [Column("page_count")]
        public int? PageCount { get; set; }

        [Column("print_datetime")]
        public DateTimeOffset? PrintDateTime { get; set; }

        [Column("expiration_datetime")]
        public DateTimeOffset? ExpirationDateTime { get; set; }


        [NotMapped]
        public string PrintBody { get; set; }


        //For foreign key fk__print_history__users
        [ForeignKey(nameof(PrintUserId))]
        [InverseProperty(nameof(Entities.User.PrintHistorys))]
        public virtual User User { get; set; }

        //For foreign key fk__print_history__devices
        [ForeignKey(nameof(DeviceId))]
        [InverseProperty(nameof(Entities.Device.PrintHistorys))]
        public virtual Device Device { get; set; }

        //For foreign key fk__print_history__patients
        [ForeignKey(nameof(PatientId))]
        [InverseProperty(nameof(Entities.Patient.PrintHistorys))]
        public virtual Patient Patient { get; set; }

    }
}
