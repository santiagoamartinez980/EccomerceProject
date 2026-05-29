using APIEccomerce.Models.DTOs;

namespace APIEccomerce.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentIntentDto> CreateIntent(int userId, int orderId);
        Task ProcessWebhook(WompiWebhookDto webhook);
    }
}
