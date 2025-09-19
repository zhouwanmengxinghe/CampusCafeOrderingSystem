using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampusCafeOrderingSystem.Services;
using CampusCafeOrderingSystem.Models;
using System.Security.Claims;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderApiController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderApiController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrders(
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            try
            {
                IEnumerable<Order> orders;

                var merchantEmail = User.FindFirstValue(ClaimTypes.Email);
                
                if (!string.IsNullOrEmpty(search))
                {
                    orders = await _orderService.SearchOrdersAsync(search, merchantEmail);
                }
                else if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    orders = await _orderService.GetOrdersByStatusAsync(orderStatus, merchantEmail);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    orders = await _orderService.GetOrdersByDateRangeAsync(startDate.Value, endDate.Value, merchantEmail);
                }
                else
                {
                    orders = await _orderService.GetOrdersByMerchantAsync(merchantEmail);
                }

                var result = orders.Select(o => new
                {
                    id = o.Id,
                    orderNumber = o.OrderNumber,
                    customerName = o.User?.UserName ?? "Unknown",
                    customerPhone = o.CustomerPhone,
                    status = o.Status.ToString().ToLower(),
                    orderTime = o.OrderDate,
                    totalAmount = o.TotalAmount,
                    notes = o.Notes,
                    deliveryType = o.DeliveryType.ToString(),
                    deliveryAddress = o.DeliveryAddress,
                    estimatedCompletionTime = o.EstimatedCompletionTime,
                    items = o.OrderItems.Select(oi => new
                    {
                        name = oi.MenuItemName,
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice,
                        specialInstructions = oi.SpecialInstructions
                    }).ToList()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                
                var result = new
                {
                    id = order.Id,
                    orderNumber = order.OrderNumber,
                    customerName = order.User?.UserName ?? "Unknown",
                    customerPhone = order.CustomerPhone,
                    status = order.Status.ToString().ToLower(),
                    orderTime = order.OrderDate,
                    totalAmount = order.TotalAmount,
                    notes = order.Notes,
                    deliveryType = order.DeliveryType.ToString(),
                    deliveryAddress = order.DeliveryAddress,
                    paymentMethod = order.PaymentMethod,
                    estimatedCompletionTime = order.EstimatedCompletionTime,
                    items = order.OrderItems.Select(oi => new
                    {
                        name = oi.MenuItemName,
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice,
                        specialInstructions = oi.SpecialInstructions
                    }).ToList()
                };

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
                {
                    return BadRequest(new { message = "Invalid status value" });
                }

                var success = await _orderService.UpdateOrderStatusAsync(id, status);
                if (!success)
                {
                    return NotFound(new { message = "Order not found" });
                }

                return Ok(new { message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult<object>> GetOrderStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var merchantEmail = User.FindFirstValue(ClaimTypes.Email);
                
                // 如果没有指定日期，默认获取今天的数据
                var start = startDate ?? DateTime.Today;
                var end = endDate ?? DateTime.Today.AddDays(1).AddTicks(-1);

                var totalRevenue = await _orderService.GetTotalRevenueAsync(merchantEmail, start, end);
                var totalOrders = await _orderService.GetTotalOrdersCountAsync(merchantEmail, start, end);
                
                var pendingOrders = await _orderService.GetOrdersByStatusAsync(OrderStatus.Pending, merchantEmail);
                var preparingOrders = await _orderService.GetOrdersByStatusAsync(OrderStatus.Preparing, merchantEmail);
                var inDeliveryOrders = await _orderService.GetOrdersByStatusAsync(OrderStatus.InDelivery, merchantEmail);
                var completedOrders = await _orderService.GetOrdersByStatusAsync(OrderStatus.Completed, merchantEmail);

                var result = new
                {
                    totalRevenue,
                    totalOrders,
                    pendingCount = pendingOrders.Count(),
                    preparingCount = preparingOrders.Count(),
                    inDeliveryCount = inDeliveryOrders.Count(),
                    completedCount = completedOrders.Where(o => o.OrderDate >= start && o.OrderDate <= end).Count()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            try
            {
                var success = await _orderService.DeleteOrderAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Order not found" });
                }

                return Ok(new { message = "Order deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyOrders()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);

                var result = orders.Select(o => new
                {
                    id = o.Id,
                    orderNumber = o.OrderNumber,
                    status = o.Status.ToString().ToLower(),
                    orderTime = o.OrderDate,
                    totalAmount = o.TotalAmount,
                    paymentMethod = o.PaymentMethod,
                    deliveryType = o.DeliveryType.ToString(),
                    deliveryAddress = o.DeliveryAddress,
                    estimatedCompletionTime = o.EstimatedCompletionTime,
                    items = o.OrderItems.Select(oi => new
                    {
                        id = oi.Id,
                        menuItemId = oi.MenuItemId,
                        name = oi.MenuItemName,
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice,
                        totalPrice = oi.TotalPrice,
                        specialInstructions = oi.SpecialInstructions
                    }).ToList()
                }).OrderByDescending(o => o.orderTime);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-orders/{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<object>> GetMyOrderDetails(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = await _orderService.GetOrderByIdAsync(id);
                
                // 验证订单是否属于当前用户
                if (order.UserId != userId)
                {
                    return Forbid("You can only access your own orders");
                }

                var result = new
                {
                    id = order.Id,
                    orderNumber = order.OrderNumber,
                    status = order.Status.ToString().ToLower(),
                    orderTime = order.OrderDate,
                    totalAmount = order.TotalAmount,
                    paymentMethod = order.PaymentMethod,
                    deliveryType = order.DeliveryType.ToString(),
                    deliveryAddress = order.DeliveryAddress,
                    customerPhone = order.CustomerPhone,
                    notes = order.Notes,
                    estimatedCompletionTime = order.EstimatedCompletionTime,
                    items = order.OrderItems.Select(oi => new
                    {
                        id = oi.Id,
                        menuItemId = oi.MenuItemId,
                        name = oi.MenuItemName,
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice,
                        totalPrice = oi.TotalPrice,
                        specialInstructions = oi.SpecialInstructions
                    }).ToList()
                };

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? EstimatedCompletionTime { get; set; }
    }
}