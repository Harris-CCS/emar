using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.FdbObjects.Model.Mappings;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.ResourceParameters;
using Emar.Core.Sites.Model.Mappings;
using Emar.Core.Users.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Medications.Model.Mappings
{
    public static class MedicationMapper
    {
        private static DrugInteractionViewDto MapDrugInteractionsView(DrugInteractionView drugInteractionsView, string drugDbVendor, BaseLinkResource resource)
        {
            if (drugInteractionsView == null)
            {
                return null;
            }

            var drugInteractionViewDto = new DrugInteractionViewDto
            {
                Id = drugInteractionsView.Id,
                InteractionDrug1 = drugInteractionsView.InteractionDrug1,
                InteractionDrug2 = drugInteractionsView.InteractionDrug2,
                Severity = drugDbVendor switch
                {
                    DrugDbVendors.FDB => ((FdbInteractionSeverity)drugInteractionsView.Severity).ToString(),
                    DrugDbVendors.Multum => ((MultumInteractionSeverity)drugInteractionsView.Severity).ToString(),
                    _ => ""
                },
                OverrideReasonDatetime = drugInteractionsView.OverrideReasonDatetime,
                OverrideReasonUser = UserMapper.MapUser(drugInteractionsView.OverrideReasonUser),
                OverrideReason = MapOverrideReason(drugInteractionsView.OverrideReason),
                InteractionOrderId = drugInteractionsView.InteractionOrderId,
                InteractionOrderTable = drugInteractionsView.InteractionOrderTable,
                InteractionOrderName = drugInteractionsView.InteractionOrderName
            };

            if (drugInteractionsView.InteractionOrderId != null && resource != null)
            {
                switch (drugInteractionsView.InteractionOrderTable)
                {
                    case "patient_orders":
                        if (!string.IsNullOrEmpty(resource.LinkGetPatientOrder))
                        {
                            drugInteractionViewDto.InteractionOrderLink =
                                new HateOasLinkDto(resource.LinkGetPatientOrder.Replace("/-99", "/" + drugInteractionsView.InteractionOrderId),
                                    "get_interaction_patient_order",
                                    "GET");
                        }

                        break;
                    case "patient_cart_orders":
                        if (!string.IsNullOrEmpty(resource.LinkGetCartOrder))
                        {
                            drugInteractionViewDto.InteractionOrderLink =
                                new HateOasLinkDto(resource.LinkGetCartOrder.Replace("/-99", "/" + drugInteractionsView.InteractionOrderId),
                                    "get_interaction_patient_cart_order",
                                    "GET");
                        }

                        break;
                    case "patient_home_medications":
                        if (!string.IsNullOrEmpty(resource.LinkGetHomeMedication))
                        {
                            drugInteractionViewDto.InteractionOrderLink =
                                new HateOasLinkDto(resource.LinkGetHomeMedication.Replace("/-99", "/" + drugInteractionsView.InteractionOrderId),
                                    "get_interaction_patient_home_medication",
                                    "GET");
                        }

                        break;
                }
            }

            return drugInteractionViewDto;
        }

        public static AllergyReactionView MapAllergyReactionViewDto(AllergyReactionViewDto allergyReactionViewDto)
        {
            if (allergyReactionViewDto == null)
            {
                return null;
            }

            var allergyReactionView = new AllergyReactionView
            {
                Id = allergyReactionViewDto.Id,
                PatientAllergyId = allergyReactionViewDto.PatientAllergyId,
                PatientAllergyName = allergyReactionViewDto.PatientAllergyName,
                OrderTable = allergyReactionViewDto.OrderTable,
                OrderId = allergyReactionViewDto.OrderId,
                OrderBrandName = allergyReactionViewDto.OrderBrandName,
                OverrideReasonId = allergyReactionViewDto.OverrideReasonId,
                OverrideReasonUserId = allergyReactionViewDto.OverrideReasonUserId,
                OverrideReasonDatetime = allergyReactionViewDto.OverrideReasonDatetime
            };

            return allergyReactionView;
        }

        public static AllergyReactionViewDto MapAllergyReactionView(AllergyReactionView allergyReactionView)
        {
            if (allergyReactionView == null)
            {
                return null;
            }

            var allergyReactionViewDto = new AllergyReactionViewDto
            {
                Id = allergyReactionView.Id,
                PatientAllergyId = allergyReactionView.PatientAllergyId,
                PatientAllergyName = allergyReactionView.PatientAllergyName,
                PatientAllergySeverity = allergyReactionView.PatientAllergySeverity,
                OrderTable = allergyReactionView.OrderTable,
                OrderId = allergyReactionView.OrderId,
                OrderBrandName = allergyReactionView.OrderBrandName,
                OverrideReason = MapOverrideReason(allergyReactionView.OverrideReason),
                OverrideReasonUser = UserMapper.MapUser(allergyReactionView.OverrideReasonUser),
                OverrideReasonDatetime = allergyReactionView.OverrideReasonDatetime
            };

            return allergyReactionViewDto;
        }

        private static DrugInteractionView MapDrugInteractionsViewDto(DrugInteractionViewDto drugInteractionsViewDto, string drugDbVendor)
        {
            if (drugInteractionsViewDto == null)
            {
                return null;
            }

            byte severityValue = 0;
            if (drugDbVendor.Equals(DrugDbVendors.Multum))
            {
                severityValue = Enum.TryParse(drugInteractionsViewDto.Severity, out MultumInteractionSeverity severity) ? (byte)severity : (byte)0;
            }
            else if (drugDbVendor.Equals(DrugDbVendors.FDB))
            {
                severityValue = Enum.TryParse(drugInteractionsViewDto.Severity, out FdbInteractionSeverity severity) ? (byte)severity : (byte)0;
            }

            var drugInteractionView = new DrugInteractionView
            {
                Id = drugInteractionsViewDto.Id,
                InteractionDrug1 = drugInteractionsViewDto.InteractionDrug1,
                InteractionDrug2 = drugInteractionsViewDto.InteractionDrug2,
                Severity = severityValue,
                OverrideReasonDatetime = drugInteractionsViewDto.OverrideReasonDatetime,
                OverrideReasonUserId = drugInteractionsViewDto.OverrideReasonUserId,
                OverrideReasonId = drugInteractionsViewDto.OverrideReasonId,
                InteractionOrderId = drugInteractionsViewDto.InteractionOrderId,
                InteractionOrderTable = drugInteractionsViewDto.InteractionOrderTable,
                InteractionOrderName = drugInteractionsViewDto.InteractionOrderName
            };

            return drugInteractionView;
        }

        public static OrderInteractionDto MapOrderInteraction(MedicationInteraction medicationInteraction, string drugDbVendor, BaseLinkResource resource, int? codeShareSiteMedicationUnit)
        {
            if (medicationInteraction == null)
            {
                return null;
            }

            var orderInteractionDto = new OrderInteractionDto
            {
                DrugInteraction =
                    new DrugInteractionViewDto
                    {
                        Id = 0,
                        InteractionDrug1 = medicationInteraction.InteractionDrug1,
                        InteractionDrug2 = medicationInteraction.InteractionDrug2,
                        Severity = drugDbVendor switch
                        {
                            DrugDbVendors.FDB => ((FdbInteractionSeverity)medicationInteraction.Severity).ToString(),
                            DrugDbVendors.Multum => ((MultumInteractionSeverity)medicationInteraction.Severity).ToString(),
                            _ => ""
                        },
                        OverrideReasonId = medicationInteraction.OverrideReasonId,
                        OverrideReasonUserId = medicationInteraction.OverrideReasonUserId,
                        OverrideReasonDatetime = medicationInteraction.OverrideReasonDatetime,
                        InteractionOrderId = medicationInteraction.InteractionOrderId,
                        InteractionOrderTable = medicationInteraction.InteractionOrderTable,
                        InteractionOrderName = medicationInteraction.InteractionOrderName,
                        InteractionMedication = MapMedication(medicationInteraction.InteractionMedication, codeShareSiteMedicationUnit)
                    }
            };

            if (medicationInteraction.InteractionOrderId != null)
            {
                switch (medicationInteraction.InteractionOrderTable)
                {
                    case "patient_orders":
                        if (!string.IsNullOrEmpty(resource.LinkGetPatientOrder))
                        {
                            orderInteractionDto.DrugInteraction.InteractionOrderLink =
                                new HateOasLinkDto(resource.LinkGetPatientOrder.Replace("/-99", "/" + medicationInteraction.InteractionOrderId),
                                    "get_interaction_patient_order",
                                    "GET");
                        }

                        break;
                    case "patient_cart_orders":
                        if (!string.IsNullOrEmpty(resource.LinkGetCartOrder))
                        {
                            orderInteractionDto.DrugInteraction.InteractionOrderLink =
                                new HateOasLinkDto(resource.LinkGetCartOrder.Replace("/-99", "/" + medicationInteraction.InteractionOrderId),
                                    "get_interaction_patient_cart_order",
                                    "GET");
                        }

                        break;
                    case "patient_home_medications":
                        if (!string.IsNullOrEmpty(resource.LinkGetHomeMedication))
                        {
                            orderInteractionDto.DrugInteraction.InteractionOrderLink =
                                new HateOasLinkDto(resource.LinkGetHomeMedication.Replace("/-99", "/" + medicationInteraction.InteractionOrderId),
                                    "get_interaction_patient_home_medication",
                                    "GET");
                        }

                        break;
                }
            }

            return orderInteractionDto;
        }

        public static OrderInteractionDto MapOrderInteraction(OrderInteraction orderInteraction, string drugDbVendor, BaseLinkResource resource)
        {
            if (orderInteraction == null)
            {
                return null;
            }

            var orderInteractionDto = new OrderInteractionDto
            {
                Id = orderInteraction.Id,
                MedicationInteractionId = orderInteraction.MedicationInteractionId,
                //DrugNum = orderInteraction.DrugNum,
                PatientOrderId = orderInteraction.PatientOrderId,
                PatientCartOrderId = orderInteraction.PatientCartOrderId,
                PatientHomeMedicationId = orderInteraction.PatientHomeMedicationId,
                DrugInteraction = MapDrugInteractionsView(orderInteraction.DrugInteractionView, drugDbVendor, resource),
            };

            return orderInteractionDto;
        }

        public static OrderInteraction MapOrderInteractionDto(OrderInteractionDto orderInteractionDto, string drugDbVendor)
        {
            if (orderInteractionDto == null)
            {
                return null;
            }

            var orderInteraction = new OrderInteraction
            {
                Id = orderInteractionDto.Id,
                MedicationInteractionId = orderInteractionDto.MedicationInteractionId,
                PatientOrderId = orderInteractionDto.PatientOrderId,
                PatientCartOrderId = orderInteractionDto.PatientCartOrderId,
                PatientHomeMedicationId = orderInteractionDto.PatientHomeMedicationId,
                DrugInteractionView = MapDrugInteractionsViewDto(orderInteractionDto.DrugInteraction, drugDbVendor)
            };

            return orderInteraction;
        }

        private static OverrideReasonDto MapOverrideReason(OverrideReason overrideReason)
        {
            if (overrideReason == null)
            {
                return null;
            }

            var overrideReasonDto = new OverrideReasonDto
            {
                Id = overrideReason.Id,

                SiteId = overrideReason.SiteId,
                IsMedication = overrideReason.IsMedication,
                Description = overrideReason.Description
            };

            return overrideReasonDto;
        }

        private static Medication MapMedicationDto(MedicationDto dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new Medication
            {
                Id = dbObj.Id,
                SiteId = dbObj.SiteId,
                DrugId = dbObj.DrugId,
                DisplayName = dbObj.DisplayName,
                DrugVendor = dbObj.DrugVendor,
                MedicationDetails = dbObj.MedicationDetails?.Select(MapMedicationDetailDto).ToList()
            };

            return ret;
        }

        public static MedicationDto MapMedication(Medication dbObj, int? codeShareSiteMedicationUnit)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new MedicationDto
            {
                Id = dbObj.Id,
                SiteId = dbObj.SiteId,
                Site = SiteMapper.MapSite(dbObj.Site),
                DrugId = dbObj.DrugId,
                DisplayName = dbObj.DisplayName,
                DrugVendor = dbObj.DrugVendor,
                MedicationDetails = dbObj.MedicationDetails?.Select(md => MapMedicationDetail(md, codeShareSiteMedicationUnit)).ToList()
            };

            return ret;
        }

        public static MedicationDetailDto MapMedicationDetail(MedicationDetail dbObj, int? codeShareSiteMedicationUnit)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new MedicationDetailDto
            {
                Id = dbObj.Id,
                MedicationId = dbObj.MedicationId,
                DrugId = dbObj.DrugId,
                BrandName = dbObj.BrandName,
                ActiveList = dbObj.ActiveList,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                DoseUnit = dbObj.MedicationUnit != null
                           && dbObj.Medication.DrugId != "COMBO"
                           //////&& dbObj.Medication.SiteId != -1
                           && dbObj.MedicationUnit.SiteId == codeShareSiteMedicationUnit
                    ? OrderMapper.MapMedicationUnit(dbObj.MedicationUnit)
                    : null,
                IsActive = dbObj.IsActive,
                FdbBrandName = FdbObjectsMapper.MapFdbBrandName(dbObj.FdbBrandName)
            };

            return ret;
        }

        private static MedicationDetail MapMedicationDetailDto(MedicationDetailDto dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new MedicationDetail
            {
                Id = dbObj.Id,
                MedicationId = dbObj.MedicationId,
                DrugId = dbObj.DrugId,
                BrandName = dbObj.BrandName,
                ActiveList = dbObj.ActiveList,
                Dose = dbObj.Dose,
                MedicationUnitId = dbObj.MedicationUnitId,
                IsActive = dbObj.IsActive
            };

            return ret;
        }

        public static AntimicrobialIndicationDto MapAntimicrobial(AntimicrobialIndication dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new AntimicrobialIndicationDto
            {
                Id = dbObj.Id,
                SiteId = dbObj.SiteId,
                Code = dbObj.Code,
                Description = dbObj.Description,
                IsActive = dbObj.IsActive,
                OrdinalPosition = dbObj.OrdinalPosition
            };

            return ret;
        }

        public static MedicationInteraction MapInteractionDictionaryToMedicationInteraction(Dictionary<string, object> interaction)
        {
            return new MedicationInteraction
            {
                InteractionDrug1 = interaction.GetValueOrDefault("drug_id_1")?.ToString(),
                InteractionDrug2 = interaction.GetValueOrDefault("drug_id_2")?.ToString(),
                Severity = byte.TryParse(interaction.GetValueOrDefault("severity_id")?.ToString(), out byte byteValue)
                    ? byteValue
                    : (byte)0,
                InteractionOrderId = long.TryParse(interaction.GetValueOrDefault("SourceTableId2")?.ToString(), out long tableId) ? tableId : (long?)null,
                InteractionOrderTable = interaction.GetValueOrDefault("SourceTable2")?.ToString(),
                InteractionOrderName = interaction.GetValueOrDefault("dname2")?.ToString(),
                InteractionMedication = MapMedicationDto((MedicationDto)interaction.GetValueOrDefault("SourceTableMedication2", null))
            };
        }

        public static AllergyReactionViewDto MapReactionDictionaryToAllergyReactionViewDto(Dictionary<string, object> reaction, MedicationInteractionReaction parentInteractionReaction)
        {
            return new AllergyReactionViewDto
            {
                PatientAllergyId = long.TryParse(reaction.GetValueOrDefault("SourceTableId")?.ToString(), out long number) ? number : 0,
                OrderId = parentInteractionReaction.SourceTableId,
                OrderTable = parentInteractionReaction.SourceTable,
                OrderBrandName = parentInteractionReaction.BrandName,
                PatientAllergyName = reaction.GetValueOrDefault("dname2")?.ToString(),
                PatientAllergySeverity = reaction.GetValueOrDefault("Severity")?.ToString()
            };
        }

        public static BrandNameSearchDto MapBrandName(BrandNameReturnDto dbObj, string schedulerDataRetrieveBase)
        {
            if (dbObj == null)
            {
                return null;
            }

            return new BrandNameSearchDto
            {
                BrandName = dbObj.BrandName,
                InpatientMatch = dbObj.InpatientMatch,
                OutpatientMatch = dbObj.OutpatientMatch,
                PyxisMatch = dbObj.PyxisMatch,
                
                //We need to sort the return list by the match level.
                //Winston Murdock, 01/22/2021.EMAR-586.
                MatchLevel = dbObj.MatchLevel,

                IsBrandNameMatch = dbObj.IsBrandNameMatch,
                SearchPos = dbObj.SearchPos,

                Link =  new HateOasLinkDto(
                    string.Format(schedulerDataRetrieveBase, dbObj), 
                    "Retrieve Scheduler Options", 
                    "GET")
            };
        }
    }
}