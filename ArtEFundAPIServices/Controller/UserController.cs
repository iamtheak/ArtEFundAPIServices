using ArtEFundAPIServices.Attributes;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO.Auth;
using ArtEFundAPIServices.DTO.User;
using ArtEFundAPIServices.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtEFundAPIServices.Controller;

[Authorize]
[ApiController]
[Route("/api/[controller]")]
[Consumes("application/json")]
[Authorize]
public class UserController(IUserInterface _userInterface) : ControllerBase
{
    [HttpGet]
    [RoleCheck("admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userInterface.GetAllUsers();

        List<UserViewDto> userViewDtos = new List<UserViewDto>();

        foreach (var user in users)
        {
            userViewDtos.Add(UserMapper.ToUserViewDto(user));
        }

        return Ok(userViewDtos);
    }

    [HttpGet("recent")]
    [RoleCheck("admin")]
    public async Task<IActionResult> GetRecentUsers()
    {
        var users = await _userInterface.GetAllUsers();
        var recentUsers = users.OrderByDescending(u => u.CreatedAt).Take(10).ToList();
        List<UserViewDto> userViewDtos = new List<UserViewDto>();
        foreach (var user in recentUsers)
        {
            userViewDtos.Add(UserMapper.ToUserViewDto(user));
        }

        return Ok(userViewDtos);
    }

    [HttpGet("total")]
    [RoleCheck("admin")]
    public async Task<IActionResult> GetTotalUsers()
    {
        var totalUsers = await _userInterface.GetAllUsers();
        return Ok(totalUsers.Count);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userInterface.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(UserMapper.ToUserViewDto(user));
    }

    [HttpPatch("role/{id}")]
    public async Task<ActionResult<UserViewDto>> UpdateUserRole([FromRoute] int id,
        [FromBody] UserRoleUpdateDto userRole)
    {
        try
        {
            var user = await _userInterface.GetUserById(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var role = await _userInterface.GetRoles();

            var newRole = role.FirstOrDefault(r => r.RoleName == userRole.Role);

            if (newRole == null)
            {
                return BadRequest("Invalid role.");
            }

            user.RoleId = newRole.RoleId;

            await _userInterface.UpdateUser(user);

            return Ok(UserMapper.ToUserViewDto(user));
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Internal server error", error = e.Message });
        }
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UserUpdateDto userDto)
    {
        var user = await _userInterface.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.UserType.UserTypeName != "google")
        {
            user.Email = userDto.Email;
        }

        if (user.Email != userDto.Email)
        {
            return BadRequest("Email not set in configuration");
        }

        var existingUser = await _userInterface.GetUserByUserName(userDto.UserName);

        if (existingUser != null && existingUser.UserId != id)
        {
            return BadRequest("Username already exists.");
        }

        existingUser = await _userInterface.GetUserByEmail(userDto.Email);
        if (existingUser != null && existingUser.UserId != id)
        {
            return BadRequest("Email already exists.");
        }

        user.UserName = userDto.UserName;
        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.ProfilePicture = userDto.ProfilePicture;

        try
        {
            await _userInterface.UpdateUser(user);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return Ok(UserMapper.ToUserViewDto(user));
    }
}