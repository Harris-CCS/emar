namespace PulseCheck.Data.Common.Rest
{
    public interface IBasicAuthRestConnection : IRestConnection
    {
        string Password { get; set; }
        string UserName { get; set; }
        string AppKey { get; set; }
        string Secret { get; set; }

    }
}