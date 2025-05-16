using System.ComponentModel.DataAnnotations;

namespace ArtEFundAPIServices.Data.Model;

public class PaymentModel
{
    [Key] public int PaymentId { get; set; }

    public decimal Amount { get; set; }
    public string KhaltiPaymentId { get; set; }

    public string PaymentStatus { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}