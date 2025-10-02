using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using CampusCafeOrderingSystem.Hubs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CampusCafeOrderingSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, IHubContext<OrderHub> hubContext, IMemoryCache memoryCache, ILogger<OrderService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByMerchantAsync(string merchantId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.VendorEmail == merchantId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(string customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new ArgumentException($"Order with ID {orderId} not found.");

            return order;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            order.OrderNumber = GenerateOrderNumber();
            order.CreatedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var createdOrder = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (createdOrder != null)
            {
                await NotifyMerchantNewOrder(createdOrder);
                _memoryCache.Remove($"merchant_report_{order.VendorEmail}");
            }

            return order;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            order.UpdatedAt = DateTime.Now;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedAt = DateTime.Now;

            if (status == OrderStatus.Preparing)
            {
                order.EstimatedCompletionTime = DateTime.Now.AddMinutes(15);
            }

            if (status == OrderStatus.Completed)
            {
                order.CompletedTime = DateTime.Now;
            }
            else if (order.CompletedTime.HasValue)
            {
                order.CompletedTime = null;
            }

            await _context.SaveChangesAsync();
            await NotifyOrderStatusUpdate(order);
            _memoryCache.Remove($"merchant_report_{order.VendorEmail}");

            return true;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Order>> SearchOrdersAsync(string searchTerm, string merchantId = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .AsQueryable();

            if (!string.IsNullOrEmpty(merchantId))
            {
                query = query.Where(o => o.VendorEmail == merchantId);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o => 
                    o.OrderNumber.Contains(searchTerm) ||
                    o.User.UserName.Contains(searchTerm) ||
                    o.CustomerPhone.Contains(searchTerm));
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, string merchantId = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.Status == status);

            if (!string.IsNullOrEmpty(merchantId))
            {
                query = query.Where(o => o.VendorEmail == merchantId);
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, string merchantId = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

            if (!string.IsNullOrEmpty(merchantId))
            {
                query = query.Where(o => o.VendorEmail == merchantId);
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(string merchantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders
                .Where(o => o.Status == OrderStatus.Completed);

            if (!string.IsNullOrEmpty(merchantId))
                query = query.Where(o => o.VendorEmail == merchantId);

            if (startDate.HasValue)
                query = query.Where(o => (o.CompletedTime ?? o.OrderDate) >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => (o.CompletedTime ?? o.OrderDate) <= endDate.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersCountAsync(string merchantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(merchantId))
                query = query.Where(o => o.VendorEmail == merchantId);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            return await query.CountAsync();
        }

        private string GenerateOrderNumber()
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random();
            var sequence = random.Next(1000, 9999);
            return $"{today}{sequence}";
        }

        private async Task NotifyMerchantNewOrder(Order order)
        {
            try
            {
                var orderData = new
                {
                    id = order.Id,
                    orderNumber = order.OrderNumber,
                    customerName = order.User?.UserName ?? "Unknown",
                    customerPhone = order.CustomerPhone,
                    totalAmount = order.TotalAmount,
                    orderTime = order.OrderDate,
                    status = order.Status.ToString(),
                    deliveryType = order.DeliveryType.ToString(),
                    deliveryAddress = order.DeliveryAddress,
                    items = order.OrderItems.Select(oi => new
                    {
                        name = oi.MenuItem?.Name ?? "Unknown Item",
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice
                    }).ToList()
                };

                await _hubContext.Clients.Group($"Merchant_{order.VendorEmail}")
                    .SendAsync("NewOrder", orderData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying merchant: {Message}", ex.Message);
            }
        }

        private async Task NotifyOrderStatusUpdate(Order order)
        {
            try
            {
                var orderData = new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    customerName = order.User?.UserName ?? "Unknown",
                    totalAmount = order.TotalAmount,
                    status = order.Status.ToString(),
                    orderDate = order.OrderDate
                };

                await _hubContext.Clients.User(order.UserId)
                    .SendAsync("OrderStatusUpdate", orderData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying order status update: {Message}", ex.Message);
            }
        }
    }
}