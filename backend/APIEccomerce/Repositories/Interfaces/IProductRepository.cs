using APIEccomerce.Models;

namespace APIEccomerce.Repositories.Interfaces
{
    public interface IProductRepository
    {
        //PUBLICO
        Task<List<Product>> ListIsActive();
        Task<List<Product>> ListByCategory(string categoria);
        Task<List<Product>> SearchByName(string nombre);
        Task<Product?> GetByIdPublic(int id);
        //ADMIN
        Task<Product?> GetByIdAdmin(int id);
        Task<Product> Create(Product producto);
        Task<Product?> Update(Product producto);
        Task<bool> Delete(int id);
    }
}