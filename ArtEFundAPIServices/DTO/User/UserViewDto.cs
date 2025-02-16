using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.User;

public class UserViewDto
{
    
    public int UserId { get; set; }
    
    public string FirstName { get; set; }
    
    public string UserName { get; set; }
    
    public string LastName { get; set; }
    
    public string Email { get; set; }
    
    public string Role { get; set; }
    
    public string? ProfilePicture { get; set; }
}