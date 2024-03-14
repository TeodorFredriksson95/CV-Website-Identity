namespace CV.Identity
{
    public static class ApiEndpoints
    {
        private const string ApiBase = "api";

        public static class OAuthProviders
        {
            public const string Base = $"{ApiBase}/authenticate";
            public const string Google = $"{Base}/google";
        }
    }
}
