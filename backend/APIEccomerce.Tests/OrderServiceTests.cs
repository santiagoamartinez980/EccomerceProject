using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services;
using FluentAssertions;
using Moq;

namespace APIEccomerce.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IAddressRepository> _addressRepoMock;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _addressRepoMock = new Mock<IAddressRepository>();
            _service = new OrderService(_orderRepoMock.Object, _productRepoMock.Object, _addressRepoMock.Object);
        }

        [Fact]
        public async Task GetByUserId_ReturnsOrderList_WhenOrdersExist()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = 1,
                    UserId = 1,
                    Status = OrderStatus.Pending,
                    Total = 2500000,
                    CreatedAt = DateTime.UtcNow,
                    Address = new Address { AddressId = 1, AddressLine = "Calle 1" },
                    Details = new List<OrderDetail>()
                }
            };
            _orderRepoMock.Setup(r => r.GetByUserId(1)).ReturnsAsync(orders);

            // Act
            var result = await _service.GetByUserId(1);

            // Assert
            result.Should().HaveCount(1);
            result[0].OrderId.Should().Be(1);
            result[0].Status.Should().Be("Pending");
        }

        [Fact]
        public async Task GetById_ReturnsOrderDto_WhenOrderBelongsToUser()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                Status = OrderStatus.Pending,
                Total = 2500000,
                Address = new Address { AddressId = 1, AddressLine = "Calle 1" },
                Details = new List<OrderDetail>()
            };
            _orderRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(order);

            // Act
            var result = await _service.GetById(1, 1);

            // Assert
            result.Should().NotBeNull();
            result!.OrderId.Should().Be(1);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenOrderNotBelongsToUser()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 1,
                UserId = 2,
                Status = OrderStatus.Pending,
                Total = 2500000,
                Address = new Address { AddressId = 1, AddressLine = "Calle 1" },
                Details = new List<OrderDetail>()
            };
            _orderRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(order);

            // Act
            var result = await _service.GetById(1, 1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Create_ReturnsOrderDto_WhenValidDataProvided()
        {
            // Arrange
            var address = new Address { AddressId = 1, UserId = 1, AddressLine = "Calle 1" };
            var product = new Product { ProductId = 1, Name = "Laptop", Price = 2500000, Stock = 10, IsActive = true };
            var dto = new CreateOrderDto
            {
                AddressId = 1,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 2 }
                }
            };

            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                AddressId = 1,
                Status = OrderStatus.Pending,
                Total = 5000000,
                Address = address,
                Details = new List<OrderDetail>
                {
                    new OrderDetail { OrderDetailId = 1, ProductId = 1, Quantity = 2, UnitPrice = 2500000, Product = product }
                }
            };

            _addressRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(address);
            _productRepoMock.Setup(r => r.GetByIdPublic(1)).ReturnsAsync(product);
            _orderRepoMock.Setup(r => r.Create(It.IsAny<Order>())).ReturnsAsync(order);
            _orderRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(order);

            // Act
            var result = await _service.Create(1, dto);

            // Assert
            result.Should().NotBeNull();
            result.Total.Should().Be(5000000);
            result.Details.Should().HaveCount(1);
        }

        [Fact]
        public async Task Create_ThrowsKeyNotFoundException_WhenAddressNotExists()
        {
            // Arrange
            var dto = new CreateOrderDto { AddressId = 99, Items = new List<OrderItemDto>() };
            _addressRepoMock.Setup(r => r.GetById(99)).ReturnsAsync((Address?)null);

            // Act & Assert
            var action = async () => await _service.Create(1, dto);
            await action.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Create_ThrowsUnauthorizedAccessException_WhenAddressNotBelongsToUser()
        {
            // Arrange
            var address = new Address { AddressId = 1, UserId = 2, AddressLine = "Calle 1" };
            var dto = new CreateOrderDto { AddressId = 1, Items = new List<OrderItemDto>() };

            _addressRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(address);

            // Act & Assert
            var action = async () => await _service.Create(1, dto);
            await action.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task Create_ThrowsInvalidOperationException_WhenProductInactive()
        {
            // Arrange
            var address = new Address { AddressId = 1, UserId = 1, AddressLine = "Calle 1" };
            var product = new Product { ProductId = 1, Name = "Laptop", Price = 2500000, Stock = 10, IsActive = false };
            var dto = new CreateOrderDto
            {
                AddressId = 1,
                Items = new List<OrderItemDto> { new OrderItemDto { ProductId = 1, Quantity = 2 } }
            };

            _addressRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(address);
            _productRepoMock.Setup(r => r.GetByIdPublic(1)).ReturnsAsync(product);

            // Act & Assert
            var action = async () => await _service.Create(1, dto);
            await action.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Create_ThrowsInvalidOperationException_WhenInsufficientStock()
        {
            // Arrange
            var address = new Address { AddressId = 1, UserId = 1, AddressLine = "Calle 1" };
            var product = new Product { ProductId = 1, Name = "Laptop", Price = 2500000, Stock = 1, IsActive = true };
            var dto = new CreateOrderDto
            {
                AddressId = 1,
                Items = new List<OrderItemDto> { new OrderItemDto { ProductId = 1, Quantity = 5 } }
            };

            _addressRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(address);
            _productRepoMock.Setup(r => r.GetByIdPublic(1)).ReturnsAsync(product);

            // Act & Assert
            var action = async () => await _service.Create(1, dto);
            await action.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task UpdateStatus_UpdatesOrderStatus_WhenOrderExists()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 1,
                Status = OrderStatus.Pending,
                Address = new Address { AddressId = 1, AddressLine = "Calle 1" },
                Details = new List<OrderDetail>()
            };
            var updated = new Order
            {
                OrderId = 1,
                Status = OrderStatus.Paid,
                Address = new Address { AddressId = 1, AddressLine = "Calle 1" },
                Details = new List<OrderDetail>()
            };

            _orderRepoMock.Setup(r => r.UpdateStatus(1, OrderStatus.Paid)).ReturnsAsync(updated);

            // Act
            var result = await _service.UpdateStatus(1, OrderStatus.Paid);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be("Paid");
        }

        [Fact]
        public async Task Cancel_CancelsOrder_WhenOrderPending()
        {
            // Arrange
            var product = new Product { ProductId = 1, Name = "Laptop", Stock = 10 };
            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                Status = OrderStatus.Pending,
                Details = new List<OrderDetail>
                {
                    new OrderDetail { OrderDetailId = 1, ProductId = 1, Quantity = 2, Product = product }
                }
            };

            _orderRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(order);
            _productRepoMock.Setup(r => r.GetByIdPublic(1)).ReturnsAsync(product);
            _orderRepoMock.Setup(r => r.UpdateStatus(1, OrderStatus.Cancelled)).ReturnsAsync(order);

            // Act
            var result = await _service.Cancel(1, 1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Cancel_ReturnsFalse_WhenOrderNotBelongsToUser()
        {
            // Arrange
            var order = new Order { OrderId = 1, UserId = 2, Status = OrderStatus.Pending };
            _orderRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(order);

            // Act
            var result = await _service.Cancel(1, 1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Cancel_ThrowsInvalidOperationException_WhenOrderNotPending()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                Status = OrderStatus.Paid,
                Details = new List<OrderDetail>()
            };

            _orderRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(order);

            // Act & Assert
            var action = async () => await _service.Cancel(1, 1);
            await action.Should().ThrowAsync<InvalidOperationException>();
        }

       
        [Fact]
        public async Task ConfirmPayment_DecreasesStock_WhenPaymentApproved()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                Name = "Laptop",
                Stock = 10,
                Price = 2500000,
                IsActive = true
            };

            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                Status = OrderStatus.Pending,
                Total = 5000000,
                CreatedAt = DateTime.UtcNow,

                Address = new Address
                {
                    AddressId = 1,
                    AddressLine = "Calle 123",
                    City = "Bogotá",
                    Department = "Cundinamarca",
                    Country = "Colombia",
                    PostalCode = "111111",
                    Latitude = 0,
                    Longitude = 0,
                    Phone = "3000000000",
                    Notes = "",
                    FormattedAddress = "Calle 123 Bogotá",
                    IsDefault = true
                },

                Details = new List<OrderDetail>
        {
            new OrderDetail
            {
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 2500000,

                Product = product
            }
        }
            };

            var updatedOrder = new Order
            {
                OrderId = 1,
                UserId = 1,
                Status = OrderStatus.Paid,
                Total = 5000000,
                CreatedAt = DateTime.UtcNow,

                Address = new Address
                {
                    AddressId = 1,
                    AddressLine = "Calle 123",
                    City = "Bogotá",
                    Department = "Cundinamarca",
                    Country = "Colombia",
                    PostalCode = "111111",
                    Latitude = 0,
                    Longitude = 0,
                    Phone = "3000000000",
                    Notes = "",
                    FormattedAddress = "Calle 123 Bogotá",
                    IsDefault = true
                },

                Details = new List<OrderDetail>
        {
            new OrderDetail
            {
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 2500000,

                Product = product
            }
        }
            };

            _orderRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync(order);

            _productRepoMock
                .Setup(r => r.GetByIdPublic(1))
                .ReturnsAsync(product);

            _orderRepoMock
                .Setup(r => r.UpdateStatus(1, OrderStatus.Paid))
                .ReturnsAsync(updatedOrder);

            // Act
            var result = await _service.ConfirmPayment(1);

            // Assert
            result.Should().NotBeNull();

            product.Stock.Should().Be(8);

            _orderRepoMock.Verify(
                r => r.UpdateStatus(1, OrderStatus.Paid),
                Times.Once);
        }
    }
}
