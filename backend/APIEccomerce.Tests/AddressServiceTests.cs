using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services;
using APIEccomerce.Services.Interfaces;
using FluentAssertions;
using Moq;

namespace APIEccomerce.Tests
{
    public class AddressServiceTests
    {
        private readonly Mock<IAddressRepository> _addressRepoMock;
        private readonly Mock<IGoogleGeocodingService> _geocodingMock;
        private readonly AddressService _service;

        public AddressServiceTests()
        {
            _addressRepoMock = new Mock<IAddressRepository>();
            _geocodingMock = new Mock<IGoogleGeocodingService>();
            _service = new AddressService(_addressRepoMock.Object, _geocodingMock.Object);
        }

        [Fact]
        public async Task GetByUserId_ReturnsAddressListDto_WhenAddressesExist()
        {
            // Arrange
            var addresses = new List<Address>
            {
                new Address
                {
                    AddressId = 1,
                    UserId = 1,
                    AddressLine = "Calle 1 #123",
                    City = "Bogotá",
                    Department = "Cundinamarca",
                    Country = "Colombia",
                    PostalCode = "110111",
                    Latitude = 4.7110,
                    Longitude = -74.0721,
                    Phone = "3001234567",
                    IsDefault = true
                }
            };

            _addressRepoMock.Setup(r => r.GetByUserId(1)).ReturnsAsync(addresses);

            // Act
            var result = await _service.GetByUserId(1);

            // Assert
            result.Should().HaveCount(1);
            result[0].AddressId.Should().Be(1);
            result[0].AddressLine.Should().Be("Calle 1 #123");
        }

        [Fact]
        public async Task GetByUserId_ReturnsEmptyList_WhenNoAddressesExist()
        {
            // Arrange
            _addressRepoMock.Setup(r => r.GetByUserId(99)).ReturnsAsync(new List<Address>());

            // Act
            var result = await _service.GetByUserId(99);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDefaultAddress_ReturnsDefaultAddress_WhenExists()
        {
            // Arrange
            var address = new Address
            {
                AddressId = 1,
                UserId = 1,
                AddressLine = "Calle 1 #123",
                City = "Bogotá",
                Department = "Cundinamarca",
                Country = "Colombia",
                PostalCode = "110111",
                Latitude = 4.7110,
                Longitude = -74.0721,
                Phone = "3001234567",
                IsDefault = true
            };

            _addressRepoMock.Setup(r => r.GetDefaultAddress(1)).ReturnsAsync(address);

            // Act
            var result = await _service.GetDefaultAddress(1);

            // Assert
            result.Should().NotBeNull();
            result!.AddressId.Should().Be(1);
            result.IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task GetDefaultAddress_ReturnsNull_WhenNoDefaultExists()
        {
            // Arrange
            _addressRepoMock.Setup(r => r.GetDefaultAddress(99)).ReturnsAsync((Address?)null);

            // Act
            var result = await _service.GetDefaultAddress(99);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Create_ReturnsAddressDto_WhenValidDataProvided()
        {
            // Arrange
            var dto = new CreateAddressDto
            {
                AddressLine = "Calle 1 #123",
                City = "Bogotá",
                Department = "Cundinamarca",
                Country = "Colombia",
                PostalCode = "110111",
                Latitude = 4.7110,
                Longitude = -74.0721,
                Phone = "3001234567",
                IsDefault = false
            };

            var address = new Address
            {
                AddressId = 1,
                UserId = 1,
                AddressLine = dto.AddressLine,
                City = dto.City,
                Department = dto.Department,
                Country = dto.Country,
                PostalCode = dto.PostalCode,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Phone = dto.Phone,
                IsDefault = true,
                FormattedAddress = "Formatted Address"
            };

            _addressRepoMock.Setup(r => r.GetByUserId(1)).ReturnsAsync(new List<Address>());
            _addressRepoMock.Setup(r => r.Create(It.IsAny<Address>())).ReturnsAsync(address);

            // Act
            var result = await _service.Create(1, dto);

            // Assert
            result.Should().NotBeNull();
            result.AddressLine.Should().Be("Calle 1 #123");
            result.City.Should().Be("Bogotá");
        }

        [Fact]
        public async Task Create_CallsGeocodingService_WhenCoordinatesAreZero()
        {
            // Arrange
            var dto = new CreateAddressDto
            {
                AddressLine = "Calle 2 #456",
                City = "Medellín",
                Department = "Antioquia",
                Country = "Colombia",
                PostalCode = "050001",
                Latitude = 0,
                Longitude = 0,
                Phone = "3009876543",
                IsDefault = false
            };

            var geoResult = new GeocodingResultDto
            {
                Latitude = 6.2442,
                Longitude = -75.5812,
                FormattedAddress = "Medellín, Antioquia",
                PostalCode = "050001",
                PlaceId = "place123"
            };

            var address = new Address
            {
                AddressId = 2,
                UserId = 1,
                AddressLine = dto.AddressLine,
                City = dto.City,
                Department = dto.Department,
                Country = dto.Country,
                PostalCode = geoResult.PostalCode,
                Latitude = geoResult.Latitude,
                Longitude = geoResult.Longitude,
                Phone = dto.Phone,
                IsDefault = true,
                FormattedAddress = geoResult.FormattedAddress,
                PlaceId = geoResult.PlaceId
            };

            _addressRepoMock.Setup(r => r.GetByUserId(1)).ReturnsAsync(new List<Address>());
            _geocodingMock.Setup(r => r.GeocodeAddressAsync(
                dto.AddressLine, dto.City, dto.Department))
                .ReturnsAsync(geoResult);
            _addressRepoMock.Setup(r => r.Create(It.IsAny<Address>())).ReturnsAsync(address);

            // Act
            await _service.Create(1, dto);

            // Assert
            _geocodingMock.Verify(r => r.GeocodeAddressAsync(
                dto.AddressLine, dto.City, dto.Department), Times.Once);
        }

        [Fact]
        public async Task Create_SetsDefaultTrue_WhenNoAddressesExistForUser()
        {
            // Arrange
            var dto = new CreateAddressDto
            {
                AddressLine = "Calle 3 #789",
                City = "Cali",
                Department = "Valle",
                Country = "Colombia",
                PostalCode = "760001",
                Latitude = 3.4372,
                Longitude = -76.5197,
                Phone = "3005555555",
                IsDefault = false
            };

            var address = new Address
            {
                AddressId = 3,
                UserId = 1,
                AddressLine = dto.AddressLine,
                City = dto.City,
                Department = dto.Department,
                Country = dto.Country,
                PostalCode = dto.PostalCode,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Phone = dto.Phone,
                IsDefault = true
            };

            _addressRepoMock.Setup(r => r.GetByUserId(1)).ReturnsAsync(new List<Address>());
            _addressRepoMock.Setup(r => r.Create(It.IsAny<Address>())).ReturnsAsync(address);

            // Act
            var result = await _service.Create(1, dto);

            // Assert
            result.IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task Update_ReturnsUpdatedAddressDto_WhenValidDataProvided()
        {
            // Arrange
            var existing = new Address
            {
                AddressId = 1,
                UserId = 1,
                AddressLine = "Calle 1 #123",
                City = "Bogotá",
                Department = "Cundinamarca",
                Country = "Colombia",
                PostalCode = "110111",
                Latitude = 4.7110,
                Longitude = -74.0721,
                Phone = "3001234567",
                IsDefault = false
            };

            var dto = new CreateAddressDto
            {
                AddressLine = "Calle 1 #999",
                City = "Bogotá Updated",
                Department = "Cundinamarca",
                Country = "Colombia",
                PostalCode = "110111",
                Latitude = 4.7110,
                Longitude = -74.0721,
                Phone = "3001234567",
                IsDefault = false
            };

            var updated = new Address
            {
                AddressId = 1,
                UserId = 1,
                AddressLine = dto.AddressLine,
                City = dto.City,
                Department = dto.Department,
                Country = dto.Country,
                PostalCode = dto.PostalCode,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Phone = dto.Phone,
                IsDefault = false
            };

            _addressRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(existing);
            _addressRepoMock.Setup(r => r.Update(It.IsAny<Address>())).ReturnsAsync(updated);

            // Act
            var result = await _service.Update(1, 1, dto);

            // Assert
            result.Should().NotBeNull();
            result!.AddressLine.Should().Be("Calle 1 #999");
            result.City.Should().Be("Bogotá Updated");
        }

        [Fact]
        public async Task Update_ReturnsNull_WhenAddressNotBelongsToUser()
        {
            // Arrange
            var existing = new Address { AddressId = 1, UserId = 2 };
            _addressRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(existing);

            // Act
            var result = await _service.Update(1, 1, new CreateAddressDto());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Update_ReturnsNull_WhenAddressNotExists()
        {
            // Arrange
            _addressRepoMock.Setup(r => r.GetById(99)).ReturnsAsync((Address?)null);

            // Act
            var result = await _service.Update(1, 99, new CreateAddressDto());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsTrue_WhenAddressDeletedByUser()
        {
            // Arrange
            var address = new Address { AddressId = 1, UserId = 1 };
            _addressRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(address);
            _addressRepoMock.Setup(r => r.Delete(1)).ReturnsAsync(true);

            // Act
            var result = await _service.Delete(1, 1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenAddressNotBelongsToUser()
        {
            // Arrange
            var address = new Address { AddressId = 1, UserId = 2 };
            _addressRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(address);

            // Act
            var result = await _service.Delete(1, 1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenAddressNotExists()
        {
            // Arrange
            _addressRepoMock.Setup(r => r.GetById(99)).ReturnsAsync((Address?)null);

            // Act
            var result = await _service.Delete(1, 99);

            // Assert
            result.Should().BeFalse();
        }
    }
}
