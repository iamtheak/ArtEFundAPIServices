using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.Membership;

public class MembershipCreateDto
{
    [Required] public int MembershipTier { get; set; }

    [Required] public string MembershipName { get; set; }

    [Required] public int CreatorId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MembershipAmount { get; set; }

    public string MembershipBenefits { get; set; }
}