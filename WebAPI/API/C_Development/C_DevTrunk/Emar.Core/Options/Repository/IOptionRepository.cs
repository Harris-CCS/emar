namespace Emar.Core.Options.Repository
{
    public interface IOptionRepository
    {
        public string GetOption(int siteId, string optionName);
    }
}
