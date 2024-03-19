using System.Security.Claims;

namespace CV.Identity.Models
{
    public class RefreshTokenValidationResult
    {
        public bool IsValid { get; set; }
        public string UserId { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
