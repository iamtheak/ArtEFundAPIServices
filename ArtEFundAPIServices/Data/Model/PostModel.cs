using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class PostModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PostId { get; set; }

    [Required] [MaxLength(21)] public string PostSlug { get; set; } // Unique slug used in URLs

    [Required] [MaxLength(200)] public string Title { get; set; }

    [Required] public string Content { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsMembersOnly { get; set; }

    public int? MembershipTier { get; set; }

    public int Views { get; set; } = 0;

    [ForeignKey("Creator")] public int CreatorId { get; set; }

    public CreatorModel Creator { get; set; }
    public ICollection<PostLikeModel> Likes { get; set; }
    public ICollection<PostCommentModel> Comments { get; set; }
}