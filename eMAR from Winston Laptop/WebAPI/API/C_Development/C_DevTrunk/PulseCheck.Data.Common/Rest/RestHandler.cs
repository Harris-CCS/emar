using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RestSharp.Authenticators;

namespace PulseCheck.Data.Common.Rest
{
    public class RestHandler : IRestHandler
    {
        private HttpBasicAuthenticator _authenticator;
        private IBasicAuthRestConnection _basicAuthRestConnection;
        private RestSharp.RestClient _restClient;

        public RestHandler(IBasicAuthRestConnection basicAuthRestConnection)
        {
            if (basicAuthRestConnection == null)
                throw new ArgumentNullException(nameof(basicAuthRestConnection));

            _basicAuthRestConnection = basicAuthRestConnection;

            _authenticator = new HttpBasicAuthenticator(_basicAuthRestConnection.UserName, _basicAuthRestConnection.Password);
            _restClient = new RestSharp.RestClient(_basicAuthRestConnection.BaseUrl);
        }

        public async Task<string> MakeHttpClientCall(string CallMethod, string CallValues, string CallUrl, string BearerAuthToken = null)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                if (BearerAuthToken != null)
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", string.Format("{0}", BearerAuthToken));
                client.DefaultRequestHeaders.Add("ApplicationKey", _basicAuthRestConnection.AppKey);
                client.DefaultRequestHeaders.Add("ApplicationSecret", _basicAuthRestConnection.Secret);

                var body = new StringContent(CallValues);
                body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var resp = new HttpResponseMessage();
                switch (CallMethod.ToUpper())
                {
                    case "GET":
                        resp = await client.GetAsync(CallUrl);
                        break;
                    case "POST":
                        resp = await client.PostAsync(CallUrl, body);
                        break;
                    case "PUT":
                        resp = await client.PutAsync(CallUrl, body);
                        break;
                    case "DELETE":
                        resp = await client.DeleteAsync(CallUrl);
                        break;
                    default:
                        throw new NotSupportedException("The REST method: '" + CallMethod + "' is not supported. Typical values are GET, PUT, POST, etc...");
                }

                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsStringAsync();
            }
        }

    }
}
