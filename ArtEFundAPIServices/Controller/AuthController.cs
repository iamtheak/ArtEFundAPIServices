using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.RefreshToken;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO;
using ArtEFundAPIServices.DTO.Auth;
using ArtEFundAPIServices.Helper;
using ArtEFundAPIServices.Mapper;
using Microsoft.AspNetCore.Mvc;
using Exception = System.Exception;
using Google.Apis.Auth;


namespace ArtEFundAPIServices.Controller;

[ApiController]
[Route("api/[controller]/[action]")]
[Consumes("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserInterface _userInterface;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenInterface _refreshTokenInterface;

    public AuthController(IUserInterface userInterface, IPasswordHasher passwordHasher, IConfiguration configuration,
        IRefreshTokenInterface refreshTokenInterface)
    {
        _userInterface = userInterface;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _refreshTokenInterface = refreshTokenInterface;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            UserModel? userModel = await _userInterface.GetUserByEmail(loginDto.Email);

            if (userModel == null)
            {
                return BadRequest("User not found");
            }

            if (!_passwordHasher.VerifyPassword(loginDto.Password, userModel.PasswordHash))
            {
                return BadRequest("Invalid password");
            }

            var accessToken = TokenGenerator.GenerateToken(userModel.UserId,
                _configuration["Jwt:SecretKey"] ?? Constants.DEFAULT_JWT_SECRET, _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"], userModel.RoleModel.RoleName);

            var userViewDto = UserMapper.ToUserViewDto(userModel);
            RefreshTokenModel refreshToken = new RefreshTokenModel()
            {
                UserId = userModel.UserId,
                Token = TokenGenerator.GenerateRefreshToken(),
                Expires = DateTime.Now.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenInterface.AddRefreshToken(refreshToken);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshToken.Expires
            };

            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

            return Ok(new { accessToken, message = "Login successful", user = userViewDto , refreshToken = refreshToken.Token});
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
            if (ModelState.IsValid == false)
            {
                return BadRequest(new
                    { errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList() });
            }

            var existingUser = await _userInterface.GetUserByEmail(userDto.Email);
            if (existingUser != null)
            {
                return BadRequest("User with this email already exists");
            }

            existingUser = await _userInterface.GetUserByUserName(userDto.UserName);
            if (existingUser != null)
            {
                return BadRequest("User with this username already exists");
            }

            UserModel user = new UserModel()
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                PasswordHash = _passwordHasher.HashPassword(userDto.Password)
            };
            UserModel newUser = await _userInterface.AddUser(user);

            UserViewDto userViewDto = UserMapper.ToUserViewDto(newUser);

            var response = new BaseResponseModel<UserViewDto>()
            {
                Data = userViewDto,
                Message = "User registered successfully"
            };

            return Ok(new { response });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Refresh([FromBody ]string token )
    {
        try
        {
          
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Refresh token is missing");
            }

            RefreshTokenModel? refreshTokenModel = await _refreshTokenInterface.GetRefreshToken(token);

            if (refreshTokenModel == null)
            {
                return BadRequest("Invalid refresh token");
            }

            if (refreshTokenModel.Expires < DateTime.Now)
            {
                await _refreshTokenInterface.RevokeRefreshToken(token);
                return BadRequest("Refresh token has expired");
            }

            if (refreshTokenModel.IsRevoked)
            {
                await _refreshTokenInterface.DeleteRefreshToken(token);
                return BadRequest("Refresh token has been revoked");
            }

            var user = await _refreshTokenInterface.GetUserFromRefreshToken(token);

            if (user == null)
            {
                return NotFound("User with the token not found");
            }

            var newRefreshToken = TokenGenerator.GenerateRefreshToken();

            RefreshTokenModel newRefreshTokenModel = new RefreshTokenModel()
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                Expires = DateTime.Now.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenInterface.RevokeRefreshToken(token);
            await _refreshTokenInterface.AddRefreshToken(newRefreshTokenModel);

            var accessToken = TokenGenerator.GenerateToken(user.UserId,
                _configuration["Jwt:SecretKey"] ?? Constants.DEFAULT_JWT_SECRET, _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"], user.RoleModel.RoleName);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = newRefreshTokenModel.Expires
            };

            Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

            return Ok(new { accessToken, refreshToken = newRefreshToken });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPost]
    [Route("/api/auth/google-login")]
    public async Task<IActionResult> VerifyGoogleLoginToken([FromBody] string token)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token);
            if (payload == null)
            {
                return BadRequest("Invalid Google token");
            }

            // Access user information from payload
            var email = payload.Email;
            var firstName = payload.GivenName;
            var lastName = payload.FamilyName;

            // Check if user exists in the database
            var user = await _userInterface.GetUserByEmail(email);
            if (user == null)
            {
                // Create a new user if not exists
                user = new UserModel
                {
                    Email = email,
                    UserName = string.Concat(firstName, "_", lastName),
                    FirstName = firstName,
                    LastName = lastName,
                    PasswordHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString())
                    // Set other properties as needed
                };
                user = await _userInterface.AddUser(user);
                
            }

            // Generate tokens
            var accessToken = TokenGenerator.GenerateToken(user.UserId, _configuration["Jwt:SecretKey"], _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], user.RoleModel.RoleName);
            var refreshToken = TokenGenerator.GenerateRefreshToken();

            var refreshTokenModel = new RefreshTokenModel
            {
                UserId = user.UserId,
                Token = refreshToken,
                Expires = DateTime.Now.AddDays(7),
                IsRevoked = false
            };
            await _refreshTokenInterface.AddRefreshToken(refreshTokenModel);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshTokenModel.Expires
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            return Ok(new { accessToken, refreshToken });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
}