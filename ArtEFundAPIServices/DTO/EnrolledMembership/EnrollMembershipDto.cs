namespace ArtEFundAPIServices.DTO.EnrolledMembership;

public class EnrollMembershipDto
{
    public int UserId { get; set; }
    public int MembershipId { get; set; }
}

public class ChangeMembershipDto
{
    public int EnrolledMembershipId { get; set; }
    public int MembershipId { get; set; }
    public int UserId { get; set; }
}