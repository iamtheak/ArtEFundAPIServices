namespace ArtEFundAPIServices.DTO.Donation;

public class DonationViewDto
{
    public int DonationId { get; set; }
    public DateTime DonationDate { get; set; }
    public decimal DonationAmount { get; set; }
    public string? DonationMessage { get; set; }
    public int CreatorId { get; set; }
    public int? UserId { get; set; }
    
    public string userName { get; set; }
}