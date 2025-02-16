using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.Creator;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO.Creator;
using ArtEFundAPIServices.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/creators")]
public class CreatorController : ControllerBase
{
    private readonly ICreatorInterface _creatorInterface;
    private readonly IUserInterface _userInterface;

    public CreatorController(ICreatorInterface creatorInterface, IUserInterface userInterface)
    {
        _creatorInterface = creatorInterface;
        _userInterface = userInterface;
    }

    // GET api/creators
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreatorViewDto>>> GetCreators()
    {
        try
        {
            var creators = await _creatorInterface.GetCreators();
            var creatorDtos = creators.Select(CreatorMapper.ToCreatorViewDto).ToList();
            return Ok(creatorDtos);
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

            
            if(userModel == null)
            {
                return NotFound(new { message = $"User with ID {creatorDto.UserId} not found" });
            }
            var existingCreator = await _creatorInterface.GetCreatorByUserId(userModel.UserId);

            if (existingCreator != null)
            {
                return BadRequest("Creator already exists for this user");
            }
            
            CreatorModel creatorModel = CreatorMapper.ToCreatorModel(creatorDto);
            var creator = await _creatorInterface.CreateCreator(creatorModel);

            if (userModel == null)
            {
                return NotFound(new { message = $"User with ID {creator.UserId} not found" });
            }

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

            CreatorModel creatorModel = CreatorMapper.ToCreatorModel(creatorDto);
            var creator = await _creatorInterface.UpdateCreator(id, creatorModel);
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
}