using ArtEFundAPIServices.Data.Model;

namespace ArtEFundAPIServices.DataAccess.Payment;

public interface IPaymentInterface
{
    Task<PaymentModel?> GetPaymentById(int id);
    Task<PaymentModel> CreatePayment(PaymentModel payment);
    Task<PaymentModel?> GetPaymentByKhaltiId(string khaltiId);
}