using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.User;

public interface IUserInterface
{
    Task<List<UserModel>> GetAllUsers();
    Task<UserModel?> GetUserById(int id);
    Task<UserModel?> GetUserByEmail(string email);
    Task<UserModel?> GetUserByUserName(string userName);
    Task<UserModel> AddUser(UserModel user);
    Task DeleteUser(int id);
    Task UpdateUser(UserModel user);
}