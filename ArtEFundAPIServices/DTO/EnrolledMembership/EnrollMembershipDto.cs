namespace ArtEFundAPIServices.DTO.EnrolledMembership;

public enum EnrollmentType
{
    New,
    Upgrade,
}

public class EnrollMembershipDto
{
    public int UserId { get; set; }
    public int MembershipId { get; set; }
    public EnrollmentType Type { get; set; }
}

public class ChangeMembershipDto
{
    public int EnrolledMembershipId { get; set; }
    public int MembershipId { get; set; }
    public int UserId { get; set; }
}