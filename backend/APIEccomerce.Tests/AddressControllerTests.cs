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
    public class AddressControllerTests
    {
        private readonly Mock<IAddressService> _serviceMock;
        private readonly AddressController _controller;

        public AddressControllerTests()
        {
            _serviceMock = new Mock<IAddressService>();
            _controller = new AddressController(_serviceMock.Object);

            // Setup User claims
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
        public async Task GetAll_ReturnsOkWithAddresses()
        {
            // Arrange
            var addresses = new List<AddressDto>
            {
                new AddressDto { AddressId = 1, AddressLine = "Calle 123" },
                new AddressDto { AddressId = 2, AddressLine = "Avenida 456" }
            };
            _serviceMock.Setup(x => x.GetByUserId(1))
                .ReturnsAsync(addresses);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<List<AddressDto>>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().BeEquivalentTo(addresses);
        }

        [Fact]
        public async Task GetDefault_AddressExists_ReturnsOk()
        {
            // Arrange
            var address = new AddressDto { AddressId = 1, AddressLine = "Calle 123", IsDefault = true };
            _serviceMock.Setup(x => x.GetDefaultAddress(1))
                .ReturnsAsync(address);

            // Act
            var result = await _controller.GetDefault();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<AddressDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().Be(address);
        }

        [Fact]
        public async Task GetDefault_NoDefaultAddress_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.Setup(x => x.GetDefaultAddress(1))
                .ReturnsAsync((AddressDto)null!);

            // Act
            var result = await _controller.GetDefault();

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var response = notFoundResult.Value.Should().BeOfType<Response<object>>().Subject;
            response.IsSuccess.Should().BeFalse();
            response.Message.Should().Be("No hay dirección predeterminada");
        }

        [Fact]
        public async Task Create_ValidAddress_ReturnsOk()
        {
            // Arrange
            var createDto = new CreateAddressDto { AddressLine = "Nueva Calle", City = "Bogotá" };
            var createdAddress = new AddressDto { AddressId = 1, AddressLine = "Nueva Calle" };
            _serviceMock.Setup(x => x.Create(1, createDto))
                .ReturnsAsync(createdAddress);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<AddressDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().Be(createdAddress);
        }

        [Fact]
        public async Task Update_ExistingAddress_ReturnsOk()
        {
            // Arrange
            var addressId = 1;
            var updateDto = new CreateAddressDto { AddressLine = "Calle Actualizada" };
            var updatedAddress = new AddressDto { AddressId = addressId, AddressLine = "Calle Actualizada" };
            _serviceMock.Setup(x => x.Update(1, addressId, updateDto))
                .ReturnsAsync(updatedAddress);

            // Act
            var result = await _controller.Update(addressId, updateDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<AddressDto>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().Be(updatedAddress);
        }

        [Fact]
        public async Task Update_NonExistingAddress_ReturnsNotFound()
        {
            // Arrange
            var addressId = 999;
            var updateDto = new CreateAddressDto { AddressLine = "Calle Actualizada" };
            _serviceMock.Setup(x => x.Update(1, addressId, updateDto))
                .ReturnsAsync((AddressDto)null!);

            // Act
            var result = await _controller.Update(addressId, updateDto);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var response = notFoundResult.Value.Should().BeOfType<Response<object>>().Subject;
            response.IsSuccess.Should().BeFalse();
            response.Message.Should().Be("Dirección no encontrada");
        }

        [Fact]
        public async Task Delete_ExistingAddress_ReturnsOk()
        {
            // Arrange
            var addressId = 1;
            _serviceMock.Setup(x => x.Delete(1, addressId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(addressId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<object>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Message.Should().Be("Dirección eliminada correctamente");
        }

        [Fact]
        public async Task Delete_NonExistingAddress_ReturnsNotFound()
        {
            // Arrange
            var addressId = 999;
            _serviceMock.Setup(x => x.Delete(1, addressId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(addressId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var response = notFoundResult.Value.Should().BeOfType<Response<object>>().Subject;
            response.IsSuccess.Should().BeFalse();
            response.Message.Should().Be("Dirección no encontrada");
        }
    }
}