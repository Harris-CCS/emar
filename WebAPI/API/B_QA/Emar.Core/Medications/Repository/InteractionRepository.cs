using System;
using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Emar.Core.Medications.Repository
{
    public class InteractionRepository : IInteractionRepository
    {
        private readonly EmarContext _context;

        public InteractionRepository(EmarContext emarContext)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
        }

        public bool RecordNewInteractionsReactions(IEnumerable<MedicationInteractionReaction> medicationInteractionsReactions, long orderId, EmarOrderType orderType)
        {
            int i = 0;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Drug Interactions
                    foreach (var interactionReaction in medicationInteractionsReactions)
                    {
                        foreach (var interaction in interactionReaction.Interactions)
                        {
                            var medicationInteraction = new MedicationInteraction
                            {
                                InteractionDrug1 = interaction.GetValueOrDefault("drug_id_1"),
                                InteractionDrug2 = interaction.GetValueOrDefault("drug_id_2"),
                                Severity = byte.TryParse(interaction.GetValueOrDefault("severity_id"), out byte byteValue) ? byteValue : (byte)0
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

                            long? id = long.TryParse(interaction.GetValueOrDefault("SourceTableId2"), out long number) ? number : (long?)null;

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

                            _context.MedicationInteractions.Add(medicationInteraction);
                        }

                        // Allergy Reactions
                        foreach (var reaction in interactionReaction.Reactions)
                        {
                            long? PatientOrderId = null;
                            long? PatientCartOrderId = null;

                            switch (orderType)
                            {
                                case EmarOrderType.PatientOrder:
                                    PatientOrderId = orderId;
                                    break;
                                case EmarOrderType.PatientCartOrder:
                                    PatientCartOrderId = orderId;
                                    break;
                            }

                            var orderReaction = new OrderReaction
                            {
                                PatientAllergyId = long.TryParse(reaction.GetValueOrDefault("SourceTableId"), out long number) ? number : 0,
                                PatientOrderId = PatientOrderId,
                                PatientCartOrderId = PatientCartOrderId
                            };

                            _context.OrderReactions.Add(orderReaction);
                        }
                    }

                    i = _context.SaveChanges(true);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    i = 0;
                    transaction.Rollback();
                }
            }

            return i > 0;
        }
    }
}