using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.DataAccess.Donation;

public class DonationRepository : IDonationInterface
{
    private readonly ApplicationDbContext _context;

    public DonationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DonationModel?> GetDonationById(int id)
    {
        return await _context.Donations
            .Include(d => d.Creator)
            .ThenInclude(c => c.UserModel)
            .FirstOrDefaultAsync(d => d.DonationId == id);
    }

    public Task<List<DonationModel>> GetDonationsAsync()
    {
        return _context.Donations
            .Include(d => d.Creator)
            .ThenInclude(c => c.UserModel)
            .ToListAsync();
    }

    public async Task<List<DonationModel>> GetDonationsByCreatorId(int creatorId)
    {
        return await _context.Donations
            .Where(d => d.CreatorId == creatorId)
            .Include(d => d.Creator)
            .ThenInclude(c => c.UserModel)
            .ToListAsync();
    }

    public async Task<List<DonationModel>> GetDonationsByUserName(string userName)
    {
        return await _context.Donations
            .Include(d => d.Creator)
            .ThenInclude(c => c.UserModel)
            .Where(d => d.Creator.UserModel.UserName == userName)
            .ToListAsync();
    }

    public async Task<DonationModel?> CreateDonation(DonationModel donation)
    {
        await _context.Donations.AddAsync(donation);
        await _context.SaveChangesAsync();
        return donation;
    }

    public async Task<List<DonationModel>> GetDonationsByUserId(int userId)
    {
        return await _context.Donations
            .Include(d => d.Creator)
            .ThenInclude(c => c.UserModel)
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }

    public async Task<GoalModel> CreateDonationGoal(GoalModel donationGoal)
    {
        await _context.Goals.AddAsync(donationGoal);
        await _context.SaveChangesAsync();
        return donationGoal;
    }

    public async Task<GoalModel?> UpdateDonationGoal(GoalModel donationGoal)
    {
        _context.Goals.Update(donationGoal);
        await _context.SaveChangesAsync();
        return donationGoal;
    }

    public async Task<GoalModel?> DeleteDonationGoal(GoalModel donationGoal)
    {
        _context.Goals.Remove(donationGoal);
        await _context.SaveChangesAsync();
        return donationGoal;
    }

    public async Task<List<GoalModel>> GetGoalsByCreatorId(int creatorId)
    {
        return await _context.Goals
            .Where(g => g.CreatorId == creatorId)
            .ToListAsync();
    }

    public async Task<GoalModel?> GetDonationGoalById(int id)
    {
        return await _context.Goals
            .FirstOrDefaultAsync(g => g.GoalId == id);
    }
}