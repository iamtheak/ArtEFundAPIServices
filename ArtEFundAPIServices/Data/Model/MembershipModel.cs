using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class MembershipModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MembershipId { get; set; }

    [Range(1, 4, ErrorMessage = "Membership tier must be between 1 and 4.")]
    public int MembershipTier { get; set; }

    public bool IsDeleted { get; set; } = false;
    public string MembershipName { get; set; }

    [ForeignKey("CreatorID")] public int CreatorId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MembershipAmount { get; set; }

    public string MembershipBenefits { get; set; }
    public CreatorModel Creator { get; set; }
    public ICollection<EnrolledMembershipModel> EnrolledMemberships { get; set; }
}