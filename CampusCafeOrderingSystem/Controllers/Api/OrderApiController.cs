using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using CampusCafeOrderingSystem.Models.DTOs;
using CampusCafeOrderingSystem.Services;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Vendor")]
    public class OrderApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;

        public OrderApiController(ApplicationDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        // GET: /api/OrderApi
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<OrderResponseDto>>>> GetOrders([FromQuery] OrderQueryDto query)
        {
            var vendorEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(vendorEmail))
            {
                return Unauthorized(ApiResponse<PagedResult<OrderResponseDto>>.Error("无法获取商家信息", 401));
            }

            // 基础查询：限制为当前商家并包含必要关联
            var q = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.VendorEmail == vendorEmail)
                .AsQueryable();

            // 状态筛选
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (Enum.TryParse<OrderStatus>(query.Status, true, out var status))
                {
                    q = q.Where(o => o.Status == status);
                }
            }

            // 日期范围筛选（包含结束日）
            if (query.StartDate.HasValue && query.EndDate.HasValue)
            {
                var start = query.StartDate.Value.Date;
                var endExclusive = query.EndDate.Value.Date.AddDays(1);
                q = q.Where(o => o.OrderDate >= start && o.OrderDate < endExclusive);
            }

            // 搜索：订单号 / 用户名 / 电话
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                q = q.Where(o => o.OrderNumber.Contains(term) ||
                                 (o.User != null && o.User.UserName!.Contains(term)) ||
                                 (o.CustomerPhone != null && o.CustomerPhone.Contains(term)));
            }

            // 排序（下单时间倒序）
            q = q.OrderByDescending(o => o.OrderDate);

            // 分页
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
            var totalCount = await q.CountAsync();
            var orders = await q.Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

            // 映射DTO
            var items = orders.Select(MapToDto).ToList();
            var paged = new PagedResult<OrderResponseDto>(items, totalCount, page, pageSize);
            return ApiResponse<PagedResult<OrderResponseDto>>.SuccessResult(paged);
        }

        // GET: /api/OrderApi/stats
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<OrderStatsDto>>> GetStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var vendorEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(vendorEmail))
            {
                return Unauthorized(ApiResponse<OrderStatsDto>.Error("无法获取商家信息", 401));
            }

            var q = _context.Orders.Where(o => o.VendorEmail == vendorEmail);
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);
                q = q.Where(o => o.OrderDate >= start && o.OrderDate < endExclusive);
            }

            // 统计
            var totalOrders = await q.CountAsync();
            var pending = await q.Where(o => o.Status == OrderStatus.Pending).CountAsync();
            var preparing = await q.Where(o => o.Status == OrderStatus.Preparing).CountAsync();
            var inDelivery = await q.Where(o => o.Status == OrderStatus.InDelivery).CountAsync();
            var completed = await q.Where(o => o.Status == OrderStatus.Completed).CountAsync();

            var stats = new OrderStatsDto
            {
                TotalOrders = totalOrders,
                PendingOrders = pending,
                PreparingOrders = preparing,
                InDeliveryOrders = inDelivery,
                CompletedOrders = completed,
                CancelledOrders = await q.Where(o => o.Status == OrderStatus.Cancelled).CountAsync(),
                TotalRevenue = 0,
                TodayRevenue = 0,
                AverageOrderValue = totalOrders > 0 ? await q.AverageAsync(o => o.TotalAmount) : 0
            };

            return ApiResponse<OrderStatsDto>.SuccessResult(stats);
        }

        // GET: /api/OrderApi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetOrderById(int id)
        {
            var vendorEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(vendorEmail))
            {
                return Unauthorized(ApiResponse<OrderResponseDto>.Error("无法获取商家信息", 401));
            }

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null || order.VendorEmail != vendorEmail)
                {
                    return NotFound(ApiResponse<OrderResponseDto>.ErrorResult("订单不存在或无权限"));
                }

                var dto = MapToDto(order);
                return ApiResponse<OrderResponseDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResult("获取订单详情失败", ex.Message));
            }
        }

        // PATCH: /api/OrderApi/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto request)
        {
            var vendorEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(vendorEmail))
            {
                return Unauthorized(ApiResponse<object>.Error("无法获取商家信息", 401));
            }

            // 校验订单归属
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null || order.VendorEmail != vendorEmail)
            {
                return NotFound(ApiResponse<object>.ErrorResult("订单不存在或无权限"));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("请求参数不合法"));
            }

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("无效的订单状态"));
            }

            var ok = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!ok)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("更新订单状态失败"));
            }

            return Ok(ApiResponse<object>.SuccessResult(new { id, status = status.ToString() }, "状态更新成功"));
        }

        private static OrderResponseDto MapToDto(Order o)
        {
            return new OrderResponseDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status.ToString(),
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod,
                DeliveryType = o.DeliveryType.ToString(),
                DeliveryAddress = o.DeliveryAddress,
                EstimatedCompletionTime = o.EstimatedCompletionTime,
                Notes = o.Notes,
                VendorEmail = o.VendorEmail,
                CustomerName = o.User?.UserName,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    MenuItemId = oi.MenuItemId,
                    MenuItemName = oi.MenuItemName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    SpecialInstructions = oi.SpecialInstructions
                }).ToList()
            };
        }
    }
}