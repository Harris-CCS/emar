using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Medications.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

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

        /// <summary>
        /// Record the interactions and reactions associated with a patient's order.
        /// </summary>
        /// <param name="medicationInteractionsReactions">
        /// Interactions and reactions associated with the order
        /// </param>
        /// <param name="orderId">
        /// Identifier for the order
        /// </param>
        /// <param name="orderType">
        /// EmarOrderType enum that specifies the type of order (PatientOrder or PatientCartOrder)
        /// </param>
        /// <param name="insertOnly">
        /// Flag that specifies how the provided interactions and reactions should be processed against the order
        /// </param>
        /// <returns>Boolean success/failure</returns>
        public bool RecordNewInteractionsReactions(IEnumerable<MedicationInteractionReaction> medicationInteractionsReactions, long orderId, EmarOrderType orderType, bool insertOnly = true)
        {
            int i;
            var medicationInteractionsList = new List<List<MedicationInteraction>>();
            var orderReactionsList = new List<List<OrderReaction>>();

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                GetInteractionsReactions(medicationInteractionsReactions, orderId, orderType, ref medicationInteractionsList, ref orderReactionsList);

                // When insertOnly flag is true, we insert all of the provided flags, without any checks.
                // This would be used when a new order is created.
                if (insertOnly)
                {
                    foreach (var list in medicationInteractionsList)
                    {
                        _context.MedicationInteractions.AddRange(list);
                    }

                    foreach (var list in orderReactionsList)
                    {
                        _context.OrderReactions.AddRange(list);
                    }

                // When insertOnly flag is false, we will keep existing matched flags (without duplication), add new flags when necessary,
                // and remove flags that are no longer applicable. This would be used for existing orders when alg/med information changes
                // on the patient.
                }
                else 
                {
                    // Interaction processing
                    var currentInteractions =
                        orderType == EmarOrderType.PatientOrder ?
                            _context.PatientOrders
                                .Include(o => o.OrderInteractions)
                                    .ThenInclude(i => i.DrugInteractionView)
                                .Where(o => o.Id == orderId)?.First()?.OrderInteractions :
                        orderType == EmarOrderType.PatientCartOrder ?
                            _context.PatientCartOrders
                                .Include(o => o.OrderInteractions)
                                    .ThenInclude(i => i.DrugInteractionView)
                                .Where(o => o.Id == orderId)?.First()?.OrderInteractions :
                        null;

                    bool currentInteractionsListIsEmpty = (currentInteractions == null || currentInteractions.Count() == 0);
                    string orderTable = (orderType == EmarOrderType.PatientOrder) ? "patient_orders" : "patient_cart_orders";

                    // In this case we have no interactions anymore, so remove all interactions on the order.
                    if (medicationInteractionsList == null || medicationInteractionsList.Count() == 0)
                    {
                        _context.OrderInteractions.RemoveRange(
                            _context.OrderInteractions.Where(
                                r => (orderType == EmarOrderType.PatientOrder) ? r.PatientOrderId == orderId : r.PatientCartOrderId == orderId
                            )
                        );
                    }
                    else
                    {
                        // First handle inserts/updates
                        foreach (var list in medicationInteractionsList)
                        {
                            if (currentInteractionsListIsEmpty)
                            {
                                _context.MedicationInteractions.AddRange(list);
                            }
                            else
                            {
                                foreach (var item in list)
                                {
                                    var matchRow = currentInteractions?.Where(i =>
                                        i.DrugInteractionView?.InteractionOrderTable == orderTable &&
                                        i.DrugInteractionView?.InteractionDrug1 == item.InteractionDrug1 &&
                                        i.DrugInteractionView?.InteractionDrug2 == item.InteractionDrug2
                                    )?.FirstOrDefault();

                                    if (matchRow == null)
                                    {
                                        _context.MedicationInteractions.Add(item);
                                    }
                                }
                            }
                        }

                        // Now handle removals
                        foreach (var currentInteraction in currentInteractions)
                        {
                            if (currentInteraction.Id <= 0 || _context.Entry(currentInteraction).State == EntityState.Added)
                            {
                                continue;
                            }

                            bool foundInteraction = false;
                            foreach (var list in medicationInteractionsList)
                            {
                                var matchRow = list.Where(i =>
                                    (i.InteractionDrug1 == currentInteraction.DrugInteractionView?.InteractionDrug1 ||
                                     i.InteractionDrug2 == currentInteraction.DrugInteractionView?.InteractionDrug1) &&
                                    (i.InteractionDrug1 == currentInteraction.DrugInteractionView?.InteractionDrug2 ||
                                     i.InteractionDrug2 == currentInteraction.DrugInteractionView?.InteractionDrug2) &&
                                    currentInteraction.DrugInteractionView?.InteractionOrderTable == orderTable
                                )?.FirstOrDefault();

                                if (matchRow != null)
                                {
                                    foundInteraction = true;
                                    break;
                                }
                            }

                            if (!foundInteraction)
                            {
                                _context.OrderInteractions.Remove(currentInteraction);
                            }
                        }
                    }

                    // Reaction processing
                    var currentReactions =
                        orderType == EmarOrderType.PatientOrder ?
                            _context.PatientOrders
                                .Include(o => o.AllergyReactionsView)
                                .Where(o => o.Id == orderId)?.First()?.AllergyReactionsView :
                        orderType == EmarOrderType.PatientCartOrder ?
                            _context.PatientCartOrders
                                .Include(o => o.AllergyReactionsView)
                                .Where(o => o.Id == orderId)?.First()?.AllergyReactionsView :
                        null;

                    bool currentReactionsListIsEmpty = (currentReactions == null || currentReactions.Count() == 0);

                    // In this case we have no reactions anymore, so remove all reactions on the order.
                    if (orderReactionsList == null || orderReactionsList.Count() == 0)
                    {
                        _context.OrderReactions.RemoveRange(
                            _context.OrderReactions.Where(
                                r => (orderType == EmarOrderType.PatientOrder) ? r.PatientOrderId == orderId : r.PatientCartOrderId == orderId
                            )
                        );
                    }
                    else
                    {
                        // First handle inserts/updates
                        foreach (var list in orderReactionsList)
                        {
                            if (currentReactionsListIsEmpty)
                            {
                                _context.OrderReactions.AddRange(list);
                            }
                            else
                            {
                                foreach (var item in list)
                                {
                                    var matchRow = currentReactions?.Where(i =>
                                        i.OrderTable == orderTable &&
                                        i.PatientAllergyId == item.PatientAllergyId
                                    )?.FirstOrDefault();

                                    if (matchRow == null)
                                    {
                                        _context.OrderReactions.Add(item);
                                    }
                                }
                            }
                        }

                        // Now handle removals
                        foreach (var currentReaction in currentReactions)
                        {
                            if (currentReaction.Id <= 0 || _context.Entry(currentReaction).State == EntityState.Added)
                            {
                                continue;
                            }

                            bool foundReaction = false;
                            foreach (var list in orderReactionsList)
                            {
                                var matchRow = list.Where(i =>
                                    i.PatientAllergyId == currentReaction.PatientAllergyId &&
                                    currentReaction.OrderTable == orderTable
                                )?.FirstOrDefault();

                                if (matchRow != null)
                                {
                                    foundReaction = true;
                                    break;
                                }
                            }

                            if (!foundReaction)
                            {
                                _context.OrderReactions.RemoveRange(
                                    _context.OrderReactions.Where(
                                        o => (orderType == EmarOrderType.PatientOrder) ? o.PatientOrderId == orderId : o.PatientCartOrderId == orderId &&
                                        o.PatientAllergyId == currentReaction.PatientAllergyId
                                    )
                                );
                            }
                        }
                    }
                }

                i = _context.SaveChanges(true);
                transaction.Commit();
            }
            catch (Exception)
            {
                i = 0;
                transaction.Rollback();
            }

            return i > 0;
        }
    }
}