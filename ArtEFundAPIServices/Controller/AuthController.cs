using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO;
using ArtEFundAPIServices.DTO.Auth;
using ArtEFundAPIServices.Helper;
using Microsoft.AspNetCore.Mvc;
using Exception = System.Exception;

namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController :ControllerBase
{

    private readonly IUserInterface _userInterface;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    public AuthController(IUserInterface userInterface,IPasswordHasher passwordHasher, IConfiguration configuration )
    {
        _userInterface = userInterface;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }
   
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        try
        {
            UserModel? userModel = await _userInterface.GetUserByEmail(email);
            
            if(userModel == null)
            {
                return BadRequest("User not found");
            }

            if (!_passwordHasher.VerifyPassword(password, userModel.PasswordHash))
            {
                return BadRequest("Invalid password");
            }

            var userView = new UserViewDto()
            {
                UserId = userModel.UserId,
                UserName = userModel.UserName,
                Email = userModel.Email,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                Roles = userModel.UserRoles.Select(userRole => userRole.RoleModel.RoleName).ToArray()
            };

            BaseResponseModel<UserViewDto> response = new BaseResponseModel<UserViewDto>()
            {
                Data = userView,
                Message = "User Logged in successfully"
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
           return BadRequest(ex.Message);
        }
        
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] UserCreateDto userDto)
    {
        try
        {
            if(ModelState.IsValid == false)
            {
                return BadRequest(new {errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList()});
            }
            var existingUser = await _userInterface.GetUserByEmail(userDto.Email);
            if (existingUser != null)
            {
               return BadRequest("User with this email already exists");
            }
            existingUser = await _userInterface.GetUserByUserName(userDto.UserName);
            if (existingUser != null)
            {
              return  BadRequest("User with this username already exists");
            }
            
            UserModel user = new UserModel()
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                PasswordHash = _passwordHasher.HashPassword(userDto.Password)
            };
            UserModel newUser =  await _userInterface.AddUser(user);

            UserViewDto userViewDto = new UserViewDto()
            {
                UserName = newUser.UserName,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                UserId = newUser.UserId
            };
            
            var response = new BaseResponseModel<UserViewDto>()
            {
                Data = userViewDto,
                Message = "User registered successfully"
            };

            return Ok(new {response});
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}