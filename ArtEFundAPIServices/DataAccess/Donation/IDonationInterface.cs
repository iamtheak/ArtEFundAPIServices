using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.Donation;

public interface IDonationInterface
{
    Task<DonationModel?> GetDonationById(int id);
    Task<List<DonationModel>> GetDonationsByCreatorId(int creatorId);
    Task<List<DonationModel>> GetDonationsByUserName(string userName);
    Task<List<DonationModel>> GetDonationsAsync();
    Task<DonationModel?> CreateDonation(DonationModel donation);
    Task<List<DonationModel>> GetDonationsByUserId(int userId);
    Task<GoalModel> CreateDonationGoal(GoalModel donationGoal);
    Task<GoalModel?> UpdateDonationGoal(GoalModel donationGoal);
    Task<GoalModel?> DeleteDonationGoal(GoalModel donationGoal);
    Task<List<GoalModel>> GetGoalsByCreatorId(int creatorId);
    Task<GoalModel?> GetDonationGoalById(int id);
    
}