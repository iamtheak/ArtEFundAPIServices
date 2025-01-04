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

    public async Task<UserModel> AddUser(UserModel user)
    {
        
        _context.Database.BeginTransaction();

        try
        {
            // Ensure RoleModel exists
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == 2);
            if (defaultRole == null)
            {
                await _context.Database.RollbackTransactionAsync();
                throw new Exception("Role not found.");
            }
            
            user.RoleId = defaultRole.RoleId;
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        
            await _context.Database.CommitTransactionAsync();
            return user;
        }
        catch (Exception e)
        {
            await _context.Database.RollbackTransactionAsync();
            throw new Exception("Error adding user.");
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
}