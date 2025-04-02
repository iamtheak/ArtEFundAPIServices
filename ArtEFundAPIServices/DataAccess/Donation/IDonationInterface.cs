using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.Donation;

public interface IDonationInterface
{
    Task<DonationModel?> GetDonationById(int id);
    Task<List<DonationModel>> GetDonationsByCreatorId(int creatorId);
    Task<List<DonationModel>> GetDonationsByUserName(string userName);
    Task<DonationModel?> CreateDonation(DonationModel donation);
    Task<List<DonationModel>> GetDonationsByUserId(int userId);
    
}