using APIEccomerce.Models;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIEccomerce.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _service = new ProductService(_repoMock.Object);
        }

        // ListIsActive

        [Fact]
        public async Task ListIsActive_ReturnsListOfProductResponseDto_WhenActiveProductsExist()
        {
            // Arrange
            var products = new List<Product>
        {
            new Product
            {
                ProductId = 1,
                Name = "Laptop",
                Description = "Laptop gamer",
                Price = 2500000,
                Stock = 10,
                ImageUrl = "url1",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                CategoryId = 1,
                Category = new Category { Name = "Electrónica" }
            },
            new Product
            {
                ProductId = 2,
                Name = "Teclado",
                Description = "Teclado mecánico",
                Price = 350000,
                Stock = 25,
                ImageUrl = "url2",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                CategoryId = 1,
                Category = new Category { Name = "Electrónica" }
            }
        };
            _repoMock.Setup(r => r.ListIsActive()).ReturnsAsync(products);

            // Act
            var result = await _service.ListIsActive();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Laptop");
            result[0].CategoryName.Should().Be("Electrónica");
            result[1].Name.Should().Be("Teclado");
        }

        [Fact]
        public async Task ListIsActive_ReturnsEmptyList_WhenNoActiveProductsExist()
        {
            // Arrange
            _repoMock.Setup(r => r.ListIsActive()).ReturnsAsync(new List<Product>());

            // Act
            var result = await _service.ListIsActive();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ListIsActive_MapsFieldsCorrectly_WhenProductHasCategory()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var products = new List<Product>
        {
            new Product
            {
                ProductId = 5,
                Name = "Mouse",
                Description = "Mouse inalámbrico",
                Price = 120000,
                Stock = 50,
                ImageUrl = "urlMouse",
                IsActive = true,
                CreatedAt = createdAt,
                CategoryId = 2,
                Category = new Category { Name = "Periféricos" }
            }
        };
            _repoMock.Setup(r => r.ListIsActive()).ReturnsAsync(products);

            // Act
            var result = await _service.ListIsActive();
            var dto = result[0];

            // Assert
            dto.Id.Should().Be(5);
            dto.Name.Should().Be("Mouse");
            dto.Description.Should().Be("Mouse inalámbrico");
            dto.Price.Should().Be(120000);
            dto.Stock.Should().Be(50);
            dto.ImageUrl.Should().Be("urlMouse");
            dto.IsActive.Should().BeTrue();
            dto.CreatedAt.Should().Be(createdAt);
            dto.CategoryId.Should().Be(2);
            dto.CategoryName.Should().Be("Periféricos");
        }

        [Fact]

        //getByIdPublic

        public async Task GetByIdPublic_ReturnsProductResponseDto_WhenProductExists()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var product = new Product
            {
                ProductId = 1,
                Name = "Laptop",
                Description = "Laptop gamer",
                Price = 2500000,
                Stock = 10,
                ImageUrl = "urlLaptop",
                IsActive = true,
                CreatedAt = createdAt,
                CategoryId = 1,
                Category = new Category { Name = "Electrónica" }
            };
            _repoMock.Setup(r => r.GetByIdPublic(1)).ReturnsAsync(product);

            // Act
            var result = await _service.GetByIdPublic(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Laptop");
            result.CategoryName.Should().Be("Electrónica");
        }

        [Fact]
        public async Task GetByIdPublic_ReturnsNull_WhenProductDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdPublic(99)).ReturnsAsync((Product?)null);

            // Act
            var result = await _service.GetByIdPublic(99);

            // Assert
            result.Should().BeNull();
        }



    }
   
}

