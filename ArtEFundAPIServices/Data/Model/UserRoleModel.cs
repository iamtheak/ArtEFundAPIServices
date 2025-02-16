using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class UserRoleModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserRoleId { get; set; }

    public int UserId { get; set; }
    public UserModel UserModel { get; set; }

    public int RoleId { get; set; }
    public RoleModel RoleModel { get; set; }
}