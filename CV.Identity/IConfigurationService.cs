using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using CV.Identity.Models;
using System.Data;

namespace CV.Identity
{
    public interface IConfigurationService
    {
        string GetJwtSecretKey();
        string GetJwtConfigIssuer();
        string GetJwtConfigAudience();
        string GetJwtApiIssuer();
        string GetJwtApiAudience();


    }
}

