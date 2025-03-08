using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.Membership;
using ArtEFundAPIServices.DTO.Membership;
using ArtEFundAPIServices.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/[controller]")]
public class MembershipController : ControllerBase
{
    private readonly IMembershipInterface _membershipRepository;

    public MembershipController(IMembershipInterface membershipRepository)
    {
        _membershipRepository = membershipRepository;
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
    public async Task<ActionResult<EnrolledMembershipModel>> EnrollMembership(EnrolledMembershipModel enrolledMembership)
    {
        var result = await _membershipRepository.EnrollMembership(enrolledMembership);
        return Ok(result);
    }

    [HttpPost("upgrade")]
    public async Task<ActionResult<EnrolledMembershipModel>> UpgradeMembership(EnrolledMembershipModel enrolledMembership, int membershipId)
    {
        var result = await _membershipRepository.UpgradeMembership(enrolledMembership, membershipId);
        return Ok(result);
    }

    [HttpPost("downgrade")]
    public async Task<ActionResult<EnrolledMembershipModel>> DowngradeMembership(EnrolledMembershipModel enrolledMembership, int membershipId)
    {
        var result = await _membershipRepository.DowngradeMembership(enrolledMembership, membershipId);
        return Ok(result);
    }

    [HttpPost("end")]
    public async Task<ActionResult> EndMembership(EnrolledMembershipModel enrolledMembership)
    {
        var result = await _membershipRepository.EndMembership(enrolledMembership);
        if (result)
        {
            return Ok();
        }
        return NotFound();
    }


}