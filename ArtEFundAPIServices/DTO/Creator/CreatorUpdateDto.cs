namespace ArtEFundAPIServices.DTO.Creator;

public class CreatorUpdateDto
{
    public int CreatorId { get; set; }
    public string? CreatorBio { get; set; }
    public string? CreatorDescription { get; set; }
    public string? CreatorBanner { get; set; }
    public string? CreatorGoal { get; set; }
    public int ContentTypeId { get; set; }
}