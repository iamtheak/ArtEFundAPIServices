namespace ArtEFundAPIServices.DTO.Creator;

public class FollowerDto
{
    public int CreatorId { get; set; }
    public int UserId { get; set; }
    public string? FollowerUserName { get; set; }
    public string? FollowerAvatarUrl { get; set; }
    public string? FollowingUserName { get; set; }
    public string? FollowingAvatarUrl { get; set; }
}