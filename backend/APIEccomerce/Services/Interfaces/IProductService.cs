using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;

namespace APIEccomerce.Services.Interfaces
{
    public interface IProductService
    {
        // catálogo público
        Task<List<ProductResponseDto>> ListIsActive();
        Task<ProductResponseDto?> GetByIdPublic(int id);
        Task<List<ProductResponseDto>> ListByCategory(string category);
        Task<List<ProductResponseDto>> SearchByName(string name);

        // admin
        Task<ProductResponseDto?> GetByIdAdmin(int id);
        Task<ProductResponseDto> Create(CreateProductDto dto);
        Task<ProductResponseDto?> Update(int id, UpdateProductDto dto);
        Task<bool> Delete(int id);
    }
}