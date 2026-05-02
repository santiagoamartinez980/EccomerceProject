using APIEccomerce.Models;

namespace APIEccomerce.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        //publico
        Task<List<Category>> ListCategory();

        //admin

        Task<Category> Create(Category category);
        Task <bool> Delete(int id);
         
    }
}
