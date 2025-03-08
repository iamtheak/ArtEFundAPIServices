using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DTO.User;

namespace ArtEFundAPIServices.DTO.Creator
{
    public class CreatorViewDto : UserViewDto
    {
        public int CreatorId { get; set; }
        public string? CreatorBio { get; set; }
        public string? CreatorDescription { get; set; }
        public string? CreatorBanner { get; set; }
        public string ContentType { get; set; } = "";
        public GoalModel? Goal { get; set; }

        public bool HasMembership { get; set; }

        public bool HasPosts { get; set; }
    }
}