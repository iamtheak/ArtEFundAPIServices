using ArtEFundAPIServices.DTO.EnrolledMembership;

namespace ArtEFundAPIServices.DTO.Membership;

public class KhaltiMembershipVerifyDto
{
    public string KhaltiPaymentId { get; set; }
    public int MembershipId { get; set; }
    public int UserId { get; set; }

    public EnrollmentType Type { get; set; }
}