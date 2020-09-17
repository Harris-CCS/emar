using System;
using System.Collections.Generic;
using System.Text;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Templates.Model;
using Emar.Core.Templates.Model.Mappings;
using Emar.Core.Templates.Repository;
using Emar.Data.Entities;

namespace Emar.Core.Templates.Service
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _repository;


        public TemplateService(ITemplateRepository repository)
        {
            _repository = repository;
        }

        public TemplateDto GetTemplateDefinition(int templateId)
        {
            Template template = _repository.GetTemplate(templateId);

            return TemplateMapper.MapTemplate(template);
        }
    }
}
