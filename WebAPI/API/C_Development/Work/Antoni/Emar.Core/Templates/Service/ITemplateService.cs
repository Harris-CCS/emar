using Emar.Core.Templates.Model;

namespace Emar.Core.Templates.Service
{
    public interface ITemplateService   
    {
        TemplateDto GetTemplateDefinition(int templateId);
    }
}