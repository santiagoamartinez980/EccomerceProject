using System.Security.Claims;
using APIEccomerce.Controllers;
using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using System.Text;

namespace APIEccomerce.Tests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _serviceMock;
        private readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _serviceMock = new Mock<IPaymentService>();
            _controller = new PaymentController(_serviceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        

        [Fact]
        public async Task Webhook_ValidWebhook_ReturnsOk()
        {
            // Arrange
            var webhookJson = @"
            {
                ""event"": ""transaction.updated"",
                ""data"": { ""id"": ""tx_123"", ""status"": ""APPROVED"" }
            }";
            var requestBody = new MemoryStream(Encoding.UTF8.GetBytes(webhookJson));
            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                Request = { Body = requestBody }
            };
            _serviceMock.Setup(x => x.ProcessWebhook(It.IsAny<WompiWebhookDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Webhook();

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        
    }
}