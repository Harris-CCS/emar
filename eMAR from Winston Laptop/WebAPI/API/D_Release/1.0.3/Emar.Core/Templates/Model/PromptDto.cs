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
        public bool IsActive { get; set; }
        public bool IsOnNewline { get; set; }
        public string PlaceholderText { get; set; }
        public string DisplayChildPromptsValue { get; set; }
        public IEnumerable<int> PromptChildren { get; set; }
        public IEnumerable<PromptChoiceDto> PromptChoices { get; set; }
        public string ChartMarkup { get; set; }
    }
}