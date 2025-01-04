using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class RefreshTokenModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RefreshTokenId { get; set; }
    
    public string Token { get; set; }
    
    public DateTime Expires { get; set; }
    
    public bool IsRevoked { get; set; }
    public int UserId { get; set; }
    
}