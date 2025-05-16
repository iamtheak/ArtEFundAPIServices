using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.Data.Model;

public class CreatorApiKeyModel
{
    [Key] public int CreatorApiId { get; set; }
    public int CreatorId { get; set; }
    public string EncryptedApiKey { get; set; }

    // Navigation property
    public CreatorModel Creator { get; set; }
}