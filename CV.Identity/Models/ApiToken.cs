using Microsoft.AspNetCore.Http.HttpResults;

namespace CV.Identity.Models
{
    public class ApiToken
    {
        public int ApiKeyId { get; set; }
        public string ApiKey { get; set; }
        public  string UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsExpired => ExpiresAt > DateTime.UtcNow;
        public bool Revoked { get; set; }
        public string ReplacedByApiKey { get; set; }


    }
}