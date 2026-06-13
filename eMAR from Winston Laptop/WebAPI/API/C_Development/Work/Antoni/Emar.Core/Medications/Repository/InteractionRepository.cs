using System;
using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Repository
{
    public class InteractionRepository : IInteractionRepository
    {
        private readonly EmarContext _context;

        public InteractionRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }

        public void GetInteractionsReactions(IEnumerable<MedicationInteractionReaction> medicationInteractionsReactions, long orderId, EmarOrderType orderType, ref List<List<MedicationInteraction>> medicationInteractionsList, ref List<List<OrderReaction>> orderReactionsList)
        {
            foreach (var interactionReaction in medicationInteractionsReactions)
            {
                // Drug Interactions
                var medicationInteractions = new List<MedicationInteraction>();

                foreach (var interaction in interactionReaction.Interactions)
                {
                    var medicationInteraction = new MedicationInteraction
                    {
                        InteractionDrug1 = interaction.GetValueOrDefault("drug_id_1")?.ToString(),
                        InteractionDrug2 = interaction.GetValueOrDefault("drug_id_2")?.ToString(),
                        Severity = byte.TryParse(interaction.GetValueOrDefault("severity_id")?.ToString(), out byte byteValue) ? byteValue : (byte)0
                    };

                    switch (orderType)
                    {
                        case EmarOrderType.PatientOrder:
                            medicationInteraction.OrderInteractions.Add(
                                new OrderInteraction
                                {
                                    MedicationInteractionId = medicationInteraction.Id,
                                    DrugNum = 1,
                                    PatientOrderId = orderId
                                });
                            break;
                        case EmarOrderType.PatientCartOrder:
                            medicationInteraction.OrderInteractions.Add(
                                new OrderInteraction
                                {
                                    MedicationInteractionId = medicationInteraction.Id,
                                    DrugNum = 1,
                                    PatientCartOrderId = orderId
                                });
                            break;
                        case EmarOrderType.HomeMedication:
                            medicationInteraction.OrderInteractions.Add(
                                new OrderInteraction
                                {
                                    MedicationInteractionId = medicationInteraction.Id,
                                    DrugNum = 1,
                                    PatientHomeMedicationId = orderId
                                });
                            break;
                    }

                    var id = long.TryParse(interaction.GetValueOrDefault("SourceTableId2")?.ToString(), out long number) ? number : (long?)null;

                    switch (interaction.GetValueOrDefault("SourceTable2"))
                    {
                        case SourceTables.PatientOrders:
                            medicationInteraction.OrderInteractions.Add(
                                new OrderInteraction
                                {
                                    MedicationInteractionId = medicationInteraction.Id,
                                    DrugNum = 2,
                                    PatientOrderId = id
                                });
                            break;
                        case SourceTables.PatientCartOrders:
                            medicationInteraction.OrderInteractions.Add(
                                new OrderInteraction
                                {
                                    MedicationInteractionId = medicationInteraction.Id,
                                    DrugNum = 2,
                                    PatientCartOrderId = id
                                });
                            break;
                        case SourceTables.PatientHomeMedications:
                            medicationInteraction.OrderInteractions.Add(
                                new OrderInteraction
                                {
                                    MedicationInteractionId = medicationInteraction.Id,
                                    DrugNum = 2,
                                    PatientHomeMedicationId = id
                                });
                            break;
                    }

                    medicationInteractions.Add(medicationInteraction);
                }

                // Allergy Reactions
                var orderReactions = new List<OrderReaction>();

                foreach (var reaction in interactionReaction.Reactions)
                {
                    long? patientOrderId = null;
                    long? patientCartOrderId = null;

                    switch (orderType)
                    {
                        case EmarOrderType.PatientOrder:
                            patientOrderId = orderId;
                            break;
                        case EmarOrderType.PatientCartOrder:
                            patientCartOrderId = orderId;
                            break;
                    }

                    var orderReaction = new OrderReaction
                    {
                        PatientAllergyId = long.TryParse(reaction.GetValueOrDefault("SourceTableId")?.ToString(), out long number) ? number : 0,
                        PatientOrderId = patientOrderId,
                        PatientCartOrderId = patientCartOrderId
                    };

                    orderReactions.Add(orderReaction);
                }

                medicationInteractionsList.Add(medicationInteractions);
                orderReactionsList.Add(orderReactions);
            }
        }

        public bool RecordNewInteractionsReactions(IEnumerable<MedicationInteractionReaction> medicationInteractionsReactions, long orderId, EmarOrderType orderType)
        {
            int i;
            var medicationInteractionsList = new List<List<MedicationInteraction>>();
            var orderReactionsList = new List<List<OrderReaction>>();

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                GetInteractionsReactions(medicationInteractionsReactions, orderId, orderType, ref medicationInteractionsList, ref orderReactionsList);

                foreach (var list in medicationInteractionsList)
                {
                    _context.MedicationInteractions.AddRange(list);
                }

                foreach (var list in orderReactionsList)
                {
                    _context.OrderReactions.AddRange(list);
                }

                i = _context.SaveChanges(true);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                i = 0;
                transaction.Rollback();
            }

            return i > 0;
        }
    }
}