using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using CampusCafeOrderingSystem.Hubs;
using System.Text;
using System.Text.Json;

namespace CampusCafeOrderingSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly HttpClient _httpClient;

        public OrderService(ApplicationDbContext context, IHubContext<OrderHub> hubContext, HttpClient httpClient)
        {
            _context = context;
            _hubContext = hubContext;
            _httpClient = httpClient;
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
            // 根据商家邮箱筛选订单
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
            // 生成订单号
            order.OrderNumber = await GenerateOrderNumberAsync();
            order.CreatedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 重新加载订单以获取完整的关联数据
            var createdOrder = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (createdOrder != null)
            {
                // 发送实时通知给对应商家
                var orderData = new
                {
                    id = createdOrder.Id,
                    orderNumber = createdOrder.OrderNumber,
                    customerName = createdOrder.User?.UserName ?? "Unknown",
                    customerPhone = createdOrder.CustomerPhone,
                    totalAmount = createdOrder.TotalAmount,
                    orderTime = createdOrder.OrderDate,
                    status = createdOrder.Status.ToString(),
                    deliveryType = createdOrder.DeliveryType.ToString(),
                    deliveryAddress = createdOrder.DeliveryAddress,
                    items = createdOrder.OrderItems.Select(oi => new
                    {
                        name = oi.MenuItem?.Name ?? "Unknown Item",
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice
                    }).ToList()
                };

                // 发送给特定商家组
                if (!string.IsNullOrEmpty(createdOrder.VendorEmail))
                {
                    await _hubContext.Clients.Group($"Merchant_{createdOrder.VendorEmail}")
                        .SendAsync("NewOrder", orderData);
                    
                    // 清除报表缓存以确保实时更新
                    await ClearReportsCacheAsync(createdOrder.VendorEmail);
                }

                // 发送给客户
                await _hubContext.Clients.Group($"Customer_{createdOrder.UserId}")
                    .SendAsync("OrderCreated", orderData);
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

            var oldStatus = order.Status;
            order.Status = status;
            order.UpdatedAt = DateTime.Now;

            // 设置预计完成时间
            if (status == OrderStatus.Preparing)
            {
                order.EstimatedCompletionTime = DateTime.Now.AddMinutes(15); // 默认15分钟
            }

            // 设置/清理完成时间
            if (status == OrderStatus.Completed)
            {
                order.CompletedTime = DateTime.Now;
            }
            else if (oldStatus == OrderStatus.Completed && status != OrderStatus.Completed)
            {
                // 如果订单从已完成切换到其他状态，清空完成时间
                order.CompletedTime = null;
            }

            await _context.SaveChangesAsync();

            // 发送状态更新通知给商家
            if (!string.IsNullOrEmpty(order.VendorEmail))
            {
                await _hubContext.Clients.Group($"Merchant_{order.VendorEmail}")
                    .SendAsync("OrderStatusUpdated", order.Id, status.ToString());
                
                // 清除报表缓存以确保实时更新
                await ClearReportsCacheAsync(order.VendorEmail);
            }

            // 发送给特定客户
            await _hubContext.Clients.Group($"Customer_{order.UserId}")
                .SendAsync("OrderStatusChanged", new
                {
                    orderId = order.Id,
                    status = status.ToString(),
                    estimatedTime = order.EstimatedCompletionTime
                });

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

        private async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var lastOrder = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith(today))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastOrder != null)
            {
                var lastSequence = lastOrder.OrderNumber.Substring(8);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{today}{sequence:D4}";
        }

        private async Task ClearReportsCacheAsync(string vendorEmail)
        {
            try
            {
                // 创建一个匿名对象并序列化为JSON
                var payload = new { vendorEmail };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // 使用绝对URL路径
                var response = await _httpClient.PostAsync("http://localhost:5117/api/ReportsApi/clear-cache", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    // 记录错误但不影响主流程
                    Console.WriteLine($"Failed to clear reports cache for vendor {vendorEmail}: {response.StatusCode}");
                    // 输出响应内容以便调试
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response content: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不影响主流程
                Console.WriteLine($"Error clearing reports cache for vendor {vendorEmail}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}