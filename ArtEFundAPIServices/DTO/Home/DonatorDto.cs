namespace ArtEFundAPIServices.DTO.Home;

public class DonatorDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? Name { get; set; }
    public string? Message { get; set; }
    public string? AvatarUrl { get; set; }
    public decimal Amount { get; set; }
}