namespace ArtEFundAPIServices.DTO.Post;

public class PostViewDto
{
    public int PostId { get; set; }
    public string PostSlug { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string ImageUrl { get; set; }
    public bool IsMembersOnly { get; set; }
    public int? MembershipTier { get; set; }
    public int Views { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CreatorId { get; set; }
    public string CreatorName { get; set; }
    public string CreatorProfilePicture { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }

    public bool IsDeleted { get; set; }
}