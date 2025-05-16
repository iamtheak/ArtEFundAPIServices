using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.User;

public class ForgetPasswordDto
{
    [Required] [EmailAddress] public string Email { get; set; }
}