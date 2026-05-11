using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services.Interfaces;
namespace APIEccomerce.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CategoryDto>> ListCategory()
        {
            var categories = await _repo.ListCategory();
            return categories.Select(Map).ToList();
        }

        public async Task<CategoryDto> Create(CategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name
            };
            var createdCategory = await _repo.Create(category);
            return Map(createdCategory);
        }

        public async Task<bool> Delete(int id)
        {
            return await _repo.Delete(id);
        }

        //mapeo
        private CategoryDto Map(Category c)
        {
            return new CategoryDto
            {
                Name = c.Name
            };
        }
    }
}
