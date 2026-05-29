using APIEccomerce.Models.DTOs;
using APIEccomerce.Models;

namespace APIEccomerce.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetByUserId(int userId);
        Task<OrderDto?> GetById(int userId, int orderId);
        Task<OrderDto> Create(int userId, CreateOrderDto dto);
        Task<OrderDto?> UpdateStatus(int orderId, OrderStatus status);
        Task<bool> Cancel(int userId, int orderId);
        Task<OrderDto?> ConfirmPayment(int orderId);  
    }
}