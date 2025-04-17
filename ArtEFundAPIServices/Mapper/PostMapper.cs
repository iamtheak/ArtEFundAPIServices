using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DTO.Post;

namespace ArtEFundAPIServices.Mapper;

public static class PostMapper
{
    public static PostModel ToModel(PostCreateDto dto, int creatorId)
    {
        return new PostModel
        {
            Title = dto.Title,
            Content = dto.Content,
            ImageUrl = dto.ImageUrl,
            IsMembersOnly = dto.IsMembersOnly,
            MembershipTier = dto.MembershipTier,
            CreatorId = creatorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateModelFromDto(PostModel model, PostUpdateDto dto)
    {
        model.Title = dto.Title;
        model.Content = dto.Content;
        model.ImageUrl = dto.ImageUrl;
        model.IsMembersOnly = dto.IsMembersOnly;
        model.MembershipTier = dto.MembershipTier;
        model.UpdatedAt = DateTime.UtcNow;
    }

    public static PostViewDto ToViewDto(PostModel model, int likesCount = 0, int commentsCount = 0)
    {
        return new PostViewDto
        {
            PostId = model.PostId,
            Title = model.Title,
            Content = model.Content,
            PostSlug = model.PostSlug,
            ImageUrl = model.ImageUrl ?? String.Empty,
            IsMembersOnly = model.IsMembersOnly,
            MembershipTier = model.MembershipTier,
            Views = model.Views,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            CreatorId = model.CreatorId,
            CreatorName = model.Creator?.UserModel.UserName ?? "",
            CreatorProfilePicture = model.Creator?.UserModel.ProfilePicture ?? "",
            LikesCount = likesCount,
            CommentsCount = commentsCount,
            IsDeleted = model.IsDeleted
        };
    }

    public static PostCommentModel ToModel(PostCommentCreateDto dto, int userId)
    {
        return new PostCommentModel
        {
            PostId = dto.PostId,
            UserId = userId,
            CommentText = dto.CommentText,
            CommentedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateModelFromDto(PostCommentModel model, PostCommentUpdateDto dto)
    {
        model.CommentText = dto.CommentText;
        model.UpdatedAt = DateTime.UtcNow;
    }

    public static PostCommentViewDto ToDto(PostCommentModel model)
    {
        return new PostCommentViewDto
        {
            CommentId = model.CommentId,
            PostId = model.PostId,
            UserId = model.UserId,
            UserName = model.User?.UserName ?? "",
            UserProfilePicture = model.User?.ProfilePicture ?? "",
            CommentText = model.CommentText,
            CommentedAt = model.CommentedAt,
            UpdatedAt = model.UpdatedAt
        };
    }

    public static PostLikeModel ToModel(PostLikeCreateDto dto, int userId)
    {
        return new PostLikeModel
        {
            PostId = dto.PostId,
            UserId = userId,
            LikedAt = DateTime.UtcNow
        };
    }

    public static PostLikeDto ToDto(PostLikeModel model)
    {
        return new PostLikeDto
        {
            LikeId = model.LikeId,
            PostId = model.PostId,
            UserId = model.UserId,
            UserName = model.User?.UserName ?? "",
            UserProfilePicture = model.User?.ProfilePicture ?? "",
            LikedAt = model.LikedAt
        };
    }
}