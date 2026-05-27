using APIEccomerce.Data;
using APIEccomerce.Interfaces;
using APIEccomerce.Models;
using Microsoft.EntityFrameworkCore;

namespace APIEccomerce.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetActiveCartByUserIdAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.IsActive);
        }

        public async Task<Cart> CreateCartAsync(int userId)
        {
            var cart = new Cart
            {
                UserId = userId
            };

            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();

            return cart;
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(i =>
                    i.CartId == cartId &&
                    i.ProductId == productId);
        }

        public async Task AddItemAsync(CartItem item)
        {
            await _context.CartItems.AddAsync(item);
        }

        public Task UpdateItemAsync(CartItem item)
        {
            _context.CartItems.Update(item);

            return Task.CompletedTask;
        }

        public Task RemoveItemAsync(CartItem item)
        {
            _context.CartItems.Remove(item);

            return Task.CompletedTask;
        }

        public async Task ClearCartAsync(int cartId)
        {
            var items = await _context.CartItems
                .Where(i => i.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

