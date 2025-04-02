using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.Creator;
using ArtEFundAPIServices.DataAccess.Membership;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO.EnrolledMembership;
using ArtEFundAPIServices.DTO.Membership;
using ArtEFundAPIServices.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/[controller]")]
public class MembershipController : ControllerBase
{
    private readonly IMembershipInterface _membershipRepository;
    private readonly IUserInterface _userRepository;
    private readonly ICreatorInterface _creatorRepository;

    public MembershipController(IMembershipInterface membershipRepository, IUserInterface userRepository,
        ICreatorInterface creatorRepository)
    {
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _creatorRepository = creatorRepository;
    }

    // GET: api/Membership/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipViewDto>> GetMembership(int id)
    {
        var membership = await _membershipRepository.GetMembershipById(id);

        if (membership == null)
        {
            return NotFound();
        }

        return MembershipMapper.MapToViewDto(membership);
    }

    // GET: api/Membership/creator/5
    [HttpGet("creator/{creatorId}")]
    public async Task<ActionResult<IEnumerable<MembershipViewDto>>> GetMembershipsByCreator(int creatorId)
    {
        var memberships = await _membershipRepository.GetMembershipsByCreatorId(creatorId);

        if (memberships == null || memberships.Count == 0)
        {
            return NotFound();
        }

        return memberships.Select(m => MembershipMapper.MapToViewDto(m)).ToList();
    }


    [HttpGet("creator/userName/{userName}")]
    public async Task<ActionResult<IEnumerable<MembershipViewDto>>> GetMembershipsByCreator(string userName)
    {
        var memberships = await _membershipRepository.GetMembershipsByUserName(userName);

        if (memberships == null || memberships.Count == 0)
        {
            return NotFound();
        }

        return memberships.Select(m => MembershipMapper.MapToViewDto(m)).ToList();
    }

    // POST: api/Membership
    [HttpPost]
    public async Task<ActionResult<MembershipViewDto>> CreateMembership(MembershipCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var creator = await _creatorRepository.GetCreatorById(createDto.CreatorId);

        if (creator == null)
        {
            return NotFound(new { message = "Creator not found" });
        }

        var membershipModel = MembershipMapper.MapToModel(createDto);

        var createdMembership = await _membershipRepository.CreateMembership(membershipModel);

        return CreatedAtAction(
            nameof(GetMembership),
            new { id = createdMembership.MembershipId },
            MembershipMapper.MapToViewDto(createdMembership));
    }

    // PUT: api/Membership/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMembership(int id, MembershipUpdateDto updateDto)
    {
        if (id != updateDto.MembershipId)
        {
            return BadRequest("ID in the URL does not match ID in the request body");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingMembership = await _membershipRepository.GetMembershipById(id);
        if (existingMembership == null)
        {
            return NotFound();
        }

        // Apply updates from DTO to existing entity
        MembershipMapper.ApplyUpdates(updateDto, existingMembership);

        var updatedMembership = await _membershipRepository.UpdateMembership(existingMembership);

        if (updatedMembership == null)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/Membership/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMembership(int id)
    {
        var result = await _membershipRepository.DeleteMembership(id);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("enroll")]
    public async Task<ActionResult<EnrolledMembershipModel>> EnrollMembership(
        [FromBody] EnrollMembershipDto enrollMembership)
    {
        var userId = enrollMembership.UserId;
        var membershipId = enrollMembership.MembershipId;

        var membership = await _membershipRepository.GetMembershipById(membershipId);
        if (membership == null)
        {
            return NotFound(new { message = "Membership not found" });
        }

        var user = await _userRepository.GetUserById(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var creatorMembers = await _membershipRepository.GetEnrolledMembershipsByCreatorId(membership.CreatorId);
        var isAlreadyEnrolled = creatorMembers.Any(em => em.UserId == userId);
        if (isAlreadyEnrolled)
        {
            return BadRequest(new { message = "User already enrolled in a membership of the creator" });
        }

        var enrolledMembership = new EnrolledMembershipModel
        {
            UserId = userId,
            MembershipId = membershipId,
            EnrolledDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddMonths(1),
            IsActive = true
        };
        var result = await _membershipRepository.EnrollMembership(enrolledMembership);
        return Ok(new { message = "Enrolled" });
    }

    [HttpPost("upgrade")]
    public async Task<ActionResult<EnrolledMembershipModel>> UpgradeMembership(int userId, int membershipId,
        int newMembershipId)
    {
        var membership = await _membershipRepository.GetMembershipById(newMembershipId);

        if (membership == null)
        {
            return NotFound(new { message = "Membership not found" });
        }

        var user = await _userRepository.GetUserById(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var enrolledMembership =
            await _membershipRepository.GetEnrolledMembershipByUserIdAndMembershipId(userId, membershipId);
        if (enrolledMembership == null)
        {
            return NotFound(new { message = "User not enrolled in the membership" });
        }

        var result = await _membershipRepository.UpgradeMembership(enrolledMembership, newMembershipId);
        return Ok(new { message = "Upgraded" });
    }

    [HttpPost("downgrade")]
    public async Task<ActionResult<EnrolledMembershipModel>> DowngradeMembership(int userId, int membershipId,
        int newMembershipId)
    {
        var enrolledMembership =
            await _membershipRepository.GetEnrolledMembershipByUserIdAndMembershipId(userId, membershipId);
        if (enrolledMembership == null)
        {
            return NotFound();
        }

        enrolledMembership.MembershipId = newMembershipId;
        var result = await _membershipRepository.DowngradeMembership(enrolledMembership, membershipId);
        return Ok(new { message = "Downgraded" });
    }

    [HttpPost("end")]
    public async Task<ActionResult> EndMembership(int userId, int membershipId)
    {
        var enrolledMembership =
            await _membershipRepository.GetEnrolledMembershipByUserIdAndMembershipId(userId, membershipId);
        if (enrolledMembership == null)
        {
            return NotFound();
        }

        var result = await _membershipRepository.EndMembership(enrolledMembership);
        if (result)
        {
            return Ok(new { message = "Membership ended" });
        }

        return NotFound();
    }

    [HttpGet("enrolled/creator/{creatorId}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>> GetEnrolledMembershipsByCreatorId(
        int creatorId)
    {
        var enrolledMemberships = await _membershipRepository.GetEnrolledMembershipsByCreatorId(creatorId);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/username/{userName}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>> GetEnrolledMembershipsByUserName(
        string userName)
    {
        var enrolledMemberships = await _membershipRepository.GetEnrolledMembershipsByUserName(userName);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/{id}")]
    public async Task<ActionResult<EnrolledMembershipViewDto>> GetEnrolledMembershipById(int id)
    {
        var enrolledMembership = await _membershipRepository.GetEnrolledMembershipById(id);

        if (enrolledMembership == null)
        {
            return NotFound();
        }

        return MembershipMapper.MapToViewDto(enrolledMembership);
    }

    [HttpGet("enrolled/membership/{membershipId}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>> GetEnrolledMembershipsByMembershipId(
        int membershipId)
    {
        var enrolledMemberships = await _membershipRepository.GetEnrolledMembershipsByMembershipId(membershipId);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/membership/{membershipId}/creator/{creatorId}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>>
        GetEnrolledMembershipsByMembershipIdAndCreatorId(int membershipId, int creatorId)
    {
        var enrolledMemberships =
            await _membershipRepository.GetEnrolledMembershipsByMembershipIdAndCreatorId(membershipId, creatorId);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/user/{userId}")]
    public async Task<ActionResult<IEnumerable<EnrolledMembershipViewDto>>> GetEnrolledMembershipsByUserId(int userId)
    {
        var enrolledMemberships = await _membershipRepository.GetEnrolledMembershipsByUserId(userId);

        if (enrolledMemberships == null || enrolledMemberships.Count == 0)
        {
            return NotFound();
        }

        return enrolledMemberships.Select(em => MembershipMapper.MapToViewDto(em)).ToList();
    }

    [HttpGet("enrolled/user/{userId}/membership/{membershipId}")]
    public async Task<ActionResult<EnrolledMembershipViewDto>> GetEnrolledMembershipByUserIdAndMembershipId(int userId,
        int membershipId)
    {
        var enrolledMembership =
            await _membershipRepository.GetEnrolledMembershipByUserIdAndMembershipId(userId, membershipId);

        if (enrolledMembership == null)
        {
            return NotFound();
        }

        return MembershipMapper.MapToViewDto(enrolledMembership);
    }
}