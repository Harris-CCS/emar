namespace Emar.Core.Options.Service
{
    public interface IOptionService
    {
        public string GetOption(long siteId, string optionName);
    }
}
