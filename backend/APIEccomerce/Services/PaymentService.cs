using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace APIEccomerce.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderService _orderService;
        private readonly IConfiguration _config;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IOrderRepository orderRepo,
            IOrderService orderService,
            IConfiguration config)
        {
            _paymentRepo  = paymentRepo;
            _orderRepo    = orderRepo;
            _orderService = orderService;
            _config       = config;
        }

        public async Task<PaymentIntentDto> CreateIntent(int userId, int orderId)
        {
            var order = await _orderRepo.GetById(orderId)
                ?? throw new KeyNotFoundException("Pedido no encontrado.");

            if (order.UserId != userId)
                throw new UnauthorizedAccessException(
                    "El pedido no pertenece al usuario.");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException(
                    "Este pedido ya fue procesado.");

            var reference = $"ORDER-{orderId}-{DateTime.UtcNow.Ticks}";
            var amountInCents = (long)(order.Total * 100);

            var secret    = _config["Wompi:IntegritySecret"]!;
            var rawString = $"{reference}{amountInCents}COP{secret}";
            var signature = ComputeSha256(rawString);

            var existing = await _paymentRepo.GetByOrderId(orderId);
            if (existing is null)
            {
                await _paymentRepo.Create(new Payment
                {
                    OrderId   = orderId,
                    Reference = reference,
                    Amount    = order.Total,
                    Status    = PaymentStatus.Pending
                });
            }

            return new PaymentIntentDto
            {
                Reference     = reference,
                AmountInCents = amountInCents,
                Currency      = "COP",
                PublicKey     = _config["Wompi:PublicKey"]!,
                Signature     = signature,
                RedirectUrl   = $"{_config["Frontend:Url"]}/pedido/{orderId}/confirmacion"
            };
        }

        public async Task ProcessWebhook(WompiWebhookDto webhook)
        {
            if (webhook.Event != "transaction.updated")
                return;

            if (!ValidateWebhookSignature(webhook))
                throw new UnauthorizedAccessException(
                    "Firma del webhook inválida.");

            var transaction = webhook.Data.Transaction;
            var status      = MapStatus(transaction.Status);
            var paidAt      = status == PaymentStatus.Approved
                ? DateTime.UtcNow : (DateTime?)null;

            await _paymentRepo.UpdateStatus(
                transaction.Reference,
                status,
                transaction.Id,
                transaction.PaymentMethodType,
                paidAt);

            if (status == PaymentStatus.Approved)
            {
                var payment = await _paymentRepo
                    .GetByReference(transaction.Reference);

                if (payment is not null)
                    await _orderService.ConfirmPayment(payment.OrderId);
            }
        }

        private bool ValidateWebhookSignature(WompiWebhookDto webhook)
        {
            var secret = _config["Wompi:EventSecret"]!;
            var transaction = webhook.Data.Transaction;
            var props       = webhook.Signature.Properties;

            var values = props.Select(p => p switch
            {
                "transaction.id"            => transaction.Id,
                "transaction.status"        => transaction.Status,
                "transaction.amount_in_cents" =>
                    transaction.AmountInCents.ToString(),
                _ => string.Empty
            });

            var rawString = string.Concat(values)
                            + webhook.Timestamp
                            + secret;

            var computed = ComputeSha256(rawString);
            return computed == webhook.Signature.Checksum;
        }

        private static PaymentStatus MapStatus(string wompiStatus) =>
            wompiStatus switch
            {
                "APPROVED" => PaymentStatus.Approved,
                "DECLINED" => PaymentStatus.Declined,
                "VOIDED"   => PaymentStatus.Voided,
                "ERROR"    => PaymentStatus.Error,
                _          => PaymentStatus.Pending
            };

        private static string ComputeSha256(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
