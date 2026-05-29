using APIEccomerce.Controllers;
using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace APIEccomerce.Tests.Controllers
{
    public class AccessControllerTests
    {
        private readonly Mock<IAccessService> _serviceMock;
        private readonly AccessController _controller;

        public AccessControllerTests()
        {
            _serviceMock = new Mock<IAccessService>();
            _controller = new AccessController(_serviceMock.Object);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var userDto = new UserDto { Email = "test@test.com", Password = "123456" };
            var expectedMessage = "Usuario registrado exitosamente";
            _serviceMock.Setup(x => x.Register(userDto))
                .ReturnsAsync(expectedMessage);

            // Act
            var result = await _controller.Register(userDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<string>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task Register_ServiceThrowsException_ReturnsBadRequestWithError()
        {
            // Arrange
            var userDto = new UserDto { Email = "test@test.com", Password = "123456" };
            var errorMessage = "El email ya existe";
            _serviceMock.Setup(x => x.Register(userDto))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _controller.Register(userDto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeOfType<Response<string>>().Subject;
            response.IsSuccess.Should().BeFalse();
            response.Message.Should().Be(errorMessage);
        }

        

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "wrong@test.com", Password = "wrong" };
            _serviceMock.Setup(x => x.Login(loginDto))
                .ReturnsAsync((TokenDto)null!);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            var response = unauthorizedResult.Value.Should().BeOfType<Response<string>>().Subject;
            response.IsSuccess.Should().BeFalse();
            response.Message.Should().Be("Invalid credentials");
        }

        [Fact]
        public void ValidateToken_ValidToken_ReturnsOkWithTrue()
        {
            // Arrange
            var token = "valid-token";
            _serviceMock.Setup(x => x.ValidateToken(token))
                .Returns(true);

            // Act
            var result = _controller.ValidateToken(token);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<bool>>().Subject;
            response.IsSuccess.Should().BeTrue();
            response.Value.Should().BeTrue();
        }

        [Fact]
        public void ValidateToken_InvalidToken_ReturnsOkWithFalse()
        {
            // Arrange
            var token = "invalid-token";
            _serviceMock.Setup(x => x.ValidateToken(token))
                .Returns(false);

            // Act
            var result = _controller.ValidateToken(token);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<Response<bool>>().Subject;
            response.IsSuccess.Should().BeFalse();
            response.Value.Should().BeFalse();
        }
    }
}