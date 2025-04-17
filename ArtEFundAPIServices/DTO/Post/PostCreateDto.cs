using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.Post;

public class PostCreateDto
{
    [Required] [MaxLength(50)] public string Title { get; set; }
    [Required] public string Content { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsMembersOnly { get; set; }
    public int? MembershipTier { get; set; }
}