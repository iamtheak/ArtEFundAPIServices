using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DTO.Auth;

namespace ArtEFundAPIServices.Mapper;

public static class UserMapper
{
    public static UserModel ToUserModel(UserCreateDto userDto)
    {
        return new UserModel()
        {
            UserName = userDto.UserName,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            PasswordHash = userDto.Password
        };
    }

    public static UserCreateDto ToUserCreateDto(UserModel userModel)
    {
        return new UserCreateDto()
        {
            UserName = userModel.UserName,
            Email = userModel.Email,
            FirstName = userModel.FirstName,
            LastName = userModel.LastName,
            Password = userModel.PasswordHash 
        };
    }

    public static UserViewDto ToUserViewDto(UserModel userModel)
    {
        return new UserViewDto()
        {
            UserId = userModel.UserId,
            UserName = userModel.UserName,
            Email = userModel.Email,
            FirstName = userModel.FirstName,
            LastName = userModel.LastName,
            Roles = userModel.UserRoles.Any() ? userModel.UserRoles.Select(x => x.RoleModel.RoleName).ToArray() : []
        };
    }
}