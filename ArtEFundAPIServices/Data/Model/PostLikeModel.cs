using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.Data.Model;

public class PostLikeModel
{
    [Key] public int LikeId { get; set; }

    [Required] 
    public int PostId { get; set; }

    public int UserId { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    public PostModel Post { get; set; }
    public UserModel User { get; set; }
}