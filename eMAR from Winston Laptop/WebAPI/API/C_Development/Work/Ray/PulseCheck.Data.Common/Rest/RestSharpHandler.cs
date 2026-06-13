using System;
using RestSharp;
using RestSharp.Authenticators;

namespace PulseCheck.Data.Common.Rest
{
    public class RestSharpHandler : IRestSharpHandler
    {
        private HttpBasicAuthenticator _authenticator;
        private RestSharp.RestClient _restClient;

        public RestSharpHandler(IRestConnection restConnection)
        {
            _restClient = new RestSharp.RestClient(restConnection.BaseUrl);
            _restClient.PreAuthenticate = false;
            _restClient.Timeout = Int32.MaxValue;

        }

        public RestSharpHandler(IBasicAuthRestConnection basicAuthRestConnection)
        {
            if (basicAuthRestConnection == null)
                throw new ArgumentNullException(nameof(basicAuthRestConnection));

            _authenticator = new HttpBasicAuthenticator(basicAuthRestConnection.UserName, basicAuthRestConnection.Password);
            _restClient = new RestSharp.RestClient(basicAuthRestConnection.BaseUrl);
            _restClient.Authenticator = _authenticator;
            _restClient.Timeout = Int32.MaxValue;
        }

        public string Get(string resource)
        {
            if (string.IsNullOrEmpty(resource))
                throw new ArgumentNullException(nameof(resource));

            RestRequest request = new RestRequest(resource, Method.GET);
            var response = _restClient.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            return response.Content;
        }

        public void Post(string resource, string body)
        {
            if (string.IsNullOrEmpty(resource))
                throw new ArgumentNullException(nameof(resource));

            if (string.IsNullOrEmpty(body))
                throw new ArgumentNullException(nameof(body));

            RestRequest request = new RestRequest(resource, Method.POST);
            request.AddBody(body);
            var response = _restClient.Execute(request);

            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException(response.Content);
            }
        }
    }
}
