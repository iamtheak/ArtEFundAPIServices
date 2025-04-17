namespace ArtEFundAPIServices.DTO.Post;

public class PostCommentCreateDto
{
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string CommentText { get; set; }
}