using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.DataAccess.User;

public class UserRepo : IUserInterface
{
    private readonly ApplicationDbContext _context;

    public UserRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserModel>> GetAllUsers()
    {
        return await _context.Users.Include(u => u.RoleModel).ToListAsync();
    }

    public async Task<UserModel?> GetUserById(int id)
    {
        return await _context.Users
            .Include(u => u.RoleModel)
            .Include(u => u.UserType)
            .SingleOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<UserModel?> GetUserByEmail(string email)
    {
        return await _context.Users
            .Include(u => u.RoleModel)
            .SingleOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UserModel?> GetUserByUserName(string userName)
    {
        return await _context.Users
            .Include(u => u.RoleModel)
            .SingleOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task<UserModel> AddUser(UserModel user, int userTypeId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var roleTask = _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == 2);
            var userTypeTask = _context.UserTypes
                .FirstOrDefaultAsync(u => u.UserTypeId == userTypeId);

            await Task.WhenAll(roleTask, userTypeTask);

            var defaultRole = await roleTask;
            var defaultUserType = await userTypeTask;

            if (defaultRole == null)
                throw new Exception("Role not found");

            if (defaultUserType == null)
                throw new Exception($"User type with id {userTypeId} not found");

            user.RoleId = defaultRole.RoleId;
            user.UserTypeId = defaultUserType.UserTypeId;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return user;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Failed to create user");
        }
    }

    public async Task DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateUser(UserModel user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UserType>> GetUserTypes()
    {
        return await _context.UserTypes.ToListAsync();
    }

    public async Task<List<RoleModel>> GetRoles()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<RoleModel?> GetRoleById(int id)
    {
        return await _context.Roles.FindAsync(id);
    }
}