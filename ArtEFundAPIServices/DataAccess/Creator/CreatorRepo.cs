using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.DataAccess.Creator;

public class CreatorRepo : ICreatorInterface
{
    private readonly ApplicationDbContext _context;

    public CreatorRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CreatorModel>> GetCreators()
    {
        List<CreatorModel> creators =
            await _context.Creators.Include(c => c.ContentType)
                .Include(c => c.ContentType)
                .Include(c => c.UserModel)
                .ThenInclude(c => c.RoleModel)
                .ToListAsync();

        return creators;
    }

    public async Task<CreatorModel?> GetCreatorById(int id)
    {
        CreatorModel? creatorModel = await _context.Creators
            .Include(cm => cm.ContentType)
            .Include(cm => cm.UserModel)
            .ThenInclude(um => um.RoleModel)
            .FirstOrDefaultAsync(x => x.CreatorId == id);
        return creatorModel;
    }

    public async Task<CreatorModel?> GetCreatorByUserId(int id)
    {
        CreatorModel? creatorModel =
            await _context.Creators.Include(cm => cm.UserModel)
                .Include(cm => cm.Goals)
                .FirstOrDefaultAsync(x => x.UserId == id);
        return creatorModel;
    }

    public async Task<CreatorModel> CreateCreator(CreatorModel creator)
    {
        ContentTypeModel? contentTypeModel = await GetContentTypeById(creator.ContentTypeId);
        if (contentTypeModel == null)
        {
            throw new Exception("Content Type not found");
        }

        await _context.Creators.AddAsync(creator);
        await _context.SaveChangesAsync();

        return creator;
    }


    public async Task<CreatorModel> UpdateCreator(CreatorModel existingCreator)
    {
        _context.Creators.Update(existingCreator);
        await _context.SaveChangesAsync();
        return existingCreator;
    }


    public Task<bool> DeleteCreator(int id)
    {
        return Task.FromResult(true);
    }

    public async Task<CreatorModel?> GetCreatorByUserName(string userName)
    {
        return await _context.Creators
            .Include(c => c.UserModel)
            .Include(c => c.Memberships)
            .Include(c => c.Posts)
            // Include the UserModel navigation property
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