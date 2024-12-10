using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.Data.Model;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(UserName), IsUnique = true)]
public class UserModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } 

    // Navigation property for UserRoles
    public ICollection<UserRole> UserRoles { get; set; }
}