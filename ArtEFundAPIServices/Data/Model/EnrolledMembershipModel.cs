using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.Data.Model;

public class EnrolledMembershipModel
{
    [Key] public int EnrolledMembershipId { get; set; } // Primary Key
    public int UserId { get; set; }
    public int MembershipId { get; set; }
    public int PaymentId { get; set; }
    public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    [Range(0.01, double.MaxValue)] public decimal PaidAmount { get; set; }
    public UserModel User { get; set; }
    public MembershipModel Membership { get; set; }

    public PaymentModel Payment { get; set; }
}