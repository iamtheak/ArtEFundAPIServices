using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class FollowModel
{
    
    [ForeignKey("FollowerId")]
    public int UserId { get; set; }
    
    [ForeignKey("CreatorId")]
    public int CreatorId { get; set; }
    
    public DateTime FollowDate { get; set; } = DateTime.Now;
    
    public CreatorModel Creator { get; set; }
    
    public UserModel User { get; set; }
    
}