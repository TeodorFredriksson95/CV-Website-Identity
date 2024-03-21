using CV.Identity.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV.Identity.Repositories.RefreshTokenRepo
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GenerateAndStoreRefreshToken(RefreshToken refreshToken);
        Task StoreRefreshToken(string userId, string token, DateTime expiresAt);
        Task<RefreshToken> GetByTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string replacedByToken = null);

    }
}
