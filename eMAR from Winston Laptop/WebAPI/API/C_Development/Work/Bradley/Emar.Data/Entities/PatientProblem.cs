using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("patient_problems")]
    public class PatientProblem
    {
        public PatientProblem()
        {
            // For Foreign Key: FK_patient_cart_orders_patient_problems_patient_problem_id
            PatientCartOrders = new HashSet<PatientCartOrder>();

            // For Foreign Key: FK_patient_orders_patient_problems_patient_problem_id
            PatientOrders = new HashSet<PatientOrder>();
        }

        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

        [Column("patient_id", TypeName = "bigint")]
        public long PatientId { get; set; }

        [Column("code_set_name", TypeName = "varchar(25)")]
        public string CodeSetName { get; set; }

        [Column("code_set_value", TypeName = "varchar(25)")]
        public string CodeSetValue { get; set; }

        [Column("problem_name", TypeName = "varchar(255)"), Required]
        public string ProblemName { get; set; }

        [Column("diagnosis_type", TypeName = "varchar(25)")]
        public string DiagnosisType { get; set; }

        // For Foreign Key: fk__users__patient_problems__patient_id
        [ForeignKey(nameof(PatientId))]
        [InverseProperty(nameof(Entities.Patient.PatientProblems))]
        public virtual Patient Patient { get; set; }

        // For Foreign Key: fk__patient_cart_orders__patient_problems
        [InverseProperty("PatientProblem")]
        public virtual ICollection<PatientCartOrder> PatientCartOrders { get; set; }

        // For Foreign Key: fk__patient_orders__patient_problems
        [InverseProperty("PatientProblem")]
        public virtual ICollection<PatientOrder> PatientOrders { get; set; }
    }
}