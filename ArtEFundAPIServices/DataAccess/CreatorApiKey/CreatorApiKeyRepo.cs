using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.DataAccess.CreatorApiKey;

public class CreatorApiKeyRepo : ICreatorApiKeyInterface
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;

    public CreatorApiKeyRepo(ApplicationDbContext context, IEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
    }

    public async Task<string> GetApiKeyForCreator(int creatorId)
    {
        var apiKey = await _context.CreatorApiKeys
            .FirstOrDefaultAsync(k => k.CreatorId == creatorId);

        if (apiKey == null)
            return null;

        return _encryptionService.Decrypt(apiKey.EncryptedApiKey);
    }

    public async Task SaveApiKeyForCreator(int creatorId, string apiKey)
    {
        var existingKey = await _context.CreatorApiKeys
            .FirstOrDefaultAsync(k => k.CreatorId == creatorId);

        if (existingKey == null)
        {
            existingKey = new CreatorApiKeyModel
            {
                CreatorId = creatorId,
                EncryptedApiKey = _encryptionService.Encrypt(apiKey)
            };
            _context.CreatorApiKeys.Add(existingKey);
        }
        else
        {
            existingKey.EncryptedApiKey = _encryptionService.Encrypt(apiKey);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteApiKeyForCreator(int creatorId)
    {
        var apiKey = await _context.CreatorApiKeys
            .FirstOrDefaultAsync(k => k.CreatorId == creatorId);

        if (apiKey != null)
        {
            _context.CreatorApiKeys.Remove(apiKey);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasApiKey(int creatorId)
    {
        return await _context.CreatorApiKeys.AnyAsync(k => k.CreatorId == creatorId);
    }
}