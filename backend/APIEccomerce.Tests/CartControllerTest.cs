using System.Security.Claims;
using APIEccomerce.Controllers;
using APIEccomerce.Interfaces;
using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace APIEccomerce.Tests.Controllers
{
    public class CartControllerTests
    {
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly CartController _controller;

        public CartControllerTests()
        {
            _cartServiceMock = new Mock<ICartService>();
            _controller = new CartController(_cartServiceMock.Object);

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
        public async Task GetCart_ReturnsOkWithCart()
        {
            // Arrange
            var cartDto = new CartDto { Id = 1, Total = 100, Items = new List<CartItemDto>() };
            _cartServiceMock.Setup(x => x.GetOrCreateCartAsync(1))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.GetCart();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<CartDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().Be(cartDto);
        }

        [Fact]
        public async Task AddOrUpdateItem_ValidProduct_ReturnsOk()
        {
            // Arrange
            var request = new CartItemRequest(1, 2);
            var cartDto = new CartDto { Id = 1, Total = 200 };
            _cartServiceMock.Setup(x => x.AddOrUpdateItemAsync(1, request.ProductId, request.Quantity))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.AddOrUpdateItem(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<CartDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().Be(cartDto);
        }

        [Fact]
        public async Task AddOrUpdateItem_ProductNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new CartItemRequest(999, 1);
            _cartServiceMock.Setup(x => x.AddOrUpdateItemAsync(1, request.ProductId, request.Quantity))
                .ThrowsAsync(new KeyNotFoundException("Producto no encontrado"));

            // Act
            var result = await _controller.AddOrUpdateItem(request);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var response = notFoundResult.Value.Should().BeOfType<Response<object>>().Subject;
            response.IsSuccess.Should().BeFalse();
            response.Message.Should().Be("Producto no encontrado");
        }

        [Fact]
        public async Task RemoveItem_ValidProduct_ReturnsOk()
        {
            // Arrange
            var productId = 1;
            var cartDto = new CartDto { Id = 1, Total = 50 };
            _cartServiceMock.Setup(x => x.RemoveItemAsync(1, productId))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.RemoveItem(productId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<CartDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().Be(cartDto);
        }

        [Fact]
        public async Task ClearCart_ReturnsOk()
        {
            // Arrange
            _cartServiceMock.Setup(x => x.ClearCartAsync(1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ClearCart();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<object>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Message.Should().Be("Carrito vaciado correctamente");
        }
    }
}