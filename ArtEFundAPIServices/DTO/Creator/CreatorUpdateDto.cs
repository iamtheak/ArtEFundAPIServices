namespace ArtEFundAPIServices.DTO.Creator;

public class CreatorUpdateDto
{
    public string? CreatorBio { get; set; }
    public string? CreatorDescription { get; set; }
    public string? CreatorBanner { get; set; }
    public int ContentTypeId { get; set; }
}