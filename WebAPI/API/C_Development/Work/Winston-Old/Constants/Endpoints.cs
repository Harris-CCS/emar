namespace PulseCheck.Constants
{
    public static class Endpoints
    {
        // Each of these is expected to be passed to string.Format() with the IdentityServerBaseAddress web.config key value 
        public const string AuthorizeEndpoint = "{0}/connect/authorize";
        public const string LogoutEndpoint = "{0}/connect/endsession";
        public const string TokenEndpoint = "{0}/connect/token";
        public const string UserInfoEndpoint = "{0}/connect/userinfo";
        public const string IdentityTokenValidationEndpoint = "{0}/connect/identitytokenvalidation";
        public const string TokenRevocationEndpoint = "{0}/connect/revocation";
    }
}
