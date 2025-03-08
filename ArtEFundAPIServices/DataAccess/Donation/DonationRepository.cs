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
            .FirstOrDefaultAsync(d => d.DonationId == id);
    }

    public async Task<List<DonationModel>> GetDonationsByCreatorId(int creatorId)
    {
        return await _context.Donations
            .Where(d => d.CreatorId == creatorId)
            .Include(d => d.Creator)
            .ToListAsync();
    }

    public async Task<List<DonationModel>> GetDonationsByUserName(string userName)
    {
        return await _context.Donations
            .Include(d => d.Creator)
            .Include(d => d.Creator.UserModel)
            .Where(d => d.Creator.UserModel.UserName == userName)
            .ToListAsync();
    }

    public async Task<DonationModel?> CreateDonation(DonationModel donation)
    {
        await _context.Donations.AddAsync(donation);
        await _context.SaveChangesAsync();
        return donation;
    }
}