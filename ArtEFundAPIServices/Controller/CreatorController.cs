using System.Security.Claims;
using ArtEFundAPIServices.Attributes;
using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.Creator;
using ArtEFundAPIServices.DataAccess.CreatorApiKey;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO.Creator;
using ArtEFundAPIServices.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/creators")]
public class CreatorController : ControllerBase
{
    private readonly ICreatorInterface _creatorInterface;
    private readonly IUserInterface _userInterface;
    private readonly ICreatorApiKeyInterface _creatorApiKeyInterface;

    public CreatorController(ICreatorInterface creatorInterface, IUserInterface userInterface,
        ICreatorApiKeyInterface creatorApiKeyInterface)
    {
        _creatorInterface = creatorInterface;
        _userInterface = userInterface;
        _creatorApiKeyInterface = creatorApiKeyInterface;
    }

// GET api/creators
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreatorViewDto>>> GetCreators()
    {
        try
        {
            var creators = await _creatorInterface.GetCreators();
            // Filter to only include creators with an API key
            creators = creators.Where(creator =>
                creator.ApiKey != null &&
                !string.IsNullOrEmpty(creator.ApiKey.EncryptedApiKey)
            ).ToList();

            var creatorDtos = creators.Select(CreatorMapper.ToCreatorViewDto).ToList();
            return Ok(creatorDtos);
        }
        catch (Exception e)
        {
            Console.Write(e);
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }
    

    [HttpGet("total")]
    [Authorize]
    [RoleCheck("admin")]
    public async Task<ActionResult<int>> GetTotalCreators()
    {
        try
        {
            var totalCreators = await _creatorInterface.GetCreators();
            return Ok(totalCreators.Count);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    // GET api/creators/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CreatorViewDto>> GetCreatorById(int id)
    {
        try
        {
            var creator = await _creatorInterface.GetCreatorById(id);
            if (creator == null)
            {
                return NotFound(new { message = $"Creator with ID {id} not found" });
            }

            return Ok(CreatorMapper.ToCreatorViewDto(creator));
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    // GET api/creators/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<CreatorViewDto>> GetCreatorByUserId(int userId)
    {
        try
        {
            var creator = await _creatorInterface.GetCreatorByUserId(userId);
            if (creator == null)
            {
                return NotFound(new { message = $"Creator with User ID {userId} not found" });
            }

            return Ok(CreatorMapper.ToCreatorViewDto(creator));
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    [HttpGet("content-type")]
    public async Task<ActionResult<List<ContentTypeModel>>> GetContentType()
    {
        try
        {
            var contentType = await _creatorInterface.GetContentTypes();
            return Ok(contentType);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    // GET api/creators/username/{username}
    [HttpGet("username/{username}")]
    public async Task<ActionResult<CreatorViewDto>> GetCreatorByUsername(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "Username cannot be empty" });
            }

            var creator = await _creatorInterface.GetCreatorByUserName(username);
            if (creator == null)
            {
                return NotFound(new { message = $"Creator with username '{username}' not found" });
            }

            if (creator.ApiKey.EncryptedApiKey.IsNullOrEmpty())
            {
                return BadRequest(new { message = "Creator does not have an API key" });
            }


            return Ok(CreatorMapper.ToCreatorViewDto(creator));
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    // POST api/creators
    [HttpPost]
    public async Task<ActionResult<CreatorViewDto>> CreateCreator([FromBody] CreatorCreateDto creatorDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            UserModel? userModel = await _userInterface.GetUserById(creatorDto.UserId);


            if (userModel == null)
            {
                return NotFound(new { message = $"User with ID {creatorDto.UserId} not found" });
            }

            var existingCreator = await _creatorInterface.GetCreatorByUserId(userModel.UserId);

            if (existingCreator != null)
            {
                return BadRequest("Creator already exists for this user");
            }

            CreatorModel? creatorModel = CreatorMapper.ToCreatorModel(creatorDto);
            var creator = await _creatorInterface.CreateCreator(creatorModel);

            return CreatedAtAction(
                nameof(GetCreatorById),
                new { id = creator.CreatorId },
                CreatorMapper.ToCreatorViewDto(creator));
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    // PUT api/creators/{id}    
    [HttpPut("{id}")]
    public async Task<ActionResult<CreatorViewDto>> UpdateCreator(int id, [FromBody] CreatorUpdateDto creatorDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            CreatorModel? existingCreator = await _creatorInterface.GetCreatorById(id);

            if (existingCreator == null)
            {
                return NotFound(new { message = $"Creator with ID {id} not found" });
            }

            existingCreator.CreatorBio = creatorDto.CreatorBio;
            existingCreator.CreatorDescription = creatorDto.CreatorDescription;
            existingCreator.CreatorBanner = creatorDto.CreatorBanner;
            existingCreator.ContentTypeId =
                creatorDto.ContentTypeId > 0 ? creatorDto.ContentTypeId : existingCreator.ContentTypeId;

            var creator = await _creatorInterface.UpdateCreator(existingCreator);

            return Ok(CreatorMapper.ToCreatorViewDto(creator));
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    // DELETE api/creators/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCreator(int id)
    {
        try
        {
            var result = await _creatorInterface.DeleteCreator(id);
            if (result)
            {
                return NotFound(new { message = $"Creator with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    // GET api/creators/api-key/{creatorId}/
    [HttpGet("api-key/{creatorId}")]
    [Authorize]
    public async Task<ActionResult<ApiKeyResponseDto>> GetCreatorApiKey(int creatorId)
    {
        try
        {
            // Check if creator exists
            var creator = await _creatorInterface.GetCreatorById(creatorId);
            if (creator == null)
            {
                return NotFound(new { message = $"Creator with ID {creatorId} not found" });
            }

            // Verify current user has permission
            int userId = 0;
            try
            {
                userId = GetUserIdFromJwt();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User ID not found in JWT" });
            }

            // Check if user is the creator or has admin role
            var currentCreator = await _creatorInterface.GetCreatorByUserId(userId);
            if (currentCreator == null || (currentCreator.CreatorId != creatorId && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            // Check if API key exists
            var hasApiKey = await _creatorApiKeyInterface.HasApiKey(creatorId);
            var apiKey = hasApiKey ? await _creatorApiKeyInterface.GetApiKeyForCreator(creatorId) : null;

            return Ok(new ApiKeyResponseDto { ApiKey = apiKey, HasApiKey = hasApiKey });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    // POST api/creators/{creatorId}/api-key
    [HttpPost("api-key/{creatorId}")]
    [Authorize]
    public async Task<ActionResult> SetCreatorApiKey(int creatorId, [FromBody] ApiKeyRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if creator exists
            var creator = await _creatorInterface.GetCreatorById(creatorId);
            if (creator == null)
            {
                return NotFound(new { message = $"Creator with ID {creatorId} not found" });
            }

            // Verify current user has permission
            int userId = 0;
            try
            {
                userId = GetUserIdFromJwt();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User ID not found in JWT" });
            }

            // Check if user is the creator or has admin role
            var currentCreator = await _creatorInterface.GetCreatorByUserId(userId);
            if (currentCreator == null || (currentCreator.CreatorId != creatorId && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            await _creatorApiKeyInterface.SaveApiKeyForCreator(creatorId, request.ApiKey);
            return Ok(new { message = "API key saved successfully" });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    // DELETE api/creators/{creatorId}/api-key
    [HttpDelete("api-key/{creatorId}")]
    [Authorize]
    public async Task<ActionResult> DeleteCreatorApiKey(int creatorId)
    {
        try
        {
            // Check if creator exists
            var creator = await _creatorInterface.GetCreatorById(creatorId);
            if (creator == null)
            {
                return NotFound(new { message = $"Creator with ID {creatorId} not found" });
            }

            // Verify current user has permission
            int userId = 0;
            try
            {
                userId = GetUserIdFromJwt();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User ID not found in JWT" });
            }

            // Check if user is the creator or has admin role
            var currentCreator = await _creatorInterface.GetCreatorByUserId(userId);
            if (currentCreator == null || (currentCreator.CreatorId != creatorId && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            await _creatorApiKeyInterface.DeleteApiKeyForCreator(creatorId);
            return Ok(new { message = "API key removed successfully" });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }

    [HttpPatch("profile-visit/{creatorId}")]
    public async Task<Boolean> CreatorProfileVisit(int creatorId)
    {
        try
        {
            var creator = await _creatorInterface.GetCreatorById(creatorId);
            if (creator == null)
            {
                return false;
            }

            creator.ProfileVisits += 1;
            await _creatorInterface.UpdateCreator(creator);
            return true;
        }
        catch (Exception e)
        {
            Console.Write(e);
            return false;
        }
    }

    [HttpPost("follow/{creatorId}/{userId}")]
    public async Task<IActionResult> FollowCreator(int creatorId, int userId)
    {
        try
        {
            var creator = await _creatorInterface.GetCreatorById(creatorId);
            if (creator == null)
            {
                return BadRequest(new { message = $"Creator with ID {creatorId} not found" });
            }

            var user = await _userInterface.GetUserById(userId);
            if (user == null)
            {
                return BadRequest(new { message = $"User with ID {userId} not found" });
            }

            if (creator.UserId == user.UserId)
            {
                return BadRequest(new { message = "You cannot follow yourself" });
            }

            var follow = new FollowModel
            {
                CreatorId = creatorId,
                UserId = userId,
                FollowDate = DateTime.UtcNow
            };

            var result = await _creatorInterface.FollowCreator(follow);

            if (result)
            {
                return Ok(new { message = "Followed successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to follow creator" });
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = e.Message });
        }
    }

    [HttpDelete("unfollow/{creatorId}/{userId}")]
    public async Task<IActionResult> UnfollowCreator(int creatorId, int userId)
    {
        try
        {
            var creator = await _creatorInterface.GetCreatorById(creatorId);
            if (creator == null)
            {
                return BadRequest(new { message = $"Creator with ID {creatorId} not found" });
            }

            var user = await _userInterface.GetUserById(userId);
            if (user == null)
            {
                return BadRequest(new { message = $"User with ID {userId} not found" });
            }

            var follow = new FollowModel
            {
                CreatorId = creatorId,
                UserId = userId
            };

            var result = await _creatorInterface.UnfollowCreator(follow);

            if (result)
            {
                return Ok(new { message = "Unfollowed successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to unfollow creator" });
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = e.Message });
        }
    }

    [HttpGet("is-following/{creatorId}/{userId}")]
    public async Task<IActionResult> IsFollowing(int creatorId, int userId)
    {
        try
        {
            var creator = await _creatorInterface.GetCreatorById(creatorId);
            if (creator == null)
            {
                return BadRequest(new { message = $"Creator with ID {creatorId} not found" });
            }

            var user = await _userInterface.GetUserById(userId);
            if (user == null)
            {
                return BadRequest(new { message = $"User with ID {userId} not found" });
            }

            var followModel = await _creatorInterface.GetFollowsByUserAndCreatorId(userId, creatorId);

            if (followModel == null)
            {
                return Ok(false);
            }

            return Ok(true);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = e.Message });
        }
    }

    [HttpGet("followers/{creatorId}")]
    public async Task<IActionResult> GetFollowers(int creatorId)
    {
        try
        {
            var creator = await _creatorInterface.GetCreatorById(creatorId);
            if (creator == null)
            {
                return BadRequest(new { message = $"Creator with ID {creatorId} not found" });
            }

            var followers = await _creatorInterface.GetFollowsByCreatorId(creatorId);
            var followerDtos = followers.Select(f => new FollowerDto
            {
                CreatorId = f.CreatorId,
                UserId = f.UserId,
                FollowerUserName = f.User?.UserName,
                FollowerAvatarUrl = f.User?.ProfilePicture,
            }).ToList();

            return Ok(followerDtos);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = e.Message });
        }
    }

    [HttpGet("follower-count/{creatorId}")]
    public async Task<IActionResult> GetFollowerCount(int creatorId)
    {
        try
        {
            var creator = await _creatorInterface.GetCreatorById(creatorId);
            if (creator == null)
            {
                return NotFound(new { message = $"Creator with ID {creatorId} not found" });
            }

            var followers = await _creatorInterface.GetFollowsByCreatorId(creatorId);
            var followerCount = followers.Count;

            return Ok(followerCount);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = e.Message });
        }
    }

    [HttpGet("following/{userId}")]
    public async Task<IActionResult> GetFollowingsByUser(int userId)
    {
        try
        {
            var user = await _userInterface.GetUserById(userId);
            if (user == null)
            {
                return BadRequest(new { message = $"User with ID {userId} not found" });
            }

            var followings = await _creatorInterface.GetFollowsByUserId(userId);
            var followingDtos = followings.Select(f => new FollowerDto()
            {
                CreatorId = f.CreatorId,
                UserId = f.UserId,
                FollowingUserName = f.Creator?.UserModel?.UserName,
                FollowingAvatarUrl = f.Creator?.UserModel?.ProfilePicture,
            }).ToList();

            return Ok(followingDtos);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = e.Message });
        }
    }


    private int GetUserIdFromJwt()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("User ID not found in JWT");
        }

        return userId;
    }
}