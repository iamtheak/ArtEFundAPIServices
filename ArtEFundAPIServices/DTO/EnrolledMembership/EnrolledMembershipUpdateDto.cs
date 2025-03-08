namespace ArtEFundAPIServices.DTO.EnrolledMembership;

public class EnrolledMembershipUpdateDto
{
    public int EnrolledMembershipId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public decimal PaidAmount { get; set; }
}