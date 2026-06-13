using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Emar.Data.Entities
{
    [Table("future_administrations_reschedule")]
    public class FutureAdministrationsReschedule
    {
        [Column("id", TypeName = "int"), Key]
        public int Id { get; set; }

        [Column("patient_order_id", TypeName = "bigint")]
        public long PatientOrderId { get; set; }

        [Column("time_offset_minutes", TypeName = "int")]
        public int TimeOffsetMinutes { get; set; }

        //for foreign key fk__future_administration_reschedule__patient_orders
        [ForeignKey(nameof(PatientOrderId))]
        [InverseProperty(nameof(Entities.PatientOrder.FutureAdministrationsReschedules))]
        public virtual PatientOrder PatientOrder { get; set; }
    }
}
