using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.Membership;

public class MembershipUpdateDto
{
    [Required] public int MembershipId { get; set; }

    public int? MembershipTier { get; set; }

    public string? MembershipName { get; set; }

    [Range(0.01, double.MaxValue)] public decimal? MembershipAmount { get; set; }

    public string MembershipBenefits { get; set; }
}