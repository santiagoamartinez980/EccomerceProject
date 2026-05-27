using APIEccomerce.Custom;
using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services;
using FluentAssertions;
using Moq;

namespace APIEccomerce.Tests
{
    public class AccessServiceTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<IUtilities> _utilsMock;
        private readonly AccessService _service;

        public AccessServiceTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _utilsMock = new Mock<IUtilities>();
            _service = new AccessService(_repoMock.Object, _utilsMock.Object);
        }

        // =====================
        // Register
        // =====================

        [Fact]
        public async Task Register_ReturnsSuccessMessage_WhenEmailNotExists()
        {
            // Arrange
            var dto = new UserDto { FirstName = "Juan", LastName = "Perez", Email = "juan@test.com", Password = "123456" };
            _repoMock.Setup(r => r.ExistsByEmail(dto.Email)).ReturnsAsync(false);
            _utilsMock.Setup(u => u.HashPassword(dto.Password)).Returns("hashed123");
            _repoMock.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync(new User());

            // Act
            var result = await _service.Register(dto);

            // Assert
            result.Should().Be("User registered successfully");
        }

        [Fact]
        public async Task Register_ThrowsException_WhenEmailAlreadyExists()
        {
            // Arrange
            var dto = new UserDto { FirstName = "Juan", LastName = "Perez", Email = "juan@test.com", Password = "123456" };
            _repoMock.Setup(r => r.ExistsByEmail(dto.Email)).ReturnsAsync(true);

            // Act
            var act = async () => await _service.Register(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Email already exists");
        }

        [Fact]
        public async Task Register_CallsHashPassword_WithCorrectPassword()
        {
            // Arrange
            var dto = new UserDto { FirstName = "Juan", LastName = "Perez", Email = "juan@test.com", Password = "123456" };
            _repoMock.Setup(r => r.ExistsByEmail(dto.Email)).ReturnsAsync(false);
            _utilsMock.Setup(u => u.HashPassword(dto.Password)).Returns("hashed123");
            _repoMock.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync(new User());

            // Act
            await _service.Register(dto);

            // Assert
            _utilsMock.Verify(u => u.HashPassword("123456"), Times.Once);
        }

        // =====================
        // Login
        // =====================

        [Fact]
        public async Task Login_ReturnsToken_WhenCredentialsAreValid()
        {
            // Arrange
            var dto = new LoginDto { Email = "juan@test.com", Password = "123456" };
            var user = new User { UserId = 1, Email = "juan@test.com", Password = "hashed123", Role = Role.User };

            _repoMock.Setup(r => r.GetByEmail(dto.Email)).ReturnsAsync(user);
            _utilsMock.Setup(u => u.VerifyPassword(dto.Password, user.Password)).Returns(true);
            _utilsMock.Setup(u => u.GenerateJwt(user)).Returns("jwt-token-fake");

            // Act
            var result = await _service.Login(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().Be("jwt-token-fake");
        }

        [Fact]
        public async Task Login_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var dto = new LoginDto { Email = "noexiste@test.com", Password = "123456" };
            _repoMock.Setup(r => r.GetByEmail(dto.Email)).ReturnsAsync((User?)null);

            // Act
            var result = await _service.Login(dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Login_ReturnsNull_WhenPasswordIsInvalid()
        {
            // Arrange
            var dto = new LoginDto { Email = "juan@test.com", Password = "wrongpassword" };
            var user = new User { UserId = 1, Email = "juan@test.com", Password = "hashed123", Role = Role.User };

            _repoMock.Setup(r => r.GetByEmail(dto.Email)).ReturnsAsync(user);
            _utilsMock.Setup(u => u.VerifyPassword(dto.Password, user.Password)).Returns(false);

            // Act
            var result = await _service.Login(dto);

            // Assert
            result.Should().BeNull();
        }

        // =====================
        // ValidateToken
        // =====================

        [Fact]
        public void ValidateToken_ReturnsTrue_WhenTokenIsValid()
        {
            // Arrange
            _utilsMock.Setup(u => u.ValidateToken("valid-token")).Returns(true);

            // Act
            var result = _service.ValidateToken("valid-token");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateToken_ReturnsFalse_WhenTokenIsInvalid()
        {
            // Arrange
            _utilsMock.Setup(u => u.ValidateToken("invalid-token")).Returns(false);

            // Act
            var result = _service.ValidateToken("invalid-token");

            // Assert
            result.Should().BeFalse();
        }
    }
}