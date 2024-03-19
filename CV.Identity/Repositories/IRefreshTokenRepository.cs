using CV.Identity.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV.Identity.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<string> GenerateAndStoreRefreshToken(string userId, string refreshToken, DateTime expiryDate);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task<bool> UpdateRefreshTokenAsync(RefreshToken refreshToken, IDbConnection dbConnection);

    }
}
