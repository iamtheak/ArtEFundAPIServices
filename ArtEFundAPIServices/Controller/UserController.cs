using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO.Auth;
using ArtEFundAPIServices.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtEFundAPIServices.Controller;

[Authorize]
[ApiController]
[Route("/api/[controller]")]
[Consumes("application/json")]
public class UserController(IUserInterface _userInterface) : ControllerBase
{
    [HttpGet]
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
}