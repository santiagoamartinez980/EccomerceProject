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

namespace APIEccomerce.Tests.Controllers
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _serviceMock;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _serviceMock = new Mock<IOrderService>();
            _controller = new OrderController(_serviceMock.Object);

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
        public async Task GetAll_ReturnsOkWithOrders()
        {
            // Arrange
            var orders = new List<OrderDto>
            {
                new OrderDto { OrderId = 1, Total = 100 },
                new OrderDto { OrderId = 2, Total = 200 }
            };
            _serviceMock.Setup(x => x.GetByUserId(1))
                .ReturnsAsync(orders);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<List<OrderDto>>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().BeEquivalentTo(orders);
        }

        [Fact]
        public async Task GetById_ExistingOrder_ReturnsOk()
        {
            // Arrange
            var orderId = 1;
            var order = new OrderDto { OrderId = orderId, Total = 100 };
            _serviceMock.Setup(x => x.GetById(1, orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.GetById(orderId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<OrderDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().Be(order);
        }

        [Fact]
        public async Task GetById_NonExistingOrder_ReturnsNotFound()
        {
            // Arrange
            var orderId = 999;
            _serviceMock.Setup(x => x.GetById(1, orderId))
                .ReturnsAsync((OrderDto)null!);

            // Act
            var result = await _controller.GetById(orderId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var response = notFoundResult.Value.Should().BeOfType<Response<object>>().Subject;
            response.IsSuccess.Should().BeFalse();
            response.Message.Should().Be("Pedido no encontrado");
        }

        [Fact]
        public async Task Cancel_ExistingOrder_ReturnsOk()
        {
            // Arrange
            var orderId = 1;
            _serviceMock.Setup(x => x.Cancel(1, orderId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Cancel(orderId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<object>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Message.Should().Be("Pedido cancelado correctamente");
        }

        [Fact]
        public async Task Cancel_NonExistingOrder_ReturnsNotFound()
        {
            // Arrange
            var orderId = 999;
            _serviceMock.Setup(x => x.Cancel(1, orderId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Cancel(orderId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var response = notFoundResult.Value.Should().BeOfType<Response<object>>().Subject;
            response.IsSuccess.Should().BeFalse();
            response.Message.Should().Be("Pedido no encontrado");
        }

        
    }
}