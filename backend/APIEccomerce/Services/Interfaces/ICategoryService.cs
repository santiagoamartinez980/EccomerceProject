using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
namespace APIEccomerce.Services.Interfaces
{
    public interface ICategoryService
    {
        //publico
        Task<List<CategoryDto>> ListCategory();
        //admin
        // [COMENTARIO DE EDICIÓN] MODIFICADO: Firma del método Create actualizada
        // Antes: Task<CategoryDto> Create(CategoryDto dto)
        // Ahora: Task<CategoryDto> Create(CreateCategoryDto dto)
        // Propósito: La interfaz ahora especifica que Create recibe CreateCategoryDto
        // Esto mantiene la consistencia con el patrón DTO segregado
        Task<CategoryDto> Create(CreateCategoryDto dto);
        Task <bool> Delete(int id);
    }
}
