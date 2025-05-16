namespace ArtEFundAPIServices.DataAccess.CreatorApiKey;

public interface ICreatorApiKeyInterface
{
    Task<string> GetApiKeyForCreator(int creatorId);
    Task SaveApiKeyForCreator(int creatorId, string apiKey);
    Task DeleteApiKeyForCreator(int creatorId);
    Task<bool> HasApiKey(int creatorId);
}