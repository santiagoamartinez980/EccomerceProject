using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services;
using APIEccomerce.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace APIEccomerce.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepoMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly PaymentService _service;

        public PaymentServiceTests()
        {
            _paymentRepoMock = new Mock<IPaymentRepository>();
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderServiceMock = new Mock<IOrderService>();
            _configMock = new Mock<IConfiguration>();

            SetupConfig();

            _service = new PaymentService(
                _paymentRepoMock.Object,
                _orderRepoMock.Object,
                _orderServiceMock.Object,
                _configMock.Object);
        }

        private void SetupConfig()
        {
            _configMock.Setup(c => c["Wompi:IntegritySecret"])
                .Returns("secret123");

            _configMock.Setup(c => c["Wompi:EventSecret"])
                .Returns("eventsecret123");

            _configMock.Setup(c => c["Wompi:PublicKey"])
                .Returns("pub_key_123");

            _configMock.Setup(c => c["Frontend:Url"])
                .Returns("http://localhost:4200");
        }

        [Fact]
        public async Task CreateIntent_ReturnsPaymentIntentDto_WhenOrderValid()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                Status = OrderStatus.Pending,
                Total = 100000
            };

            _orderRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync(order);

            _paymentRepoMock
                .Setup(r => r.GetByOrderId(1))
                .ReturnsAsync((Payment?)null);

            _paymentRepoMock
                .Setup(r => r.Create(It.IsAny<Payment>()))
                .ReturnsAsync(new Payment());

            // Act
            var result = await _service.CreateIntent(1, 1);

            // Assert
            result.Should().NotBeNull();
            result.AmountInCents.Should().Be(10000000);
            result.Currency.Should().Be("COP");
            result.PublicKey.Should().Be("pub_key_123");
            result.Signature.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateIntent_ThrowsKeyNotFoundException_WhenOrderNotExists()
        {
            // Arrange
            _orderRepoMock
                .Setup(r => r.GetById(99))
                .ReturnsAsync((Order?)null);

            // Act
            Func<Task> act = async () =>
                await _service.CreateIntent(1, 99);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Pedido no encontrado*");
        }

        [Fact]
        public async Task CreateIntent_ThrowsUnauthorizedAccessException_WhenOrderNotBelongsToUser()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 1,
                UserId = 2,
                Status = OrderStatus.Pending,
                Total = 100000
            };

            _orderRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _service.CreateIntent(1, 1);

            // Assert
            await act.Should()
                .ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*no pertenece al usuario*");
        }

        [Fact]
        public async Task CreateIntent_ThrowsInvalidOperationException_WhenOrderAlreadyProcessed()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                Status = OrderStatus.Paid,
                Total = 100000
            };

            _orderRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _service.CreateIntent(1, 1);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*ya fue procesado*");
        }

        [Fact]
        public async Task ProcessWebhook_UpdatesPaymentStatus_WhenSignatureValid()
        {
            // Arrange
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var webhook = new WompiWebhookDto
            {
                Event = "transaction.updated",
                Timestamp = timestamp,
                Data = new WompiWebhookData
                {
                    Transaction = new WompiTransaction
                    {
                        Id = "trans_123",
                        Reference = "ORDER-1-123",
                        Status = "APPROVED",
                        AmountInCents = 10000000,
                        Currency = "COP",
                        PaymentMethodType = "CARD"
                    }
                },
                Signature = new WompiWebhookSignature
                {
                    Properties = new List<string>
                    {
                        "transaction.id",
                        "transaction.status",
                        "transaction.amount_in_cents"
                    },
                    Checksum = ComputeTestChecksum(
                        timestamp,
                        "trans_123",
                        "APPROVED",
                        "10000000")
                }
            };

            var payment = new Payment
            {
                PaymentId = 1,
                OrderId = 1
            };

            _paymentRepoMock
                .Setup(r => r.UpdateStatus(
                    "ORDER-1-123",
                    PaymentStatus.Approved,
                    "trans_123",
                    "CARD",
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(payment);

            _paymentRepoMock
                .Setup(r => r.GetByReference("ORDER-1-123"))
                .ReturnsAsync(payment);

            _orderServiceMock
                .Setup(r => r.ConfirmPayment(1))
                .ReturnsAsync(new OrderDto());

            // Act
            await _service.ProcessWebhook(webhook);

            // Assert
            _paymentRepoMock.Verify(
                r => r.UpdateStatus(
                    "ORDER-1-123",
                    PaymentStatus.Approved,
                    "trans_123",
                    "CARD",
                    It.IsAny<DateTime?>()),
                Times.Once);

            _orderServiceMock.Verify(
                r => r.ConfirmPayment(1),
                Times.Once);
        }

        [Fact]
        public async Task ProcessWebhook_IgnoresEvent_WhenNotTransactionUpdated()
        {
            // Arrange
            var webhook = new WompiWebhookDto
            {
                Event = "other.event",
                Data = new WompiWebhookData()
            };

            // Act
            await _service.ProcessWebhook(webhook);

            // Assert
            _paymentRepoMock.Verify(
                r => r.UpdateStatus(
                    It.IsAny<string>(),
                    It.IsAny<PaymentStatus>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>()),
                Times.Never);
        }


        [Fact]
        public async Task ProcessWebhook_ThrowsUnauthorizedAccessException_WhenSignatureInvalid()
        {
            // Arrange
            var webhook = new WompiWebhookDto
            {
                Event = "transaction.updated",
                Data = new WompiWebhookData(),
                Signature = new WompiWebhookSignature
                {
                    Checksum = "invalid_checksum"
                }
            };

            // Act
            Func<Task> act = async () =>
                await _service.ProcessWebhook(webhook);

            // Assert
            await act.Should()
                .ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*inválida*");
        }

        [Fact]
        public async Task ProcessWebhook_MapsStatusCorrectly_FromDeclined()
        {
            // Arrange
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var webhook = new WompiWebhookDto
            {
                Event = "transaction.updated",
                Timestamp = timestamp,
                Data = new WompiWebhookData
                {
                    Transaction = new WompiTransaction
                    {
                        Id = "trans_456",
                        Reference = "ORDER-2-456",
                        Status = "DECLINED",
                        AmountInCents = 5000000
                    }
                },
                Signature = new WompiWebhookSignature
                {
                    Properties = new List<string>
                    {
                        "transaction.id",
                        "transaction.status",
                        "transaction.amount_in_cents"
                    },
                    Checksum = ComputeTestChecksum(
                        timestamp,
                        "trans_456",
                        "DECLINED",
                        "5000000")
                }
            };

            _paymentRepoMock
                .Setup(r => r.UpdateStatus(
                    "ORDER-2-456",
                    PaymentStatus.Declined,
                    "trans_456",
                    It.IsAny<string>(),
                    null))
                .ReturnsAsync(new Payment());

            // Act
            await _service.ProcessWebhook(webhook);

            // Assert
            _paymentRepoMock.Verify(
                r => r.UpdateStatus(
                    "ORDER-2-456",
                    PaymentStatus.Declined,
                    "trans_456",
                    It.IsAny<string>(),
                    null),
                Times.Once);
        }

        private string ComputeTestChecksum(
            long timestamp,
            string? id = null,
            string? status = null,
            string? amount = null)
        {
            id ??= "trans_123";
            status ??= "APPROVED";
            amount ??= "10000000";

            var secret = "eventsecret123";

            var rawString =
                id +
                status +
                amount +
                timestamp +
                secret;

            using var sha = SHA256.Create();

            var hash = sha.ComputeHash(
                Encoding.UTF8.GetBytes(rawString));

            return Convert.ToHexString(hash).ToLower();
        }
    }
}