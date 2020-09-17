using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.Templates.Model
{
    public class TemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<PromptGroupDto> PromptGroups { get; set; } = new List<PromptGroupDto>();
        public bool Active { get; set; }
        public string Title { get; set; }
        public int SiteId { get; set; }
    }
}
