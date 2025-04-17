namespace ArtEFundAPIServices.DTO.Post;

public class PostLikeDto
{
    public int LikeId { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string UserProfilePicture { get; set; }
    public DateTime LikedAt { get; set; }
}