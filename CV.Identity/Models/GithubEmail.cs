using System.Text.Json.Serialization;

namespace CV.Identity.Models
{
    public class GithubEmail
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("primary")]
        public bool Primary { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        // If you decide to use the visibility field later on
        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }
    }
}
