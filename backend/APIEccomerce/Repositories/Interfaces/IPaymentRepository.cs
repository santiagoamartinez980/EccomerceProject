using APIEccomerce.Models;

namespace APIEccomerce.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByReference(string reference);
        Task<Payment?> GetByOrderId(int orderId);
        Task<Payment> Create(Payment payment);
        Task<Payment?> UpdateStatus(
            string reference,
            PaymentStatus status,
            string transactionId,
            string? paymentMethod,
            DateTime? paidAt);
    }
}
