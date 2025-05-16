using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtEFundAPIServices.DataAccess.Payment;

public class PaymentRepo : IPaymentInterface
{
    private readonly ApplicationDbContext _context;

    public PaymentRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentModel?> GetPaymentById(int id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public async Task<PaymentModel> CreatePayment(PaymentModel payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<PaymentModel?> GetPaymentByKhaltiId(string khaltiId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.KhaltiPaymentId == khaltiId);
    }
}