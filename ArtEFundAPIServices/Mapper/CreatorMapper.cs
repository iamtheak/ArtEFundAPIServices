using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DTO.Creator;

namespace ArtEFundAPIServices.Mapper;

public static class CreatorMapper
{
    public static CreatorModel ToCreatorModel(CreatorCreateDto createDto)
    {
        return new CreatorModel()
        {
            ContentTypeId = createDto.ContentTypeId,
            CreatorBio = createDto.Bio,
            CreatorDescription = createDto.Description,
            UserId = createDto.UserId
        };
    }

    public static CreatorModel ToCreatorModel(CreatorUpdateDto creatorUpdateDto)
    {
        return new CreatorModel()
        {
            ContentTypeId = creatorUpdateDto.ContentTypeId,
            CreatorBio = creatorUpdateDto.CreatorBio,
            CreatorDescription = creatorUpdateDto.CreatorDescription,
            CreatorBanner = creatorUpdateDto.CreatorBanner,
        };
    }


    public static CreatorViewDto ToCreatorViewDto(CreatorModel creator)
    {
        return new CreatorViewDto()
        {
            CreatorBanner = creator.CreatorBanner,
            CreatorBio = creator.CreatorBio,
            CreatorDescription = creator.CreatorDescription,
            CreatorId = creator.CreatorId,
            ContentType = creator.ContentType?.ContentTypeId ?? 0, // Handle potential null
            Email = creator.UserModel?.Email, // Handle potential null
            FirstName = creator.UserModel?.FirstName,   
            LastName = creator.UserModel?.LastName,
            UserId = creator.UserId,
            UserName = creator.UserModel?.UserName,
            ProfilePicture = creator.UserModel?.ProfilePicture,
            Role = creator.UserModel?.RoleModel?.RoleName, // Handle potential null
            Goal = creator.Goals?.FirstOrDefault(g => g.IsGoalActive == true), // Handle potential null
            HasMembership = creator.Memberships?.Any() ?? false,
            HasPosts = creator.Posts?.Any() ?? false
        };
    }
}