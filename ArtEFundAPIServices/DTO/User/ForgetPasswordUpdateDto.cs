using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.User;

public class ForgetPasswordUpdateDto
{

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\W).{8,}$",
        ErrorMessage =
            "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one special character.")]
    public string Password { get; set; }

    public string? ConfirmPassword { get; set; }
    public string? Token { get; set; }
}