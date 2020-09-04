using System.ComponentModel.Design;
using System.Linq;
using Emar.Data.Entities;

namespace Emar.Core.Templates.Model.Mappings
{
    class TemplateMapper
    {
        public static TemplateDto MapTemplate(Template dbObj)
        {
            
                if (dbObj == null)
                {
                    return null;
                }

                var ret = new TemplateDto
                {
                    Id = dbObj.Id,
                    Name = dbObj.Name,
                    Active = dbObj.IsActive,
                    Title = dbObj.Title,
                    SiteId = dbObj.SiteId,
                    PromptGroups = dbObj.TemplatePromptGroups
                        .Select(tpg => tpg.PromptGroup)
                        .Select(MapPromptGroup).ToList()
                };

                return ret;
            
        }

        private static PromptGroupDto MapPromptGroup(PromptGroup dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new PromptGroupDto()
            {
                Id = dbObj.Id,
                Name = dbObj.Name,
                DisplayTitle = dbObj.Title,
                Prompts = dbObj.Prompts?.Select(MapPrompt).ToList()
            };

            return ret;
        }

        private static PromptDto MapPrompt(Prompt dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new PromptDto()
            {
                Id = dbObj.Id,
                PromptGroupId = dbObj.PromptGroupId,
                Sequence = dbObj.Sequence,
                Prompt = dbObj.PromptText,
                IsActive = dbObj.IsActive,
                Type = dbObj.PromptType,
                Default = dbObj.PromptDefault,
                Required = dbObj.Required,
                PromptChoices = dbObj.PromptChoices?.Select(MapPromptChoice).ToList()
            };

            return ret;
        }

        private static PromptChoiceDto MapPromptChoice(PromptChoice dbObj)
        {
            if (dbObj == null)
            {
                return null;
            }

            var ret = new PromptChoiceDto
            {
                Id = dbObj.Id,
                PromptId = dbObj.PromptId,
                Sequence = dbObj.Sequence,
                ChoiceText = dbObj.ChoiceText
            };

            return ret;
        }
    }
}
