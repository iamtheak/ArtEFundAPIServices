using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.Creator;

public class ApiKeyRequestDto
{
    [Required] 
    public string ApiKey { get; set; }
}
