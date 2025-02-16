using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.ContentType;

public interface IContentTypeInterface
{
    public Task<ContentTypeModel> GetContentTypeById(int id);
    public Task<List<ContentTypeModel>> GetContentTypes();
}