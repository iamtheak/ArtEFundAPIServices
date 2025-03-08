using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.DataAccess.Membership;

public class MembershipRepository : IMembershipInterface
{
    private readonly ApplicationDbContext _context;

    public MembershipRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MembershipModel?> GetMembershipById(int id)
    {
        return await _context.Memberships
            .FirstOrDefaultAsync(m => m.MembershipId == id);
    }

    public async Task<List<MembershipModel>> GetMembershipsByCreatorId(int creatorId)
    {
        return await _context.Memberships
            .Where(m => m.CreatorId == creatorId)
            .ToListAsync();
    }

    public async Task<MembershipModel> CreateMembership(MembershipModel membership)
    {
        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync();
        return membership;
    }

    public async Task<MembershipModel?> UpdateMembership(MembershipModel membership)
    {
        _context.Entry(membership).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MembershipExists(membership.MembershipId))
            {
                return null;
            }

            throw;
        }

        return membership;
    }

    public async Task<bool> DeleteMembership(int id)
    {
        var membership = await _context.Memberships.FindAsync(id);

        if (membership == null)
        {
            return false;
        }

        _context.Memberships.Remove(membership);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<MembershipModel>> GetMembershipsByUserName(string userName)
    {
        return await _context.Memberships
            .Include(m => m.Creator)
            .ThenInclude(c => c.UserModel)
            .Where(m => m.Creator.UserModel.UserName == userName)
            .ToListAsync();
    }
    
    public async Task<EnrolledMembershipModel> EnrollMembership(EnrolledMembershipModel enrolledMembership)
    {
        _context.EnrolledMembershipModels.Add(enrolledMembership);
        await _context.SaveChangesAsync();
        return enrolledMembership;
    }
    
    public async Task<EnrolledMembershipModel> UpgradeMembership(EnrolledMembershipModel enrolledMembershipModel, int membershipId)
    {
        var currentEnrollment = await _context.EnrolledMembershipModels
            .FirstOrDefaultAsync(em => em.UserId == enrolledMembershipModel.UserId && em.IsActive);
    
        if (currentEnrollment != null)
        {
            currentEnrollment.IsActive = false;
            currentEnrollment.ExpiryDate = DateTime.UtcNow;
            _context.EnrolledMembershipModels.Update(currentEnrollment);
        }
    
        enrolledMembershipModel.MembershipId = membershipId;
        enrolledMembershipModel.EnrolledDate = DateTime.UtcNow;
        enrolledMembershipModel.IsActive = true;
    
        _context.EnrolledMembershipModels.Add(enrolledMembershipModel);
        await _context.SaveChangesAsync();
    
        return enrolledMembershipModel;
    }
    
    public async Task<EnrolledMembershipModel> DowngradeMembership(EnrolledMembershipModel enrolledMembershipModel, int membershipId)
    {
        return await UpgradeMembership(enrolledMembershipModel, membershipId);
    }
    
    public async Task<bool> EndMembership(EnrolledMembershipModel enrolledMembershipModel)
    {
        var currentEnrollment = await _context.EnrolledMembershipModels
            .FirstOrDefaultAsync(em => em.UserId == enrolledMembershipModel.UserId && em.IsActive);
    
        if (currentEnrollment != null)
        {
            currentEnrollment.IsActive = false;
            currentEnrollment.ExpiryDate = DateTime.UtcNow;
            _context.EnrolledMembershipModels.Update(currentEnrollment);
            await _context.SaveChangesAsync();
            return true;
        }
    
        return false;
    }

    private bool MembershipExists(int id)
    {
        return _context.Memberships.Any(m => m.MembershipId == id);
    }
    
}