namespace Emar.Core.Options.Repository
{
    public interface IOptionRepository
    {
        public string GetOption(long siteId, string optionName);
    }
}
