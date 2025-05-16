using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.User;

public interface IUserInterface
{
    Task<List<UserModel>> GetAllUsers();
    Task<UserModel?> GetUserById(int id);
    Task<UserModel?> GetUserByEmail(string email);
    Task<UserModel?> GetUserByUserName(string userName);
    Task<UserModel> AddUser(UserModel user, int userTypeId);
    Task DeleteUser(int id);
    Task UpdateUser(UserModel user);
    Task<List<UserType>> GetUserTypes();
    Task<List<RoleModel>> GetRoles();
    Task<RoleModel?> GetRoleById(int id);
    Task<UserModel?> GetUserByVerificationToken(string token);
}