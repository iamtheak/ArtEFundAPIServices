namespace ArtEFundAPIServices.Data.Model;

public class EnrolledMembershipModel
{
    public int MembershipId { get; set; }
    
    public int UserId { get; set; }
    
    public DateTime EnrolledDate { get; set; } = DateTime.Now;
    
    public bool isActive { get; set; }
    public UserModel User { get; set; }
    
    public MembershipModel MembershipModel { get; set; }
}