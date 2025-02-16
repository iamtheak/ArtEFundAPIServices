using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.ContentType;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.DataAccess.Creator;

public class CreatorRepo : ICreatorInterface
{
    private readonly ApplicationDbContext _context;
    private readonly IContentTypeInterface _contentTypeInterface;


    public CreatorRepo(ApplicationDbContext context, IContentTypeInterface contentTypeInterface)
    {
        _context = context;
        _contentTypeInterface = contentTypeInterface;
    }

    public async Task<List<CreatorModel>> GetCreators()
    {
        List<CreatorModel> creators =
            await _context.Creators.Include(c => c.ContentType).Include(c => c.UserModel).ToListAsync();

        return creators;
    }

    public async Task<CreatorModel?> GetCreatorById(int id)
    {
        CreatorModel creatorModel = await _context.Creators.FirstOrDefaultAsync(x => x.CreatorId == id);
        return creatorModel;
    }


    public async Task<CreatorModel?> GetCreatorByUserId(int id)
    {
        CreatorModel creatorModel =
            await _context.Creators.Include(cm => cm.UserModel).FirstOrDefaultAsync(x => x.UserId == id);
        return creatorModel;
    }

    public async Task<CreatorModel> CreateCreator(CreatorModel creator)
    {
        ContentTypeModel contentTypeModel = await _contentTypeInterface.GetContentTypeById(creator.ContentTypeId);
        if (contentTypeModel == null)
        {
            throw new Exception("Content Type not found");
        }

        await _context.Creators.AddAsync(creator);
        await _context.SaveChangesAsync();

        return creator;
    }

    public Task<CreatorModel> UpdateCreator(int id, CreatorModel creator)
    {
        throw new System.NotImplementedException();
    }

    public Task<bool> DeleteCreator(int id)
    {
        return Task.FromResult(true);
    }

    public async Task<CreatorModel?> GetCreatorByUserName(string userName)
    {
        return await _context.Creators
            .Include(c => c.UserModel) // Include the UserModel navigation property
            .FirstOrDefaultAsync(c => c.UserModel.UserName == userName);
    }

    public async Task<ContentTypeModel?> GetContentTypeById(int id)
    {
        return await _context.ContentTypes.FirstOrDefaultAsync(x => x.ContentTypeId == id);
    }

    public async Task<List<ContentTypeModel>> GetContentTypes()
    {
        return await _context.ContentTypes.ToListAsync();
    }
}