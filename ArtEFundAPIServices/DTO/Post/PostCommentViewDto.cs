namespace ArtEFundAPIServices.DTO.Post;

public class PostCommentViewDto
{
    public int CommentId { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string UserProfilePicture { get; set; }
    public string CommentText { get; set; }
    public DateTime CommentedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}