using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.Post;

public class PostCommentUpdateDto
{
    [Required] public string CommentText { get; set; } = string.Empty;
}