using APIEccomerce.Controllers;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace APIEccomerce.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _serviceMock;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _serviceMock = new Mock<ICategoryService>();
            _controller = new CategoryController(_serviceMock.Object);
        }

        [Fact]
        public async Task ListCategory_ReturnsOkWithCategories()
        {
            // Arrange
            var categories = new List<CategoryDto>
            {
                new CategoryDto { CategoryId = 1, Name = "Electrónica" },
                new CategoryDto { CategoryId = 2, Name = "Ropa" }
            };
            _serviceMock.Setup(x => x.ListCategory())
                .ReturnsAsync(categories);

            // Act
            var result = await _controller.ListCategory();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { value = categories });
        }

        [Fact]
        public async Task Create_ValidCategory_ReturnsOk()
        {
            // Arrange
            var createDto = new CreateCategoryDto { Name = "Nueva Categoría" };
            var createdCategory = new CategoryDto { CategoryId = 1, Name = "Nueva Categoría" };
            _serviceMock.Setup(x => x.Create(createDto))
                .ReturnsAsync(createdCategory);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { value = createdCategory });
        }

        [Fact]
        public async Task Delete_ExistingCategory_ReturnsOk()
        {
            // Arrange
            var categoryId = 1;
            _serviceMock.Setup(x => x.Delete(categoryId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(categoryId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { mensaje = "Categoría eliminada" });
        }

        [Fact]
        public async Task Delete_NonExistingCategory_ReturnsNotFound()
        {
            // Arrange
            var categoryId = 999;
            _serviceMock.Setup(x => x.Delete(categoryId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(categoryId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { mensaje = "Categoría no encontrada o con productos asociados" });
        }
    }
}