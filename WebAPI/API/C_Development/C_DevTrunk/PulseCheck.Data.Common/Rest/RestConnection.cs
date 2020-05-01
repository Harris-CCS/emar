using System;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using PulseCheck.Data.Common.RegEx;

namespace PulseCheck.Data.Common.Rest
{
    public class RestConnection : IRestConnection
    {
        private string _baseUrl;

        public string BaseUrl
        {
            get { return _baseUrl; }

            set {
                ValidateUrl(value);
                _baseUrl = value;
            }
        }

        public RestConnection()
        { }

        public RestConnection(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));

            BaseUrl = baseUrl;
        }

        public void ValidateUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            if (!Regex.IsMatch(url, RegExHttp.ValidUrl))
                throw new ArgumentOutOfRangeException($"Invalid URL: {url}");
        }
    }
}
