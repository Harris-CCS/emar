using System.Collections.Generic;

namespace Emar.Core.Templates.Model
{
    public class PromptGroupDto 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayTitle { get; set; }
        public IEnumerable<PromptDto> Prompts { get; set; }
    }
}