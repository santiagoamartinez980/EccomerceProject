using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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

            var existing = await _paymentRepo.GetByOrderId(orderId);
            var reference = existing?.Status == PaymentStatus.Pending
                ? existing.Reference
                : $"ORDER-{orderId}-{Guid.NewGuid():N}";
            var amountInCents = (long)(order.Total * 100);

            var secret = _config["Wompi:IntegritySecret"];
            var publicKey = _config["Wompi:PublicKey"];
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(publicKey))
                throw new InvalidOperationException("Faltan credenciales de Wompi Sandbox.");
            var rawString = $"{reference}{amountInCents}COP{secret}";
            var signature = ComputeSha256(rawString);

            if (existing?.Status != PaymentStatus.Pending)
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
                PublicKey     = publicKey,
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
            var payment = await _paymentRepo.GetByReference(transaction.Reference)
                ?? throw new KeyNotFoundException("Referencia de pago no encontrada.");
            if (transaction.AmountInCents != (long)(payment.Amount * 100) ||
                !string.Equals(transaction.Currency, payment.Currency, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("El monto o la moneda no coinciden con el pago esperado.");
            var order = await _orderRepo.GetById(payment.OrderId)
                ?? throw new KeyNotFoundException("Pedido no encontrado.");
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Paid)
                throw new InvalidOperationException("El pedido no admite este pago.");
            var status      = MapStatus(transaction.Status);
            if (status == PaymentStatus.Pending)
                return;
            if (payment.Status == PaymentStatus.Approved)
            {
                if (status == PaymentStatus.Approved && order.Status == OrderStatus.Pending)
                    await _orderService.ConfirmPayment(payment.OrderId);
                return;
            }
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
                if (order.Status == OrderStatus.Pending)
                    await _orderService.ConfirmPayment(payment.OrderId);
            }
        }

        private bool ValidateWebhookSignature(WompiWebhookDto webhook)
        {
            var secret = _config["Wompi:EventSecret"];
            if (string.IsNullOrWhiteSpace(secret) || webhook.Data is null || webhook.Signature is null)
                return false;
            var props       = webhook.Signature.Properties;
            if (props.Count == 0) return false;
            var data = JsonSerializer.SerializeToElement(webhook.Data);
            var values = new List<string>();
            foreach (var property in props)
            {
                var value = data;
                foreach (var segment in property.Split('.'))
                {
                    if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(segment, out value))
                        return false;
                }
                if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Array)
                    return false;
                values.Add(value.ToString());
            }

            var rawString = string.Concat(values)
                            + webhook.Timestamp
                            + secret;

            var computed = ComputeSha256(rawString);
            return string.Equals(computed, webhook.Signature.Checksum, StringComparison.OrdinalIgnoreCase);
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
