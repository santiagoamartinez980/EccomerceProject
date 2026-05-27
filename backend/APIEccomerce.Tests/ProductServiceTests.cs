using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
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

        // =====================
        // ListByCategory
        // =====================

        [Fact]
        public async Task ListByCategory_ReturnsProducts_WhenCategoryExists()
        {
            // Arrange
            var products = new List<Product>
    {
        new Product { ProductId = 1, Name = "Laptop", Price = 2500000, Stock = 10, IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1, Category = new Category { Name = "Electrónica" } },
        new Product { ProductId = 2, Name = "Mouse", Price = 120000, Stock = 50, IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1, Category = new Category { Name = "Electrónica" } }
    };
            _repoMock.Setup(r => r.ListByCategory("Electrónica")).ReturnsAsync(products);

            // Act
            var result = await _service.ListByCategory("Electrónica");

            // Assert
            result.Should().HaveCount(2);
            result[0].CategoryName.Should().Be("Electrónica");
        }

        [Fact]
        public async Task ListByCategory_ReturnsEmptyList_WhenCategoryHasNoProducts()
        {
            // Arrange
            _repoMock.Setup(r => r.ListByCategory("Vacia")).ReturnsAsync(new List<Product>());

            // Act
            var result = await _service.ListByCategory("Vacia");

            // Assert
            result.Should().BeEmpty();
        }

        // =====================
        // SearchByName
        // =====================

        [Fact]
        public async Task SearchByName_ReturnsProducts_WhenNameMatches()
        {
            // Arrange
            var products = new List<Product>
    {
        new Product { ProductId = 1, Name = "Laptop Gamer", Price = 2500000, Stock = 10, IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1, Category = new Category { Name = "Electrónica" } }
    };
            _repoMock.Setup(r => r.SearchByName("Laptop")).ReturnsAsync(products);

            // Act
            var result = await _service.SearchByName("Laptop");

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Laptop Gamer");
        }

        [Fact]
        public async Task SearchByName_ReturnsEmptyList_WhenNoProductMatches()
        {
            // Arrange
            _repoMock.Setup(r => r.SearchByName("XYZ")).ReturnsAsync(new List<Product>());

            // Act
            var result = await _service.SearchByName("XYZ");

            // Assert
            result.Should().BeEmpty();
        }

        // =====================
        // GetByIdAdmin
        // =====================

        [Fact]
        public async Task GetByIdAdmin_ReturnsProduct_WhenExists()
        {
            // Arrange
            var product = new Product { ProductId = 1, Name = "Laptop", Price = 2500000, Stock = 10, IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1, Category = new Category { Name = "Electrónica" } };
            _repoMock.Setup(r => r.GetByIdAdmin(1)).ReturnsAsync(product);

            // Act
            var result = await _service.GetByIdAdmin(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Laptop");
        }

        [Fact]
        public async Task GetByIdAdmin_ReturnsNull_WhenNotExists()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAdmin(99)).ReturnsAsync((Product?)null);

            // Act
            var result = await _service.GetByIdAdmin(99);

            // Assert
            result.Should().BeNull();
        }

        // =====================
        // Create
        // =====================

        [Fact]
        public async Task Create_ReturnsProductResponseDto_WhenProductIsCreated()
        {
            // Arrange
            var dto = new CreateProductDto { Name = "Teclado", Description = "Mecánico", Price = 350000, Stock = 20, ImageUrl = "url", IsActive = true, CategoryId = 1 };
            var created = new Product { ProductId = 3, Name = "Teclado", Description = "Mecánico", Price = 350000, Stock = 20, ImageUrl = "url", IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1, Category = new Category { Name = "Electrónica" } };
            _repoMock.Setup(r => r.Create(It.IsAny<Product>())).ReturnsAsync(created);

            // Act
            var result = await _service.Create(dto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Teclado");
            result.Price.Should().Be(350000);
        }

        [Fact]
        public async Task Create_CallsRepository_WithCorrectFields()
        {
            // Arrange
            var dto = new CreateProductDto { Name = "Monitor", Description = "4K", Price = 1500000, Stock = 5, ImageUrl = "urlMonitor", IsActive = true, CategoryId = 2 };
            var created = new Product { ProductId = 4, Name = "Monitor", Description = "4K", Price = 1500000, Stock = 5, ImageUrl = "urlMonitor", IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 2, Category = new Category { Name = "Electrónica" } };
            _repoMock.Setup(r => r.Create(It.IsAny<Product>())).ReturnsAsync(created);

            // Act
            await _service.Create(dto);

            // Assert
            _repoMock.Verify(r => r.Create(It.Is<Product>(p =>
                p.Name == "Monitor" &&
                p.Price == 1500000 &&
                p.CategoryId == 2
            )), Times.Once);
        }

        // =====================
        // Update
        // =====================

        [Fact]
        public async Task Update_ReturnsUpdatedProduct_WhenProductExists()
        {
            // Arrange
            var existing = new Product { ProductId = 1, Name = "Laptop", Price = 2500000, Stock = 10, IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1, Category = new Category { Name = "Electrónica" } };
            var updated = new Product { ProductId = 1, Name = "Laptop Pro", Price = 3000000, Stock = 10, IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1, Category = new Category { Name = "Electrónica" } };
            var dto = new UpdateProductDto { Name = "Laptop Pro", Price = 3000000 };

            _repoMock.Setup(r => r.GetByIdAdmin(1)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.Update(It.IsAny<Product>())).ReturnsAsync(updated);

            // Act
            var result = await _service.Update(1, dto);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Laptop Pro");
            result.Price.Should().Be(3000000);
        }

        [Fact]
        public async Task Update_ReturnsNull_WhenProductNotExists()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAdmin(99)).ReturnsAsync((Product?)null);

            // Act
            var result = await _service.Update(99, new UpdateProductDto());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Update_OnlyUpdatesProvidedFields()
        {
            // Arrange
            var existing = new Product { ProductId = 1, Name = "Laptop", Description = "Original", Price = 2500000, Stock = 10, IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1, Category = new Category { Name = "Electrónica" } };
            var dto = new UpdateProductDto { Name = "Laptop Actualizado" };
            var updated = new Product { ProductId = 1, Name = "Laptop Actualizado", Description = "Original", Price = 2500000, Stock = 10, IsActive = true, CreatedAt = DateTime.UtcNow, CategoryId = 1, Category = new Category { Name = "Electrónica" } };

            _repoMock.Setup(r => r.GetByIdAdmin(1)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.Update(It.IsAny<Product>())).ReturnsAsync(updated);

            // Act
            var result = await _service.Update(1, dto);

            // Assert
            result!.Name.Should().Be("Laptop Actualizado");
            result.Description.Should().Be("Original");
            result.Price.Should().Be(2500000);
        }

        // =====================
        // Delete
        // =====================

        [Fact]
        public async Task Delete_ReturnsTrue_WhenProductIsDeleted()
        {
            // Arrange
            _repoMock.Setup(r => r.Delete(1)).ReturnsAsync(true);

            // Act
            var result = await _service.Delete(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenProductNotExists()
        {
            // Arrange
            _repoMock.Setup(r => r.Delete(99)).ReturnsAsync(false);

            // Act
            var result = await _service.Delete(99);

            // Assert
            result.Should().BeFalse();
        }

    }
   
}

