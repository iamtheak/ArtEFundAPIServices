using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtEFundAPIServices.Data.Model;

public class DonationModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DonationId { get; set; }

    public DateTime DonationDate { get; set; } = DateTime.Now;


    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal DonationAmount { get; set; }

    public string? DonationMessage { get; set; }
    public int CreatorId { get; set; }

    public int? UserId { get; set; }
    public CreatorModel Creator { get; set; }
}