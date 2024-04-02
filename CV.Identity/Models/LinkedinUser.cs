using System.Text.Json.Serialization;

namespace CV.Identity.Models
{
  public class LinkedInUser
        {
            [JsonPropertyName("sub")]
            public string Sub { get; set; }
            
            [JsonPropertyName("email_verified")]
            public bool VerifiedEmail { get; set; }            
            
            [JsonPropertyName("given_name")]
            public string Firstname { get; set; }

            

            [JsonPropertyName("family_name")]
            public string LastName { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }
            [JsonPropertyName("picture")]
            public string Picture { get; set; }

    }
}
