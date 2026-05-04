using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services.Interfaces;

namespace APIEccomerce.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        // =========================
        // 📦 CATALOGO PUBLICO
        // =========================

        public async Task<List<ProductResponseDto>> ListIsActive()
        {
            var products = await _repo.ListIsActive();

            return products.Select(Map).ToList();
        }

        public async Task<ProductResponseDto?> GetByIdPublic(int id)
        {
            var product = await _repo.GetByIdPublic(id);

            if (product == null)
                return null;

            return Map(product);
        }

        public async Task<List<ProductResponseDto>> ListByCategory(string category)
        {
            var products = await _repo.ListByCategory(category);
            return products.Select(Map).ToList();
        }

        public async Task<List<ProductResponseDto>> SearchByName(string name)
        {
            var products = await _repo.SearchByName(name);
            return products.Select(Map).ToList();
        }

        // =========================
        // 🔐 ADMIN
        // =========================

        public async Task<ProductResponseDto?> GetByIdAdmin(int id)
        {
            var product = await _repo.GetByIdAdmin(id);

            if (product == null)
                return null;

            return Map(product);
        }
        public async Task<List<ProductResponseDto>> getAllAdmin()
        {
            var products = await _repo.getAllAdmin();

            return products.Select(Map).ToList();
        }
        public async Task<ProductResponseDto> Create(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                CategoryId = dto.CategoryId
            };

            var created = await _repo.Create(product);

            return Map(created);
        }

        public async Task<ProductResponseDto?> Update(int id, UpdateProductDto dto)
        {
            var product = await _repo.GetByIdAdmin(id);

            if (product == null)
                return null;

            // PATCH UPDATE
            if (dto.Name != null)
                product.Name = dto.Name;

            if (dto.Description != null)
                product.Description = dto.Description;

            if (dto.Price.HasValue)
                product.Price = dto.Price.Value;

            if (dto.Stock.HasValue)
                product.Stock = dto.Stock.Value;

            if (dto.ImageUrl != null)
                product.ImageUrl = dto.ImageUrl;

            if (dto.IsActive.HasValue)
                product.IsActive = dto.IsActive.Value;

            if (dto.CategoryId.HasValue)
                product.CategoryId = dto.CategoryId.Value;

            var updated = await _repo.Update(product);

            return updated == null ? null : Map(updated);
        }

        public async Task<bool> Delete(int id)
        {
            return await _repo.Delete(id);
        }

        // =========================
        // 🔁 MAPPER
        // =========================

        private ProductResponseDto Map(Product p)
        {
            return new ProductResponseDto
            {
                Id = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            };
        }
    }
}