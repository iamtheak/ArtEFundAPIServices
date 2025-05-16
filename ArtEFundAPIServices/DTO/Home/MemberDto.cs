namespace ArtEFundAPIServices.DTO.Home;

public class MemberDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? AvatarUrl { get; set; }
    public int MembershipTier { get; set; }
    public string MembershipName { get; set; }
    public DateTime JoinDate { get; set; }
}