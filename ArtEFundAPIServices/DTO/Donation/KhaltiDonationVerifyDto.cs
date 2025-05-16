namespace ArtEFundAPIServices.DTO;

public class KhaltiDonationVerifyDto
{
    public string KhaltiPaymentId { get; set; }
    public int CreatorId { get; set; }
    public int? UserId { get; set; }
    public string? Message { get; set; }
}