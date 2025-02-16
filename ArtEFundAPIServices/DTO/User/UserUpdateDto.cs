using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.Auth;

public class UserUpdateDto
{
    [Required(ErrorMessage = "First Name is required")]
    [MinLength(3, ErrorMessage = "First Name must be at least 3 characters long")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "First Name is required")]
    [MinLength(3, ErrorMessage = "First Name must be at least 3 characters long")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "First Name is required")]
    [MinLength(3, ErrorMessage = "First Name must be at least 3 characters long")]
    [RegularExpression("^[a-zA-Z0-9_-]+$")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
    public string Email { get; set; }

    public string? ProfilePicture { get; set; }
}