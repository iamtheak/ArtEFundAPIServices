namespace ArtEFundAPIServices.DTO.Membership;

public class MembershipViewDto
{
    public int MembershipId { get; set; }
    public int MembershipTier { get; set; }
    public string MembershipName { get; set; }
    public int CreatorId { get; set; }
    public decimal MembershipAmount { get; set; }
    public string MembershipBenefits { get; set; }

    public bool IsDeleted { get; set; }
}