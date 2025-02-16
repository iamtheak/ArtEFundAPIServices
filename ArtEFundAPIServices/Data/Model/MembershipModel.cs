using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class MembershipModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MembershipId { get; set; }

    public int MembershipTier { get; set; }

    public int MembershipName { get; set; }

    [ForeignKey("CreatorID")] public int CreatorId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MembershipAmount { get; set; }

    public string MembershipBenifits { get; set; }
    public CreatorModel Creator { get; set; }
    public ICollection<EnrolledMembershipModel> EnrolledMemberships { get; set; }
}