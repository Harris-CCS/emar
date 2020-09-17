using System.Collections.Generic;

namespace Emar.Core.Templates.Model
{
    public class PromptDto
    {
        public int Id { get; set; }
        public int PromptGroupId { get; set; }
        public int Sequence { get; set; }
        public string Prompt { get; set; }
        public string Type { get; set; }
        public string Default { get; set; }
        public bool Required { get; set; }
        public IEnumerable<PromptChoiceDto> PromptChoices { get; set; } = new List<PromptChoiceDto>();
        public bool IsActive { get; set; }
    }
}