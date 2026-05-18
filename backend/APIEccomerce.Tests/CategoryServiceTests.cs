using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services;
using FluentAssertions;
using Moq;

namespace APIEccomerce.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _repoMock;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _repoMock = new Mock<ICategoryRepository>();
            _service = new CategoryService(_repoMock.Object);
        }
        // ListCategory
        [Fact]
        public async Task ListCategory_ReturnsListOfCategoryDto_WhenCategoriesExist()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Electrónica" },
                new Category { Name = "Ropa" }
            };
            _repoMock.Setup(r => r.ListCategory()).ReturnsAsync(categories);

            // Act
            var result = await _service.ListCategory();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Electrónica");
            result[1].Name.Should().Be("Ropa");
        }

        [Fact]
        public async Task ListCategory_ReturnsEmptyList_WhenNoCategoriesExist()
        {
            // Arrange
            _repoMock.Setup(r => r.ListCategory()).ReturnsAsync(new List<Category>());

            // Act
            var result = await _service.ListCategory();

            // Assert
            result.Should().BeEmpty();
        }

        // Create


        [Fact]
        public async Task Create_ReturnsCategoryDto_WhenCategoryIsCreated()
        {
            // Arrange
            var dto = new CreateCategoryDto { Name = "Hogar" };
            var createdCategory = new Category { Name = "Hogar" };

            _repoMock.Setup(r => r.Create(It.IsAny<Category>())).ReturnsAsync(createdCategory);

            // Act
            var result = await _service.Create(dto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Hogar");
        }

        [Fact]
        public async Task Create_CallsRepository_WithCorrectCategoryName()
        {
            // Arrange
            var dto = new CreateCategoryDto { Name = "Deportes" };
            var createdCategory = new Category { Name = "Deportes" };

            _repoMock.Setup(r => r.Create(It.IsAny<Category>())).ReturnsAsync(createdCategory);

            // Act
            await _service.Create(dto);

            // Assert
            _repoMock.Verify(r => r.Create(It.Is<Category>(c => c.Name == "Deportes")), Times.Once);
        }

        // Delete


        [Fact]
        public async Task Delete_ReturnsTrue_WhenCategoryIsDeleted()
        {
            // Arrange
            _repoMock.Setup(r => r.Delete(1)).ReturnsAsync(true);

            // Act
            var result = await _service.Delete(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenCategoryDoesNotExist()
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