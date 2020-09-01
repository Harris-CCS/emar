using System;
using System.Collections.Generic;
using System.Text;
using Emar.Core.Templates.Model;

namespace Emar.Core.Templates.Service
{
    public class TemplateService : ITemplateService
    {
        public TemplateDto GetTemplateDefinition(int templateId)
        {
            var ret = new TemplateDto();
            ret.Id = 1234;
            ret.Name = "Ear";
            ret.PromptGroups = new List<PromptGroupDto>
            {

            }
                ;

            return ret;
        }
    }
}
