using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services.Interfaces;

namespace APIEccomerce.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepository _productRepo;
        private readonly IAddressRepository _addressRepo;

        public OrderService(
            IOrderRepository orderRepo,
            IProductRepository productRepo,
            IAddressRepository addressRepo)
        {
            _orderRepo   = orderRepo;
            _productRepo = productRepo;
            _addressRepo = addressRepo;
        }

        public async Task<List<OrderDto>> GetByUserId(int userId)
        {
            var orders = await _orderRepo.GetByUserId(userId);
            return orders.Select(Map).ToList();
        }

        public async Task<OrderDto?> GetById(int userId, int orderId)
        {
            var order = await _orderRepo.GetById(orderId);

            if (order is null || order.UserId != userId)
                return null;

            return Map(order);
        }

        public async Task<OrderDto> Create(int userId, CreateOrderDto dto)
        {
            var address = await _addressRepo.GetById(dto.AddressId)
                ?? throw new KeyNotFoundException("Dirección no encontrada.");

            if (address.UserId != userId)
                throw new UnauthorizedAccessException(
                    "La dirección no pertenece al usuario.");

            var details = new List<OrderDetail>();
            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var product = await _productRepo.GetByIdPublic(item.ProductId)
                    ?? throw new KeyNotFoundException(
                        $"Producto {item.ProductId} no encontrado.");

                if (!product.IsActive)
                    throw new InvalidOperationException(
                        $"El producto '{product.Name}' no está disponible.");

                if (product.Stock < item.Quantity)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{product.Name}'. " +
                        $"Disponible: {product.Stock}.");

                details.Add(new OrderDetail
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });

                total += product.Price * item.Quantity;

                // ← product.Stock -= item.Quantity  ELIMINADO
            }

            var order = new Order
            {
                UserId = userId,
                AddressId = dto.AddressId,
                Total = total,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _orderRepo.Create(order);
            var full = await _orderRepo.GetById(created.OrderId);
            return Map(full!);
        }

        public async Task<OrderDto?> UpdateStatus(int orderId, OrderStatus status)
        {
            var order = await _orderRepo.UpdateStatus(orderId, status);
            return order is null ? null : Map(order);
        }

        public async Task<bool> Cancel(int userId, int orderId)
        {
            var order = await _orderRepo.GetById(orderId);

            if (order is null || order.UserId != userId)
                return false;

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException(
                    "Solo se pueden cancelar pedidos en estado pendiente.");

            foreach (var detail in order.Details)
            {
                var product = await _productRepo.GetByIdPublic(detail.ProductId);
                if (product is not null)
                    product.Stock += detail.Quantity;
            }

            await _orderRepo.UpdateStatus(orderId, OrderStatus.Cancelled);
            return true;
        }

        public async Task<OrderDto?> ConfirmPayment(int orderId)
        {
            var order = await _orderRepo.GetById(orderId);

            if (order is null)
                return null;

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException(
                    "El pedido ya fue procesado anteriormente.");

            // Stock baja solo cuando Wompi confirma el pago
            foreach (var detail in order.Details)
            {
                var product = await _productRepo.GetByIdPublic(detail.ProductId);

                if (product is null) continue;

                if (product.Stock < detail.Quantity)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{product.Name}' " +
                        $"al confirmar el pago.");

                product.Stock -= detail.Quantity;
            }

            var updated = await _orderRepo.UpdateStatus(orderId, OrderStatus.Paid);
            return updated is null ? null : Map(updated);
        }

        private static OrderDto Map(Order o) => new()
        {
            OrderId   = o.OrderId,
            Status    = o.Status.ToString(),
            Total     = o.Total,
            CreatedAt = o.CreatedAt,
            Address   = new AddressDto
            {
                AddressId        = o.Address.AddressId,
                AddressLine      = o.Address.AddressLine,
                City             = o.Address.City,
                Department       = o.Address.Department,
                Country          = o.Address.Country,
                PostalCode       = o.Address.PostalCode,
                Latitude         = o.Address.Latitude,
                Longitude        = o.Address.Longitude,
                Phone            = o.Address.Phone,
                Notes            = o.Address.Notes,
                FormattedAddress = o.Address.FormattedAddress,
                IsDefault        = o.Address.IsDefault
            },
            Details = o.Details.Select(d => new OrderDetailDto
            {
                ProductId   = d.ProductId,
                ProductName = d.Product?.Name ?? string.Empty,
                ImageUrl    = d.Product?.ImageUrl,
                Quantity    = d.Quantity,
                UnitPrice   = d.UnitPrice,
                Subtotal    = d.Subtotal
            }).ToList()
        };
    }
}
