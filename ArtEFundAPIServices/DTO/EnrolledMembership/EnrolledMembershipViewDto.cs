namespace ArtEFundAPIServices.DTO.EnrolledMembership;

public class EnrolledMembershipViewDto
{
    public int EnrolledMembershipId { get; set; }
    public int UserId { get; set; }

    public string UserName { get; set; }
    public int MembershipId { get; set; }

    public int CreatorId { get; set; }

    public string CreatorUserName { get; set; }
    public string MembershipName { get; set; }
    public DateTime EnrolledDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public decimal PaidAmount { get; set; }
}