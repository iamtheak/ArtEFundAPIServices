using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.DTO.Creator;

public class CreatorCreateDto
{
    public int ContentTypeId { get; set; }

    [MaxLength(200)] public string? Bio { get; set; }

    [MaxLength(300)] public string? Description { get; set; }

    public int UserId { get; set; }
}