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

    [Required] [MaxLength(50)] public string FirstName { get; set; }

    [Required] [MaxLength(50)] public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }

    [Required] [MaxLength(255)] public string PasswordHash { get; set; }

    [Required] [MaxLength(50)] public string UserName { get; set; }

    public string? ProfilePicture { get; set; } 
    // Navigation property for UserRoles
    [ForeignKey("RoleId")] public int RoleId { get; set; }

    [ForeignKey("UserType")] public int UserTypeId { get; set; }
    public RoleModel RoleModel { get; set; }

    public UserType UserType { get; set; }

    public ICollection<RefreshTokenModel> RefreshTokens { get; set; }
    public ICollection<FollowModel> Followings { get; set; }
    public ICollection<EnrolledMembershipModel> EnrolledMemberships { get; set; }
}