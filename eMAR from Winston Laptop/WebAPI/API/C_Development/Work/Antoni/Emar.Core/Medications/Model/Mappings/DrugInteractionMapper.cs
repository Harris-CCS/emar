////////////using Emar.Data.Entities;

////////////namespace Emar.Core.Medications.Model.Mappings
////////////{
////////////    public static class DrugInteractionMapper
////////////    {
////////////        public static DrugInteractionDto MapDrugInteraction(DrugInteraction drugInteraction)
////////////        {
////////////            if (drugInteraction == null)
////////////            {
////////////                return null;
////////////            }

////////////            DrugInteractionDto drugInteractionDto = new DrugInteractionDto
////////////            {
////////////                Drug_id_1 = drugInteraction.Drug_id_1,
////////////                Drug_id_2 = drugInteraction.Drug_id_2,
////////////                Int_id = drugInteraction.Int_id,
////////////                Dnum2 = drugInteraction.Dnum2,
////////////                Dname2 = drugInteraction.Dname2,
////////////                Dnum = drugInteraction.Dnum,
////////////                Drug = drugInteraction.Drug,
////////////                Severity_id = drugInteraction.Severity_id,
////////////                Sevtxt = drugInteraction.Sevtxt,
////////////                Interaction = drugInteraction.Interaction,
////////////                SourceTable2 = drugInteraction.SourceTable2,
////////////                SourceTableId2 = drugInteraction.SourceTableId2
////////////            };

////////////            return drugInteractionDto;
////////////        }

////////////        public static DrugInteraction MapDrugInteractionDto(DrugInteractionDto drugInteractionDto)
////////////        {
////////////            if (drugInteractionDto == null)
////////////            {
////////////                return null;
////////////            }

////////////            DrugInteraction drugInteraction = new DrugInteraction
////////////            {
////////////                Drug_id_1 = drugInteractionDto.Drug_id_1,
////////////                Drug_id_2 = drugInteractionDto.Drug_id_2,
////////////                Int_id = drugInteractionDto.Int_id,
////////////                Dnum2 = drugInteractionDto.Dnum2,
////////////                Dname2 = drugInteractionDto.Dname2,
////////////                Dnum = drugInteractionDto.Dnum,
////////////                Drug = drugInteractionDto.Drug,
////////////                Severity_id = drugInteractionDto.Severity_id,
////////////                Sevtxt = drugInteractionDto.Sevtxt,
////////////                Interaction = drugInteractionDto.Interaction,
////////////                SourceTable2 = drugInteractionDto.SourceTable2,
////////////                SourceTableId2 = drugInteractionDto.SourceTableId2
////////////            };

////////////            return drugInteraction;
////////////        }
////////////    }
////////////}
