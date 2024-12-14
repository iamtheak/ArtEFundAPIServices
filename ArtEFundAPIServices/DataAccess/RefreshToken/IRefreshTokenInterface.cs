using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.RefreshToken;

public interface IRefreshTokenInterface
{
    Task AddRefreshToken(RefreshTokenModel refreshToken);
    Task DeleteRefreshToken(int userId);
    Task DeleteRefreshToken(string token);
    Task RevokeRefreshToken(string token);
    Task RevokeRefreshToken(int userId);
    Task<RefreshTokenModel?> GetRefreshToken(string token);
    Task<UserModel?> GetUserFromRefreshToken(string token);
    
}