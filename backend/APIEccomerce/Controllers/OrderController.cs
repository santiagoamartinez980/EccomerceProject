using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIEccomerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _service.GetByUserId(GetUserId());
            return Ok(new Response<List<OrderDto>>
            {
                IsSuccess = true,
                Message   = "Pedidos obtenidos correctamente",
                Value     = orders
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _service.GetById(GetUserId(), id);
            if (order is null)
                return NotFound(new Response<object>
                {
                    IsSuccess = false,
                    Message   = "Pedido no encontrado"
                });

            return Ok(new Response<OrderDto>
            {
                IsSuccess = true,
                Message   = "Pedido obtenido correctamente",
                Value     = order
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var order = await _service.Create(GetUserId(), dto);
            return Ok(new Response<OrderDto>
            {
                IsSuccess = true,
                Message   = "Pedido creado correctamente",
                Value     = order
            });
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _service.Cancel(GetUserId(), id);
            if (!result)
                return NotFound(new Response<object>
                {
                    IsSuccess = false,
                    Message   = "Pedido no encontrado"
                });

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Message   = "Pedido cancelado correctamente"
            });
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _service.UpdateStatus(id, dto.Status);
            if (order is null)
                return NotFound(new Response<object>
                {
                    IsSuccess = false,
                    Message   = "Pedido no encontrado"
                });

            return Ok(new Response<OrderDto>
            {
                IsSuccess = true,
                Message   = "Estado actualizado correctamente",
                Value     = order
            });
        }
    }

    public record UpdateOrderStatusDto(OrderStatus Status);
}
