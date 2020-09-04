namespace Emar.Core.Templates.Model
{
    public class PromptChoiceDto
    {
        public int Id { get; set; }
        public int PromptId { get; set; }
        public int Sequence { get; set; }
        public string ChoiceText { get; set; }
    }
}