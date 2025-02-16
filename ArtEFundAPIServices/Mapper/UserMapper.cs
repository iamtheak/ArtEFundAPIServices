using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DTO.Auth;
using ArtEFundAPIServices.DTO.User;

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
            PasswordHash = userDto.Password,
            ProfilePicture = userDto.ProfilePicture
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
            Password = userModel.PasswordHash,
            ProfilePicture = userModel.ProfilePicture
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
            Role = userModel.RoleModel.RoleName,
            ProfilePicture = userModel.ProfilePicture
        };
    }
    
    public static UserModel ToUserModel(UserUpdateDto userDto)
    {
        return new UserModel()
        {
            UserName = userDto.UserName,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            ProfilePicture = userDto.ProfilePicture
        };
    }
}