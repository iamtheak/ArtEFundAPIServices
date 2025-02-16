using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.DataAccess.RefreshToken;

public class RefreshTokenRepo : IRefreshTokenInterface
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddRefreshToken(RefreshTokenModel refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRefreshToken(int userId)
    {
        var tokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId).ToListAsync();
        _context.RefreshTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRefreshToken(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null)
        {
            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeRefreshToken(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeRefreshToken(int userId)
    {
        var tokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId).ToListAsync();
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        _context.RefreshTokens.UpdateRange(tokens);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshTokenModel?> GetRefreshToken(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<UserModel?> GetUserFromRefreshToken(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null)
        {
            return await _context.Users.Include(u => u.RoleModel)
                .FirstOrDefaultAsync(u => u.UserId == refreshToken.UserId);
        }

        return null;
    }
}