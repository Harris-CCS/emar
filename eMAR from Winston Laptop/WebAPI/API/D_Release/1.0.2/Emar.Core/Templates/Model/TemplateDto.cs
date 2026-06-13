using System;
using System.Collections.Generic;
using System.Text;
using Emar.Core.Helpers;

namespace Emar.Core.Templates.Model
{
    public class TemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Title { get; set; }
        public string SaveButtonText { get; set; }
        public string CancelButtonText { get; set; }
        public int? EventDatetimePromptId { get; set; }
        public IEnumerable<PromptGroupDto> PromptGroups { get; set; } = new List<PromptGroupDto>();
        public HateOasLinkDto Link { get; set; }
    }
}
