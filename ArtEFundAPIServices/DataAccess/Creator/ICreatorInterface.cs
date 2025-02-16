using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.Creator;

public interface ICreatorInterface
{
    public Task<CreatorModel?> GetCreatorById(int id);
    public Task<CreatorModel?> GetCreatorByUserId(int id);
    public Task<CreatorModel?> GetCreatorByUserName(string userName);
    public Task<List<CreatorModel>> GetCreators();
    public Task<CreatorModel> CreateCreator(CreatorModel creator);
    public Task<CreatorModel> UpdateCreator(int id, CreatorModel creator);
    public Task<bool> DeleteCreator(int id);
    public Task<ContentTypeModel?> GetContentTypeById(int id);
    public Task<List<ContentTypeModel>> GetContentTypes();
}