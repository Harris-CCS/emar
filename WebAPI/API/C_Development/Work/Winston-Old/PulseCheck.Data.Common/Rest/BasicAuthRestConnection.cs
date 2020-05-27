namespace PulseCheck.Data.Common.Rest
{
    public class BasicAuthRestConnection : RestConnection, IBasicAuthRestConnection
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AppKey { get; set; }
        public string Secret { get; set; }
    }
}
