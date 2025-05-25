using System.Net;
using System.Net.Mail;
using ArtEFundAPIServices.Data.Model;
using ArtEFundAPIServices.DataAccess.RefreshToken;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.DTO;
using ArtEFundAPIServices.DTO.Auth;
using ArtEFundAPIServices.DTO.User;
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

            if (!userModel.IsVerified)
            {
                return BadRequest("User is not verified");
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

            return Ok(new
            {
                accessToken = accessToken.token,
                accessTokenExpires = accessToken.expires,
                message = "Login successful",
                user = userViewDto,
                refreshToken = refreshToken.Token,
                refreshTokenExpires = refreshToken.Expires
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> ResendVerificationLink([FromBody] TokenDto tokenDto)
    {
        try
        {
            UserModel? user = await _userInterface.GetUserByVerificationToken(tokenDto.Token);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            if (user.IsVerified)
            {
                return BadRequest("User is already verified");
            }

            Guid verificationToken = Guid.NewGuid();

            user.VerificationToken = verificationToken;
            user.VerificationTokenExpiry = DateTime.Now.AddHours(1);
            await _userInterface.UpdateUser(user);

            String emailPass = _configuration["Gmail:Password"];

            if (emailPass == null)
            {
                return BadRequest("Email password not set in configuration");
            }

            var mail = new MailMessage("adarshakarki33@gmail.com", user.Email)
            {
                Subject = "Verify Account",
                Body =
                    $"<h1>Verify your Art E Fund Account</h1> <a href='http://localhost:3000/verify-account?token={verificationToken}'>Click this link to verify</a>",
                IsBodyHtml = true
            };

            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("adarshakarki33@gmail.com", password: emailPass),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(mail);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> VerifyAccount([FromBody] TokenDto tokenDto)
    {
        try
        {
            String token = tokenDto.Token;

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is missing");
            }

            var verifyTokenDto = await VerifyUserToken(token);
            if (verifyTokenDto.IsRevoked)
            {
                return BadRequest(verifyTokenDto.Message);
            }

            var user = verifyTokenDto.User;

            user.IsVerified = true;
            user.VerificationToken = null;
            user.VerificationTokenExpiry = null;

            await _userInterface.UpdateUser(user);

            return Ok("Account verified successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgetPasswordDto forgotPasswordDto)
    {
        try
        {
            if (string.IsNullOrEmpty(forgotPasswordDto.Email))
            {
                return BadRequest("Email is missing");
            }

            var user = await _userInterface.GetUserByEmail(forgotPasswordDto.Email);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var resetToken = Guid.NewGuid();

            user.VerificationToken = resetToken;
            user.VerificationTokenExpiry = DateTime.Now.AddHours(1);

            await _userInterface.UpdateUser(user);

            String emailPass = _configuration["Gmail:Password"];

            if (emailPass == null)
            {
                return BadRequest("Email password not set in configuration");
            }

            var mail = new MailMessage("adarshakarki33@gmail.com", user.Email)
            {
                Subject = "Reset Password",
                Body =
                    $"<h1>Reset your password</h1> <a href='http://localhost:3000/update-password?token={resetToken}'>Click this link to reset your password</a>",
                IsBodyHtml = true
            };
            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("adarshakarki33@gmail.com", emailPass),
                EnableSsl = true,
            };
            await smtpClient.SendMailAsync(mail);

            return Ok("Reset password email sent successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPost]
    public async Task<IActionResult> UpdateForgotPassword(ForgetPasswordUpdateDto forgotPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            VerifyTokenDto verifyTokenDto = await VerifyUserToken(forgotPasswordDto.Token);

            if (verifyTokenDto.IsRevoked)
            {
                return BadRequest(verifyTokenDto.Message);
            }

            var user = verifyTokenDto.User;

            if (forgotPasswordDto.Password != forgotPasswordDto.ConfirmPassword)
            {
                return BadRequest("Passwords do not match");
            }

            user.PasswordHash = _passwordHasher.HashPassword(forgotPasswordDto.Password);
            user.VerificationToken = null;
            user.VerificationTokenExpiry = null;

            await _userInterface.UpdateUser(user);

            return Ok(true);
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

            var verificationToken = Guid.NewGuid();

            UserModel user = new UserModel()
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                PasswordHash = _passwordHasher.HashPassword(userDto.Password),
                CreatedAt = DateTime.Now,
                IsVerified = false,
                VerificationToken = verificationToken,
                VerificationTokenExpiry = DateTime.Now.AddHours(1),
            };

            List<UserType> userTypes = await _userInterface.GetUserTypes();

            int userTypeId = userTypes.FirstOrDefault(u => u.UserTypeName == "credentials")?.UserTypeId ??
                             Constants.DEFAULT_USER_TYPE_ID;

            UserModel newUser = await _userInterface.AddUser(user, userTypeId);

            UserViewDto userViewDto = UserMapper.ToUserViewDto(newUser);

            var response = new BaseResponseModel<UserViewDto>()
            {
                Data = userViewDto,
                Message = "User registered successfully"
            };

            String emailPass = _configuration["Gmail:Password"];

            if (emailPass == null)
            {
                return BadRequest("Email password not set in configuration");
            }

            var mail = new MailMessage("adarshakarki33@gmail.com", user.Email)
            {
                Subject = "Verify Account",
                Body =
                    $"<h1>Verify your Art E Fund Account</h1> <a href='http://localhost:3000/verify-account?token={verificationToken}'>Click this link to verify</a>",
                IsBodyHtml = true
            };

            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("adarshakarki33@gmail.com", password: emailPass),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(mail);

            return Ok(new { response });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Refresh([FromBody] string token)
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

            return Ok(new
            {
                accessToken = accessToken.token, accessTokenExpires = accessToken.expires,
                refreshToken = newRefreshToken, refreshTokenExpires = newRefreshTokenModel.Expires
            });
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

            List<UserType> userTypes = await _userInterface.GetUserTypes();

            int userTypeId = userTypes.FirstOrDefault(u => u.UserTypeName == "google")?.UserTypeId ??
                             Constants.DEFAULT_USER_TYPE_ID;

            if (user == null)
            {
                // Create a new user if not exists
                user = new UserModel
                {
                    Email = email,
                    UserName = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PasswordHash = ""
                    // Set other properties as needed
                };
                user = await _userInterface.AddUser(user, userTypeId);
            }

            // Generate tokens
            var accessToken = TokenGenerator.GenerateToken(user.UserId, _configuration["Jwt:SecretKey"],
                _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], user.RoleModel.RoleName);
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

            var userViewDto = UserMapper.ToUserViewDto(user);

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            return Ok(new
            {
                accessToken = accessToken.token, accessTokenExpires = accessToken.expires, refreshToken,
                refreshTokenExpires = refreshTokenModel.Expires, user = userViewDto
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    private class VerifyTokenDto
    {
        public String Message { get; set; }
        public Boolean IsRevoked { get; set; }
        public UserModel? User { get; set; }
    }

    private async Task<VerifyTokenDto> VerifyUserToken(string token)
    {
        try
        {
            var verifyTokenDto = new VerifyTokenDto()
            {
                Message = "",
                IsRevoked = true,
                User = null
            };

            if (string.IsNullOrEmpty(token))
            {
                verifyTokenDto.Message = "Token is missing";
                verifyTokenDto.IsRevoked = true;
                return verifyTokenDto;
            }

            var user = await _userInterface.GetUserByVerificationToken(token);

            if (user == null)
            {
                verifyTokenDto.Message = "User not found";
                verifyTokenDto.IsRevoked = true;
                return verifyTokenDto;
            }

            if (user.VerificationTokenExpiry < DateTime.Now)
            {
                verifyTokenDto.Message = "Token has expired";
                verifyTokenDto.IsRevoked = true;
                verifyTokenDto.User = user;

                return verifyTokenDto;
            }


            verifyTokenDto.IsRevoked = false;
            verifyTokenDto.Message = "Token is valid";
            verifyTokenDto.User = user;

            return verifyTokenDto;
        }
        catch (Exception ex)
        {
            return new VerifyTokenDto()
            {
                Message = ex.Message,
                IsRevoked = true,
                User = null
            };
        }
    }
}