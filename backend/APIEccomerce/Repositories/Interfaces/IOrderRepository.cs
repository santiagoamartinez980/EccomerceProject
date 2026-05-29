using APIEccomerce.Models;

namespace APIEccomerce.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetByUserId(int userId);
        Task<Order?> GetById(int orderId);
        Task<Order> Create(Order order);
        Task<Order?> UpdateStatus(int orderId, OrderStatus status);
        Task<bool> Delete(int orderId);
    }
}
