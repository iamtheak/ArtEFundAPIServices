using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class CreatorModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CreatorId { get; set; }

    [MaxLength(200)] public string? CreatorBio { get; set; }
    [MaxLength(300)] public string? CreatorDescription { get; set; }
    public string? CreatorBanner { get; set; } = "";
    public string? CreatorGoal { get; set; } = "";
    public int ProfileVisits { get; set; } = 0;
    [ForeignKey("ContentTypeId")] public int ContentTypeId { get; set; }
    [ForeignKey("UserId")] public int UserId { get; set; }
    public UserModel UserModel { get; set; }
    public ICollection<FollowModel> Followers { get; set; }
    public ICollection<DonationModel> Donations { get; set; }
    public ICollection<MembershipModel> Memberships { get; set; }
    public ICollection<GoalModel> Goals { get; set; }
    public ContentTypeModel ContentType { get; set; }
    public ICollection<PostModel> Posts { get; set; }

    public virtual CreatorApiKeyModel ApiKey { get; set; }
}