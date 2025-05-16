namespace ArtEFundAPIServices.DTO.Home;

public class DailyDonationDto
{
    public DateOnly Date { get; set; }
    public decimal Donations { get; set; }
}