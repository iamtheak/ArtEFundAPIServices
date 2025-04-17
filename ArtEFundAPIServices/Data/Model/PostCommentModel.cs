using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.Data.Model;

public class PostCommentModel
{
    [Key] public int CommentId { get; set; }

    [Required] public int PostId { get; set; }

    public int UserId { get; set; }

    [Required] [MaxLength(500)] public string CommentText { get; set; }

    public DateTime CommentedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public PostModel Post { get; set; }

    public UserModel User { get; set; }
    public bool IsDeleted { get; set; } = false;
}