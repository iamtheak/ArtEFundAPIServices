using ArtEFundAPIServices.DTO.User;

namespace ArtEFundAPIServices.DTO.Creator
{
    public class CreatorViewDto : UserViewDto
    {
        public int CreatorId { get; set; }
        public string? CreatorBio { get; set; }
        public string? CreatorDescription { get; set; }
        public string? CreatorAvatar { get; set; }
        public string? CreatorBanner { get; set; }
        public string? CreatorGoal { get; set; }
        public string ContentType { get; set; }
    }
}