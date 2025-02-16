using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.Auth;

public class UserCreateDto
{
    
    [Required(ErrorMessage = "First Name is required")]
    [MinLength(3, ErrorMessage = "First Name must be at least 3 characters long")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Last Name is required")]
    [MinLength(3, ErrorMessage = "Last Name must be at least 3 characters long")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
    public string UserName { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\W).{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one special character.")]    
    public string Password { get; set; }
    
    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
    
    public string? ProfilePicture { get; set; }
}