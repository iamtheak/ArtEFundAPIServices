namespace ArtEFundAPIServices.DTO.Home;

public class TopEarnerDto
{
    public string Name { get; set; }
    public string ProfilePicture { get; set; }
    public decimal TotalEarnings { get; set; }
    
    public int CreatorId { get; set; }
}