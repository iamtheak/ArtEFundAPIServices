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
        _context.EnrolledMembership.Add(enrolledMembership);
        await _context.SaveChangesAsync();
        return enrolledMembership;
    }

    public async Task<EnrolledMembershipModel> UpgradeMembership(EnrolledMembershipModel enrolledMembershipModel,
        int membershipId)
    {
        var currentEnrollment = await _context.EnrolledMembership
            .FirstOrDefaultAsync(em =>
                em.UserId == enrolledMembershipModel.UserId && em.IsActive &&
                (em.MembershipId == enrolledMembershipModel.MembershipId));

        if (currentEnrollment != null)
        {
            currentEnrollment.IsActive = false;
            currentEnrollment.ExpiryDate = DateTime.UtcNow;
            _context.EnrolledMembership.Update(currentEnrollment);
        }

        enrolledMembershipModel.MembershipId = membershipId;
        enrolledMembershipModel.EnrolledDate = DateTime.UtcNow;
        enrolledMembershipModel.IsActive = true;
        enrolledMembershipModel.ExpiryDate = DateTime.Now.AddMonths(1);

        _context.EnrolledMembership.Add(enrolledMembershipModel);
        await _context.SaveChangesAsync();

        return enrolledMembershipModel;
    }

    public async Task<EnrolledMembershipModel> DowngradeMembership(EnrolledMembershipModel enrolledMembershipModel,
        int membershipId)
    {
        return await UpgradeMembership(enrolledMembershipModel, membershipId);
    }

    public async Task<bool> EndMembership(EnrolledMembershipModel enrolledMembershipModel)
    {
        var currentEnrollment = await _context.EnrolledMembership
            .FirstOrDefaultAsync(em => em.UserId == enrolledMembershipModel.UserId && em.IsActive);

        if (currentEnrollment != null)
        {
            currentEnrollment.IsActive = false;
            currentEnrollment.ExpiryDate = DateTime.UtcNow;
            _context.EnrolledMembership.Update(currentEnrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByCreatorId(int creatorId)
    {
        return await _context.EnrolledMembership
            .Include(em => em.Membership)
            .Where(em => em.Membership.CreatorId == creatorId)
            .Include(em => em.User)
            .ToListAsync();
    }

    public async Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByUserName(string userName)
    {
        return await _context.EnrolledMembership
            .Include(em => em.User)
            .Where(em => em.User.UserName == userName)
            .Include(em => em.Membership)
            .ToListAsync();
    }

    public async Task<EnrolledMembershipModel?> GetEnrolledMembershipById(int id)
    {
        return await _context.EnrolledMembership
            .Include(em => em.User)
            .Include(em => em.Membership)
            .FirstOrDefaultAsync(em => em.EnrolledMembershipId == id);
    }

    public async Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByMembershipId(int membershipId)
    {
        return await _context.EnrolledMembership
            .Where(em => em.MembershipId == membershipId)
            .Include(em => em.User)
            .Include(em => em.Membership)
            .ToListAsync();
    }

    public async Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByMembershipIdAndCreatorId(int membershipId,
        int creatorId)
    {
        return await _context.EnrolledMembership
            .Include(em => em.Membership)
            .Where(em => em.MembershipId == membershipId && em.Membership.CreatorId == creatorId)
            .Include(em => em.User)
            .ToListAsync();
    }

    public async Task<List<EnrolledMembershipModel>> GetEnrolledMembershipsByUserId(int userId)
    {
        return await _context.EnrolledMembership
            .Where(em => em.UserId == userId)
            .Include(em => em.Membership)
            .Include(em => em.User)
            .ToListAsync();
    }

    public async Task<EnrolledMembershipModel?> GetEnrolledMembershipByUserIdAndMembershipId(int userId,
        int membershipId)
    {
        return await _context.EnrolledMembership
            .Include(em => em.Membership)
            .Include(em => em.User)
            .FirstOrDefaultAsync(em => em.UserId == userId && em.MembershipId == membershipId);
    }

    private bool MembershipExists(int id)
    {
        return _context.Memberships.Any(m => m.MembershipId == id);
    }
}