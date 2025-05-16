namespace ArtEFundAPIServices.DTO;

public class KhaltiLookUpDto
{
    public string Pidx { get; set; }
    public int TotalAmount { get; set; }
    public string Status { get; set; }
    public string? TransactionId { get; set; }
    public int Fee { get; set; }
    public bool Refunded { get; set; }
}