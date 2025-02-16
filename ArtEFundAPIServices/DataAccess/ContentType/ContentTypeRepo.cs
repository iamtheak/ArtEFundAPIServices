using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.ContentType;

public class ContentTypeRepo : IContentTypeInterface
{
    private readonly ApplicationDbContext _context;

    public ContentTypeRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ContentTypeModel> GetContentTypeById(int id)
    {
        return Task.FromResult(_context.ContentTypes.FirstOrDefault(x => x.ContentTypeId == id));
    }
    public Task<List<ContentTypeModel>> GetContentTypes()
    {
        return Task.FromResult(_context.ContentTypes.ToList());
    }
}