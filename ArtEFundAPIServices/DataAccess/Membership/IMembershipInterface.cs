using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.Membership;

public interface IMembershipInterface
{
    Task<MembershipModel?> GetMembershipById(int id);

    Task<List<MembershipModel>> GetMembershipsByCreatorId(int creatorId);
    
    Task<MembershipModel> CreateMembership(MembershipModel membership);
    
    Task<MembershipModel?> UpdateMembership(MembershipModel membership);
    
    Task<bool> DeleteMembership(int id);
    
    Task<List<MembershipModel>> GetMembershipsByUserName(string userName);
    
    Task<EnrolledMembershipModel> EnrollMembership(EnrolledMembershipModel enrolledMembership);

    Task<EnrolledMembershipModel> UpgradeMembership(EnrolledMembershipModel enrolledMembershipModel, int membershipId);

    Task<EnrolledMembershipModel>
        DowngradeMembership(EnrolledMembershipModel enrolledMembershipModel, int membershipId);

    Task<bool> EndMembership(EnrolledMembershipModel enrolledMembershipModel);
    
    Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByCreatorId(int creatorId);
    
    Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByUserName(string userName);
    
    Task<EnrolledMembershipModel?> GetEnrolledMembershipById(int id);
    
    Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByMembershipId(int membershipId);
    
    Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByMembershipIdAndCreatorId(int membershipId, int creatorId);
    
    Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByUserId(int userId);
    
    Task<EnrolledMembershipModel?> GetEnrolledMembershipByUserIdAndMembershipId(int userId, int membershipId);
    
}