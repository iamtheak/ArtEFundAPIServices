namespace ArtEFundAPIServices.DTO.Donation;

public class DonationCreateDto
{
    public decimal DonationAmount { get; set; }
    public string? DonationMessage { get; set; }
    public int CreatorId { get; set; }
    public int? UserId { get; set; }
}