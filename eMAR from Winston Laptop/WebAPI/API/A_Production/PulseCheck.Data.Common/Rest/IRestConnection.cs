namespace PulseCheck.Data.Common.Rest
{
    public interface IRestConnection
    {
        string BaseUrl { get; set; }
        void ValidateUrl(string url);
    }
}