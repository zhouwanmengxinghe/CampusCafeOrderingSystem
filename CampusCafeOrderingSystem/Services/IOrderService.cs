using CampusCafeOrderingSystem.Models;

namespace CampusCafeOrderingSystem.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByMerchantAsync(string merchantId);
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(string customerId);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> DeleteOrderAsync(int orderId);
        Task<IEnumerable<Order>> SearchOrdersAsync(string searchTerm, string merchantId = null);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, string merchantId = null);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, string merchantId = null);
        Task<decimal> GetTotalRevenueAsync(string merchantId, DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalOrdersCountAsync(string merchantId, DateTime? startDate = null, DateTime? endDate = null);
    }
}