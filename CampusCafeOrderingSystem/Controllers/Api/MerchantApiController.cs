using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Vendor")]
    public class MerchantApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly Random _random = new Random();
        private readonly bool _useDemoData = false; // 设置为false以使用真实数据

        public MerchantApiController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }
        
        // 新增：统一获取商家邮箱的方法，避免因缺少 Email Claim 导致 Unauthorized
        private string? GetVendorEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.Identity?.Name
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
        }
        
        // 生成演示数据的辅助方法
        private object GenerateDemoDashboardStats()
        {
            return new
            {
                todayOrders = _random.Next(10, 50),
                todayRevenue = Math.Round(_random.Next(500, 2000) + (decimal)_random.NextDouble(), 2),
                totalMenuItems = _random.Next(20, 50),
                avgRating = Math.Round(3.5m + (decimal)(_random.NextDouble()) * 1.5m, 1)
            };
        }
        
        private object GenerateDemoSalesData(DateTime startDate, DateTime endDate)
        {
            var labels = new List<string>();
            var data = new List<decimal>();
            
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                labels.Add(date.ToString("ddd"));
                data.Add(Math.Round(_random.Next(100, 1000) + (decimal)_random.NextDouble(), 2));
            }
            
            return new { labels, data };
        }
        
        private object GenerateDemoPopularItems()
        {
            string[] items = new[] { "Latte", "Americano", "Cappuccino", "Espresso", "Croissant", "Caesar Salad", "Chocolate Cookie" };
            var labels = new List<string>();
            var data = new List<int>();
            
            // 随机选择5个商品
            var selectedItems = items.OrderBy(x => _random.Next()).Take(5).ToList();
            
            foreach (var item in selectedItems)
            {
                labels.Add(item);
                data.Add(_random.Next(10, 100));
            }
            
            return new { labels, data };
        }
        
        private object GenerateDemoRecentOrders()
        {
            var orders = new List<object>();
            string[] statuses = new[] { "Pending", "Preparing", "InDelivery", "Completed" };
            string[] customers = new[] { "John Smith", "Emma Johnson", "Michael Brown", "Olivia Davis", "William Wilson", "Sophia Martinez", "James Anderson", "Charlotte Taylor" };
            
            for (int i = 0; i < 10; i++)
            {
                var createdAt = DateTime.Now.AddMinutes(-_random.Next(1, 300));
                orders.Add(new
                {
                    orderNumber = $"ORD-{DateTime.Now.ToString("yyyyMMdd")}-{_random.Next(1000, 9999)}",
                    customerName = customers[_random.Next(customers.Length)],
                    totalAmount = Math.Round(_random.Next(20, 200) + (decimal)_random.NextDouble(), 2),
                    status = statuses[_random.Next(statuses.Length)],
                    createdAt
                });
            }
            
            return orders.OrderByDescending(o => ((dynamic)o).createdAt).ToList();
        }
        
        private object GenerateDemoReviewStats()
        {
            var avgRating = Math.Round(3.5 + (_random.NextDouble() * 1.5), 1);
            var totalReviews = _random.Next(50, 200);
            var pendingReviews = _random.Next(5, 20);
            
            var ratingBreakdown = new int[5];
            var remainingReviews = totalReviews;
            
            // 生成1-4星评价
            for (int i = 0; i < 4; i++)
            {
                ratingBreakdown[i] = _random.Next(0, remainingReviews / 3);
                remainingReviews -= ratingBreakdown[i];
            }
            
            // 剩余的都是5星评价
            ratingBreakdown[4] = remainingReviews;
            
            return new
            {
                avgRating,
                totalReviews,
                pendingReviews,
                ratingBreakdown
            };
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var demoStats = GenerateDemoDashboardStats();
                    return Ok(demoStats);
                }
                
                // 以下是原有的实际数据获取逻辑
                var vendorEmail = GetVendorEmail();
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                var today = DateTime.Today;
                var todayEnd = today.AddDays(1).AddTicks(-1);

                // Get today's orders for this vendor
                var todayOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .Where(o => o.VendorEmail == vendorEmail && 
                               o.OrderDate >= today && 
                               o.OrderDate <= todayEnd)
                    .ToListAsync();

                // Get total menu items for this vendor
                var totalMenuItems = await _context.MenuItems
                    .CountAsync(m => m.VendorEmail == vendorEmail && m.IsAvailable);

                // Calculate average rating based on real reviews for this vendor
                var avgRating = await _context.Reviews
                    .Include(r => r.MenuItem)
                    .Where(r => r.MenuItem.VendorEmail == vendorEmail && r.Status != ReviewStatus.Hidden)
                    .AverageAsync(r => (decimal?)r.Rating) ?? 0;
                avgRating = Math.Round(avgRating, 1);

                var stats = new
                {
                    todayOrders = todayOrders.Count,
                    todayRevenue = todayOrders
                        .Where(o => o.Status == OrderStatus.Completed)
                        .Sum(o => o.TotalAmount),
                    totalMenuItems = totalMenuItems,
                    avgRating = avgRating
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load dashboard stats", error = ex.Message });
            }
        }

        [HttpGet("sales-data")]
        public async Task<IActionResult> GetSalesData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var demoResult = GenerateDemoSalesData(startDate, endDate);
                    return Ok(demoResult);
                }
                
                // 以下是原有的实际数据获取逻辑
                var vendorEmail = GetVendorEmail();
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                endDate = endDate.Date.AddDays(1).AddTicks(-1);
                
                // 生成缓存键
                string cacheKey = $"sales_data_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // 尝试从缓存获取数据
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                // 获取所有相关订单，包括已完成和其他状态的订单
                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && 
                               (o.CompletedTime ?? o.OrderDate) >= startDate && 
                               (o.CompletedTime ?? o.OrderDate) <= endDate)
                    .ToListAsync();

                // 只计算已完成订单的收入
                var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();

                // Group by day using completed orders only
                var dailyData = completedOrders
                    .GroupBy(o => (o.CompletedTime ?? o.OrderDate).Date)
                    .Select(g => new
                    {
                        date = g.Key,
                        revenue = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(d => d.date)
                    .ToList();

                // Create labels and data arrays
                var labels = new List<string>();
                var data = new List<decimal>();

                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    labels.Add(date.ToString("ddd"));
                    var dayData = dailyData.FirstOrDefault(d => d.date == date);
                    data.Add(dayData?.revenue ?? 0);
                }

                var result = new { labels, data };
                
                // 存储到缓存中，设置过期时间为1小时
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                _cache.Set(cacheKey, result, cacheOptions);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load sales data", error = ex.Message });
            }
        }

        [HttpGet("popular-items")]
        public async Task<IActionResult> GetPopularItems()
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var demoResult = GenerateDemoPopularItems();
                    return Ok(demoResult);
                }
                
                // 以下是原有的实际数据获取逻辑
                var vendorEmail = GetVendorEmail();
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // 生成缓存键
                string cacheKey = $"popular_items_{vendorEmail}_{DateTime.Today:yyyyMMdd}";
                
                // 尝试从缓存获取数据
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                var last30Days = DateTime.Today.AddDays(-30);

                // 获取所有相关订单项，包括各种状态的订单
                var allOrderItems = await _context.OrderItems
                    .Include(oi => oi.MenuItem)
                    .Include(oi => oi.Order)
                    .Where(oi => oi.MenuItem.VendorEmail == vendorEmail &&
                                (oi.Order.CompletedTime ?? oi.Order.OrderDate) >= last30Days)
                    .ToListAsync();

                // 只统计已完成订单的商品销量
                var popularItems = allOrderItems
                    .Where(oi => oi.Order.Status == OrderStatus.Completed)
                    .GroupBy(oi => oi.MenuItem.Name)
                    .Select(g => new
                    {
                        name = g.Key,
                        quantity = g.Sum(oi => oi.Quantity)
                    })
                    .OrderByDescending(x => x.quantity)
                    .Take(5)
                    .ToList();

                if (!popularItems.Any())
                {
                    return Ok(new { labels = new[] { "No Data" }, data = new[] { 1 } });
                }

                var result = new
                {
                    labels = popularItems.Select(x => x.name).ToArray(),
                    data = popularItems.Select(x => x.quantity).ToArray()
                };
                
                // 存储到缓存中，设置过期时间为1小时
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                _cache.Set(cacheKey, result, cacheOptions);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load popular items", error = ex.Message });
            }
        }

        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders()
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var result = GenerateDemoRecentOrders();
                    return Ok(result);
                }
                
                // 以下是原有的实际数据获取逻辑
                var vendorEmail = GetVendorEmail();
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                var recentOrders = await _context.Orders
                    .Include(o => o.User)
                    .Where(o => o.VendorEmail == vendorEmail)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new
                    {
                        orderNumber = o.OrderNumber,
                        customerName = o.User.UserName ?? "Unknown",
                        totalAmount = o.TotalAmount,
                        status = o.Status.ToString(),
                        createdAt = o.OrderDate
                    })
                    .ToListAsync();

                return Ok(recentOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load recent orders", error = ex.Message });
            }
        }

        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? rating = null,
            [FromQuery] string status = "all",
            [FromQuery] string? category = null,
            [FromQuery] string? product = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var vendorEmail = GetVendorEmail();
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // Build email alias list to tolerate compuscafe/campuscafe domain mismatch
                var emails = new List<string> { vendorEmail };
                try
                {
                    var parts = vendorEmail.Split('@');
                    if (parts.Length == 2)
                    {
                        var local = parts[0];
                        var domain = parts[1];
                        if (string.Equals(domain, "compuscafe.com", StringComparison.OrdinalIgnoreCase))
                        {
                            emails.Add($"{local}@campuscafe.com");
                        }
                        else if (string.Equals(domain, "campuscafe.com", StringComparison.OrdinalIgnoreCase))
                        {
                            emails.Add($"{local}@compuscafe.com");
                        }
                    }
                }
                catch { }

                // Normalize to lowercase for case-insensitive comparison
                var lowerEmails = emails.Select(e => e.ToLower()).ToList();

                var query = _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.MenuItem)
                    .Include(r => r.Order)
                    .Where(r => r.Status != ReviewStatus.Hidden &&
                                ((r.MenuItem != null && lowerEmails.Contains((r.MenuItem.VendorEmail ?? string.Empty).ToLower())) ||
                                 (r.Order != null && lowerEmails.Contains((r.Order.VendorEmail ?? string.Empty).ToLower()))));

                // Apply filters
                if (rating.HasValue)
                {
                    query = query.Where(r => r.Rating == rating.Value);
                }

                var normalizedStatus = (status ?? "all").ToLowerInvariant();
                if (normalizedStatus != "all")
                {
                    if (normalizedStatus == "pending")
                    {
                        query = query.Where(r => string.IsNullOrEmpty(r.MerchantReply));
                    }
                    else if (normalizedStatus == "replied")
                    {
                        query = query.Where(r => !string.IsNullOrEmpty(r.MerchantReply));
                    }
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(r => r.MenuItem.Category == category);
                }

                if (!string.IsNullOrWhiteSpace(product))
                {
                    query = query.Where(r => r.MenuItem.Name.Contains(product));
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(r => r.Comment.Contains(search) ||
                                             (r.User.UserName != null && r.User.UserName.Contains(search)) ||
                                             (r.MenuItem != null && r.MenuItem.Name.Contains(search)));
                }

                var totalCount = await query.CountAsync();
                var reviewsQuery = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var reviews = reviewsQuery.Select(r => new
                {
                    id = r.Id,
                    customerName = r.User.UserName ?? "Unknown",
                    customerAvatar = !string.IsNullOrEmpty(r.User.UserName) ? r.User.UserName.Substring(0, 1).ToUpper() : "U",
                    rating = r.Rating,
                    product = r.MenuItem.Name,
                    comment = r.Comment,
                    createdAt = r.CreatedAt,
                    images = string.IsNullOrEmpty(r.Images) ? new string[0] :
                            System.Text.Json.JsonSerializer.Deserialize<string[]>(r.Images),
                    reply = string.IsNullOrEmpty(r.MerchantReply) ? null : new
                    {
                        content = r.MerchantReply,
                        date = r.RepliedAt,
                        author = "Manager"
                    },
                    status = string.IsNullOrEmpty(r.MerchantReply) ? "pending" : "replied"
                }).ToList();

                return Ok(new
                {
                    reviews = reviews,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load reviews", error = ex.Message });
            }
        }

        [HttpGet("review-stats")]
        public async Task<IActionResult> GetReviewStats()
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var result = GenerateDemoReviewStats();
                    return Ok(result);
                }
                
                // 以下是原有的实际数据获取逻辑
                var vendorEmail = GetVendorEmail();
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // Build email alias list to tolerate compuscafe/campuscafe domain mismatch
                var emails = new List<string> { vendorEmail };
                try
                {
                    var parts = vendorEmail.Split('@');
                    if (parts.Length == 2)
                    {
                        var local = parts[0];
                        var domain = parts[1];
                        if (string.Equals(domain, "compuscafe.com", StringComparison.OrdinalIgnoreCase))
                        {
                            emails.Add($"{local}@campuscafe.com");
                        }
                        else if (string.Equals(domain, "campuscafe.com", StringComparison.OrdinalIgnoreCase))
                        {
                            emails.Add($"{local}@compuscafe.com");
                        }
                    }
                }
                catch { }

                // Normalize to lowercase for case-insensitive comparison
                var lowerEmails = emails.Select(e => e.ToLower()).ToList();

                var reviews = await _context.Reviews
                    .Include(r => r.MenuItem)
                    .Include(r => r.Order)
                    .Where(r => r.Status != ReviewStatus.Hidden &&
                                ((r.MenuItem != null && lowerEmails.Contains((r.MenuItem.VendorEmail ?? string.Empty).ToLower())) ||
                                 (r.Order != null && lowerEmails.Contains((r.Order.VendorEmail ?? string.Empty).ToLower()))))
                    .ToListAsync();

                if (!reviews.Any())
                {
                    return Ok(new
                    {
                        avgRating = 0.0,
                        totalReviews = 0,
                        pendingReviews = 0,
                        ratingBreakdown = new int[] { 0, 0, 0, 0, 0 }
                    });
                }

                var avgRating = reviews.Average(r => r.Rating);
                var totalReviews = reviews.Count;
                var pendingReviews = reviews.Count(r => string.IsNullOrEmpty(r.MerchantReply));
                
                var ratingBreakdown = new int[5];
                for (int i = 1; i <= 5; i++)
                {
                    ratingBreakdown[i - 1] = reviews.Count(r => r.Rating == i);
                }

                return Ok(new
                {
                    avgRating = Math.Round(avgRating, 1),
                    totalReviews = totalReviews,
                    pendingReviews = pendingReviews,
                    ratingBreakdown = ratingBreakdown
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load review stats", error = ex.Message });
            }
        }

        [HttpPost("reply-review/{reviewId}")]
        public async Task<IActionResult> ReplyToReview(int reviewId, [FromBody] ReplyRequest request)
        {
            try
            {
                var vendorEmail = GetVendorEmail();
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // Build email alias list to tolerate compuscafe/campuscafe domain mismatch
                var emails = new List<string> { vendorEmail };
                try
                {
                    var parts = vendorEmail.Split('@');
                    if (parts.Length == 2)
                    {
                        var local = parts[0];
                        var domain = parts[1];
                        if (string.Equals(domain, "compuscafe.com", StringComparison.OrdinalIgnoreCase))
                        {
                            emails.Add($"{local}@campuscafe.com");
                        }
                        else if (string.Equals(domain, "campuscafe.com", StringComparison.OrdinalIgnoreCase))
                        {
                            emails.Add($"{local}@compuscafe.com");
                        }
                    }
                }
                catch { }

                // Normalize to lowercase for case-insensitive comparison
                var lowerEmails = emails.Select(e => e.ToLower()).ToList();

                var review = await _context.Reviews
                    .Include(r => r.MenuItem)
                    .Include(r => r.Order)
                    .FirstOrDefaultAsync(r => r.Id == reviewId &&
                        ((r.MenuItem != null && lowerEmails.Contains((r.MenuItem.VendorEmail ?? string.Empty).ToLower())) ||
                         (r.Order != null && lowerEmails.Contains((r.Order.VendorEmail ?? string.Empty).ToLower()))));

                if (review == null)
                {
                    return NotFound(new { error = "Review not found" });
                }

                review.MerchantReply = request.Reply;
                review.RepliedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();

                return Ok(new { message = "Reply submitted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to submit reply", details = ex.Message });
            }
        }
    }

    public class ReplyRequest
    {
        public string Reply { get; set; } = string.Empty;
    }
}