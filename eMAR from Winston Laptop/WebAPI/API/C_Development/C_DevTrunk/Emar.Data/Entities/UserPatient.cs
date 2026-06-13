using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("user_patients")]
    public class UserPatient
    {
        [Column("user_id", TypeName = "int")]
        public int UserId { get; set; }

        [Column("patient_id", TypeName = "bigint")]
        public long PatientId { get; set; }

        [Column("role_name", TypeName = "varchar(25)")]
        public string RoleName { get; set; }

        // For Foreign Key: fk__user_patients__users
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Entities.User.UserPatients))]
        public virtual User User { get; set; }

        // For Foreign Key: fk__user_patients__patients
        [ForeignKey(nameof(PatientId))]
        [InverseProperty(nameof(Entities.Patient.UserPatients))]
        public virtual Patient Patient { get; set; }

    }
}