using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
namespace APIEccomerce.Services.Interfaces
{
    public interface ICategoryService
    {
        //publico
        Task<List<CategoryDto>> ListCategory();
        //admin
        Task<CategoryDto> Create(CategoryDto dto);
        Task <bool> Delete(int id);
    }
}
