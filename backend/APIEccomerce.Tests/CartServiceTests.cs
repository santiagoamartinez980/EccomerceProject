using APIEccomerce.Interfaces;
using APIEccomerce.Models;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services;
using FluentAssertions;
using Moq;

namespace APIEccomerce.Tests
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _cartRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly CartService _service;

        public CartServiceTests()
        {
            _cartRepoMock = new Mock<ICartRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _service = new CartService(_cartRepoMock.Object, _productRepoMock.Object);
        }

        [Fact]
        public async Task GetOrCreateCartAsync_ReturnsCartDto_WhenCartExists()
        {
            // Arrange
            var cart = new Cart
            {
                Id = 1,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Items = new List<CartItem>()
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(1))
                .ReturnsAsync(cart);

            // Act
            var result = await _service.GetOrCreateCartAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.UserId.Should().Be(1);
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetOrCreateCartAsync_CreatesCart_WhenNoCartExists()
        {
            // Arrange
            var newCart = new Cart
            {
                Id = 2,
                UserId = 2,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Items = new List<CartItem>()
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(2))
                .ReturnsAsync((Cart?)null);

            _cartRepoMock
                .Setup(r => r.CreateCartAsync(2))
                .ReturnsAsync(newCart);

            // Act
            var result = await _service.GetOrCreateCartAsync(2);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(2);

            _cartRepoMock.Verify(r => r.CreateCartAsync(2), Times.Once);
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_AddsNewItem_WhenItemDoesNotExist()
        {
            // Arrange
            var cart = new Cart
            {
                Id = 1,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Items = new List<CartItem>()
            };

            var product = new Product
            {
                ProductId = 1,
                Name = "Laptop",
                Price = 2500000,
                Stock = 10,
                IsActive = true
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(1))
                .ReturnsAsync(cart);

            _productRepoMock
                .Setup(r => r.GetByIdPublic(1))
                .ReturnsAsync(product);

            _cartRepoMock
                .Setup(r => r.GetCartItemAsync(1, 1))
                .ReturnsAsync((CartItem?)null);

            _cartRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var updatedCart = new Cart
            {
                Id = 1,
                UserId = 1,
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        CartItemId = 1,
                        CartId = 1,
                        ProductId = 1,
                        Quantity = 2,
                        UnitPrice = 2500000,
                        Product = product
                    }
                }
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(1))
                .ReturnsAsync(updatedCart);

            // Act
            var result = await _service.AddOrUpdateItemAsync(1, 1, 2);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);

            _cartRepoMock.Verify(
                r => r.AddItemAsync(It.IsAny<CartItem>()),
                Times.Once
            );
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_ThrowsKeyNotFoundException_WhenProductNotExists()
        {
            // Arrange
            var cart = new Cart
            {
                Id = 1,
                UserId = 1,
                Items = new List<CartItem>()
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(1))
                .ReturnsAsync(cart);

            _productRepoMock
                .Setup(r => r.GetByIdPublic(99))
                .ReturnsAsync((Product?)null);

            // Act
            Func<Task> act = async () =>
                await _service.AddOrUpdateItemAsync(1, 99, 2);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_ThrowsInvalidOperationException_WhenProductInactive()
        {
            // Arrange
            var cart = new Cart
            {
                Id = 1,
                UserId = 1,
                Items = new List<CartItem>()
            };

            var product = new Product
            {
                ProductId = 1,
                Name = "Laptop",
                Price = 2500000,
                Stock = 10,
                IsActive = false
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(1))
                .ReturnsAsync(cart);

            _productRepoMock
                .Setup(r => r.GetByIdPublic(1))
                .ReturnsAsync(product);

            // Act
            Func<Task> act = async () =>
                await _service.AddOrUpdateItemAsync(1, 1, 2);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*no está disponible*");
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_ThrowsInvalidOperationException_WhenInsufficientStock()
        {
            // Arrange
            var cart = new Cart
            {
                Id = 1,
                UserId = 1,
                Items = new List<CartItem>()
            };

            var product = new Product
            {
                ProductId = 1,
                Name = "Laptop",
                Price = 2500000,
                Stock = 1,
                IsActive = true
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(1))
                .ReturnsAsync(cart);

            _productRepoMock
                .Setup(r => r.GetByIdPublic(1))
                .ReturnsAsync(product);

            // Act
            Func<Task> act = async () =>
                await _service.AddOrUpdateItemAsync(1, 1, 2);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task RemoveItemAsync_RemovesItem_WhenItemExists()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                Name = "Laptop",
                Price = 2500000
            };

            var cartItem = new CartItem
            {
                CartItemId = 1,
                CartId = 1,
                ProductId = 1,
                Quantity = 2,
                Product = product
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = 1,
                Items = new List<CartItem> { cartItem }
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(1))
                .ReturnsAsync(cart);

            _cartRepoMock
                .Setup(r => r.GetCartItemAsync(1, 1))
                .ReturnsAsync(cartItem);

            _cartRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var emptyCart = new Cart
            {
                Id = 1,
                UserId = 1,
                Items = new List<CartItem>()
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(1))
                .ReturnsAsync(emptyCart);

            // Act
            var result = await _service.RemoveItemAsync(1, 1);

            // Assert
            result.Should().NotBeNull();

            _cartRepoMock.Verify(
                r => r.RemoveItemAsync(cartItem),
                Times.Once
            );
        }

        [Fact]
        public async Task ClearCartAsync_ClearsAllItems_WhenCartExists()
        {
            // Arrange
            var cart = new Cart
            {
                Id = 1,
                UserId = 1,
                Items = new List<CartItem>()
            };

            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(1))
                .ReturnsAsync(cart);

            _cartRepoMock
                .Setup(r => r.ClearCartAsync(1))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ClearCartAsync(1);

            // Assert
            _cartRepoMock.Verify(
                r => r.ClearCartAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task ClearCartAsync_ThrowsInvalidOperationException_WhenCartNotExists()
        {
            // Arrange
            _cartRepoMock
                .Setup(r => r.GetActiveCartByUserIdAsync(99))
                .ReturnsAsync((Cart?)null);

            // Act
            Func<Task> act = async () =>
                await _service.ClearCartAsync(99);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*Carrito no encontrado*");
        }
    }
}