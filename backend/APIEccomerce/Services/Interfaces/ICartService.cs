using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;

namespace APIEccomerce.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetOrCreateCartAsync(int userId);

        Task<CartDto> AddOrUpdateItemAsync(
            int userId,
            int productId,
            int quantity);

        Task<CartDto> RemoveItemAsync(
            int userId,
            int productId);

        Task ClearCartAsync(int userId);
    }
}
