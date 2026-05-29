using APIEccomerce.Data;
using APIEccomerce.Models;
using APIEccomerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIEccomerce.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByReference(string reference) =>
            await _context.Payments
                .FirstOrDefaultAsync(p => p.Reference == reference);

        public async Task<Payment?> GetByOrderId(int orderId) =>
            await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

        public async Task<Payment> Create(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment?> UpdateStatus(
            string reference,
            PaymentStatus status,
            string transactionId,
            string? paymentMethod,
            DateTime? paidAt)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Reference == reference);

            if (payment is null) return null;

            payment.Status        = status;
            payment.TransactionId = transactionId;
            payment.PaymentMethod = paymentMethod;
            payment.PaidAt        = paidAt;

            await _context.SaveChangesAsync();
            return payment;
        }
    }
}
