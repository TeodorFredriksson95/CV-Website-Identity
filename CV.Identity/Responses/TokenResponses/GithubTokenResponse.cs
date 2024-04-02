using System.Text.Json.Serialization;

namespace CV.Identity.Responses.TokenResponses
{
    public class GithubTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("scope")]
        public int Scope { get; set; }
    }
}
