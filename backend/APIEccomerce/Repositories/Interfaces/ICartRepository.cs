using APIEccomerce.Models;

namespace APIEccomerce.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetActiveCartByUserIdAsync(int userId);

        Task<Cart> CreateCartAsync(int userId);

        Task<CartItem?> GetCartItemAsync(int cartId, int productId);

        Task AddItemAsync(CartItem item);

        Task UpdateItemAsync(CartItem item);

        Task RemoveItemAsync(CartItem item);

        Task ClearCartAsync(int cartId);

        Task SaveChangesAsync();
    }
}