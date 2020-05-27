using RestSharp;

namespace PulseCheck.Data.Common.Rest
{
    public interface IRestSharpHandler : IRestHandler
    {
        string Get(string resource);
        void Post(string resource, string body);
    }
}
