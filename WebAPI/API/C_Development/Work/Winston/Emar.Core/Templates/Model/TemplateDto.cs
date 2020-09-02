using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.Templates.Model
{
    public class TemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<PromptGroupDto> PromptGroups { get; set; } = new List<PromptGroupDto>();
    }

    public class PromptGroupDto 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayTitle { get; set; }
        public List<PromptDto> PromptGroups { get; set; } = new List<PromptDto>();
    }

    public class PromptDto
    {
        public int Id { get; set; }
        public int PromptGroupId { get; set; }
        public int Sequence { get; set; }
        public string Prompt { get; set; }
        public string Type { get; set; }
        public string Default { get; set; }
        public bool Required { get; set; }
        public List<PromptChoiceDto> PromptGroups { get; set; } = new List<PromptChoiceDto>();

    }

    public class PromptChoiceDto
    {
        public int Id { get; set; }
        public int PromptId { get; set; }
        public int Sequence { get; set; }
        public string ChoiceText { get; set; }
    }
}
