using APIEccomerce.Interfaces;
using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;

namespace APIEccomerce.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;

        public CartService(
            ICartRepository cartRepo,
            IProductRepository productRepo)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
        }

        public async Task<CartDto> GetOrCreateCartAsync(int userId)
        {
            var cart = await _cartRepo.GetActiveCartByUserIdAsync(userId)
                   ?? await _cartRepo.CreateCartAsync(userId);
            
            return MapCart(cart);
        }

        public async Task<CartDto> AddOrUpdateItemAsync(
            int userId,
            int productId,
            int quantity)
        {
            var cart = await _cartRepo.GetActiveCartByUserIdAsync(userId)
                   ?? await _cartRepo.CreateCartAsync(userId);

            var product = await _productRepo.GetByIdPublic(productId);

            if (product == null)
            {
                throw new KeyNotFoundException("Producto no encontrado.");
            }

            if (quantity <= 0)
            {
                return await RemoveItemAsync(userId, productId);
            }

            var existingItem = await _cartRepo
                .GetCartItemAsync(cart.Id, productId);

            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                existingItem.UnitPrice = product.Price;

                await _cartRepo.UpdateItemAsync(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };

                await _cartRepo.AddItemAsync(newItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _cartRepo.SaveChangesAsync();

            var updatedCart = await _cartRepo.GetActiveCartByUserIdAsync(userId);
            
            return MapCart(updatedCart!);
        }

        public async Task<CartDto> RemoveItemAsync(
            int userId,
            int productId)
        {
            var cart = await _cartRepo
                .GetActiveCartByUserIdAsync(userId);

            if (cart == null)
            {
                throw new InvalidOperationException(
                    "Carrito no encontrado.");
            }

            var item = await _cartRepo
                .GetCartItemAsync(cart.Id, productId);

            if (item != null)
            {
                await _cartRepo.RemoveItemAsync(item);

                await _cartRepo.SaveChangesAsync();
            }

            var updatedCart = await _cartRepo.GetActiveCartByUserIdAsync(userId);
            
            return MapCart(updatedCart!);
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _cartRepo
                .GetActiveCartByUserIdAsync(userId);

            if (cart == null)
            {
                throw new InvalidOperationException(
                    "Carrito no encontrado.");
            }

            await _cartRepo.ClearCartAsync(cart.Id);
        }

        private CartDto MapCart(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
             
                IsActive = cart.IsActive,
                Items = cart.Items.Select(MapCartItem).ToList(),
                Total = cart.Items.Sum(i => i.Subtotal)
            };
        }

        private CartItemDto MapCartItem(CartItem item)
        {
            return new CartItemDto
            {
                CartItemId = item.CartItemId,  
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal,
                ImagenUrl = item.Product?.ImageUrl,   // ? agrega
                Stock = item.Product?.Stock ?? 0
            };
        }
    }
}
