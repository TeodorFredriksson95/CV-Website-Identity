using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CV.Identity.Models
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }

        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Revoked { get; set; } = false;
        public string ReplacedByToken { get; set; } = null; 
        public string UserId { get; set; }
    }

}
