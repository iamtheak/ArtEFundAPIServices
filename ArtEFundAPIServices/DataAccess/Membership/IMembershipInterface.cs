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

}