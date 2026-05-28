using APIEccomerce.Interfaces;
using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIEccomerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int GetUserId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet]
        public  async Task<IActionResult> GetCart()
        {
            var cart = await _cartService
                .GetOrCreateCartAsync(GetUserId());

            return Ok(new Response<CartDto>
            {
                IsSuccess = true,
                Message = "Carrito obtenido correctamente",
                Value = cart
            });
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddOrUpdateItem([FromBody] CartItemRequest request)
        {
            try
            {
                var cart = await _cartService.AddOrUpdateItemAsync(
                    GetUserId(), request.ProductId, request.Quantity);

                return Ok(new Response<CartDto>
                {
                    IsSuccess = true,
                    Message = "Carrito actualizado correctamente",
                    Value = cart
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new Response<object>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var cart = await _cartService
                .RemoveItemAsync(GetUserId(), productId);

            return Ok(new Response<CartDto>
            {
                IsSuccess = true,
                Message = "Producto eliminado del carrito",
                Value = cart
            });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync(GetUserId());

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Message = "Carrito vaciado correctamente"
            });
        }
    }

    public record CartItemRequest(
        int ProductId,
        int Quantity);
}
