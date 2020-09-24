using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emar.Data.Entities
{
    [Table("order_interactions")]
    public partial class OrderInteraction
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("medication_interaction_id")]
        public long? MedicationInteractionId { get; set; }
        [Column("drug_num")]
        public byte DrugNum { get; set; }
        [Column("patient_order_id")]
        public long? PatientOrderId { get; set; }
        [Column("patient_cart_order_id")]
        public long? PatientCartOrderId { get; set; }
        [Column("patient_home_medication_id")]
        public long? PatientHomeMedicationId { get; set; }

        [ForeignKey(nameof(MedicationInteractionId))]
        [InverseProperty(nameof(Entities.MedicationInteraction.OrderInteractions))]
        public virtual MedicationInteraction MedicationInteraction { get; set; }
        [ForeignKey(nameof(PatientCartOrderId))]
        [InverseProperty(nameof(Entities.PatientCartOrder.OrderInteractions))]
        public virtual PatientCartOrder PatientCartOrder { get; set; }
        [ForeignKey(nameof(PatientHomeMedicationId))]
        [InverseProperty(nameof(Entities.PatientHomeMedication.OrderInteractions))]
        public virtual PatientHomeMedication PatientHomeMedication { get; set; }
        [ForeignKey(nameof(PatientOrderId))]
        [InverseProperty(nameof(Entities.PatientOrder.OrderInteractions))]
        public virtual PatientOrder PatientOrder { get; set; }


        [ForeignKey(nameof(MedicationInteractionId))]
        [InverseProperty(nameof(Entities.DrugInteractionView.OrderInteractions))]
        public virtual DrugInteractionView DrugInteractionView { get; set; }
    }
}