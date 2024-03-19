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
        Task<RefreshToken> GenerateAndStoreRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task<bool> UpdateRefreshTokenAsync(RefreshToken refreshToken, IDbConnection dbConnection);
        Task StoreRefreshToken(string userId, string token, DateTime expiresAt);
        Task<RefreshToken> GetByTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string replacedByToken = null);

    }
}
