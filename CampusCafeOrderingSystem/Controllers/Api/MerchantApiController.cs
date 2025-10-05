using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CampusCafeOrderingSystem.Hubs;
using System.Security.Claims;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Vendor")]
    public class MerchantApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;

        public MerchantApiController(ApplicationDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        
        /// <summary>
        /// Get current merchant's review list
        /// </summary>
        [HttpGet("reviews")]
        public async Task<ActionResult<object>> GetMerchantReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? rating = null,
            [FromQuery] string? status = null,
            [FromQuery] string? product = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { error = "Unable to get merchant information" });
                }

                var query = _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.MenuItem)
                    .Where(r => r.MenuItem.VendorEmail == vendorEmail) // Only get current merchant's reviews
                    .AsQueryable();

                // Apply filters
                if (rating.HasValue)
                {
                    query = query.Where(r => r.Rating == rating.Value);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    switch (status.ToLower())
                    {
                        case "pending":
                            query = query.Where(r => r.Status == ReviewStatus.Pending);
                            break;
                        case "replied":
                            query = query.Where(r => r.Status == ReviewStatus.Replied);
                            break;
                        case "hidden":
                            query = query.Where(r => r.Status == ReviewStatus.Hidden);
                            break;
                    }
                }
                
                if (!string.IsNullOrEmpty(product))
                {
                    query = query.Where(r => r.MenuItem.Name.Contains(product));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r => r.Comment.Contains(search) ||
                                           (r.User.UserName != null && r.User.UserName.Contains(search)));
                }

                var totalCount = await query.CountAsync();
                
                var reviews = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new ReviewDto
                    {
                        Id = r.Id,
                        CustomerName = r.User.UserName ?? "Anonymous",
                        CustomerAvatar = r.User.UserName != null ? r.User.UserName.Substring(0, 1).ToUpper() : "A",
                        Rating = r.Rating,
                        Product = r.MenuItem.Name,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        Images = string.IsNullOrEmpty(r.Images) ? new List<string>() : 
                                JsonSerializer.Deserialize<List<string>>(r.Images, (JsonSerializerOptions)null!) ?? new List<string>(),
                        Reply = r.MerchantReply != null ? new ReviewReplyDto
                        {
                            Content = r.MerchantReply,
                            Date = r.RepliedAt ?? DateTime.Now,
                            Author = "Manager"
                        } : null,
                        Status = r.Status == ReviewStatus.Pending ? "pending" :
                                r.Status == ReviewStatus.Replied ? "replied" :
                                r.Status == ReviewStatus.Hidden ? "hidden" : "pending"
                    })
                    .ToListAsync();

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
                return StatusCode(500, new { error = "Failed to get review list", details = ex.Message });
            }
        }

        /// <summary>
        /// Get current merchant's review statistics
        /// </summary>
        [HttpGet("review-stats")]
        public async Task<ActionResult<object>> GetMerchantReviewStats()
        {
            try
            {
                var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { error = "Unable to get merchant information" });
                }

                var reviews = await _context.Reviews
                    .Include(r => r.MenuItem)
                    .Where(r => r.MenuItem.VendorEmail == vendorEmail) // Only count current merchant's reviews
                    .ToListAsync();

                if (!reviews.Any())
                {
                    return Ok(new
                    {
                        avgRating = 0.0,
                        totalReviews = 0,
                        pendingReviews = 0,
                        ratingBreakdown = new int[] { 0, 0, 0, 0, 0 } // [1star, 2star, 3star, 4star, 5star]
                    });
                }

                var avgRating = reviews.Average(r => r.Rating);
                var totalReviews = reviews.Count;
                var pendingReviews = reviews.Count(r => r.Status == ReviewStatus.Pending);
                
                // Calculate rating distribution (sorted by 1-5 stars)
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
                return StatusCode(500, new { error = "Failed to get review statistics", details = ex.Message });
            }
        }

        /// <summary>
        /// Reply to review
        /// </summary>
        [HttpPost("reply-review/{reviewId}")]
        public async Task<ActionResult> ReplyToReview(int reviewId, [FromBody] ReplyRequest request)
        {
            try
            {
                var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { error = "Unable to get merchant information" });
                }

                var review = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.MenuItem)
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.MenuItem.VendorEmail == vendorEmail);

                if (review == null)
                {
                    return NotFound(new { error = "Review not found or no permission" });
                }

                review.MerchantReply = request.Reply;
                review.RepliedAt = DateTime.Now;
                review.Status = ReviewStatus.Replied;
                
                await _context.SaveChangesAsync();

                // Send SignalR notification to customer
                var replyData = new
                {
                    reviewId = review.Id,
                    productName = review.MenuItem?.Name ?? "Unknown Product",
                    replyContent = request.Reply,
                    repliedAt = review.RepliedAt
                };
                
                await _hubContext.Clients.Group($"Customer_{review.UserId}")
                    .SendAsync("ReviewReply", replyData);
                
                return Ok(new { message = "Reply successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Reply failed", details = ex.Message });
            }
        }
        
        /// <summary>
        /// Get current merchant's product category list
        /// </summary>
        [HttpGet("product-categories")]
        public async Task<ActionResult<List<string>>> GetProductCategories()
        {
            try
            {
                var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { error = "Unable to get merchant information" });
                }

                var categories = await _context.MenuItems
                    .Where(m => m.VendorEmail == vendorEmail && !string.IsNullOrWhiteSpace(m.Category))
                    .Select(m => m.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
                
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get category list", details = ex.Message });
            }
        }
        
        /// <summary>
        /// Get business report data for the merchant
        /// </summary>
        [HttpGet("business-report")]
        public async Task<ActionResult> GetBusinessReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var vendorEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return BadRequest(new { error = "User not authenticated" });
                }

                // Get orders in date range for this vendor
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .Where(o => o.OrderItems.Any(oi => oi.MenuItem.VendorEmail == vendorEmail) && 
                               o.OrderDate >= startDate && o.OrderDate <= endDate.AddDays(1))
                    .ToListAsync();

                // Calculate overview statistics
                var totalOrders = orders.Count;
                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                // Get top product
                var productStats = orders
                    .SelectMany(o => o.OrderItems.Where(oi => oi.MenuItem.VendorEmail == vendorEmail))
                    .GroupBy(oi => oi.MenuItem.Name)
                    .Select(g => new { 
                        Name = g.Key, 
                        Quantity = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                    })
                    .OrderByDescending(p => p.Quantity)
                    .ToList();

                var topProduct = productStats.FirstOrDefault()?.Name ?? "No sales";

                // Generate daily data
                var dailyData = new List<object>();
                var currentDate = startDate;
                while (currentDate <= endDate)
                {
                    var dayOrders = orders.Where(o => o.OrderDate.Date == currentDate.Date).ToList();
                    var dayRevenue = dayOrders.Sum(o => o.TotalAmount);
                    var dayOrderCount = dayOrders.Count;
                    var dayAvgOrder = dayOrderCount > 0 ? dayRevenue / dayOrderCount : 0;

                    dailyData.Add(new {
                        date = currentDate.ToString("yyyy-MM-dd"),
                        orders = dayOrderCount,
                        revenue = dayRevenue,
                        avgOrder = dayAvgOrder
                    });

                    currentDate = currentDate.AddDays(1);
                }

                var result = new {
                    overview = new {
                        totalRevenue = totalRevenue,
                        totalOrders = totalOrders,
                        avgOrderValue = avgOrderValue,
                        topProduct = topProduct
                    },
                    dailyData = dailyData,
                    productRanking = productStats.Take(10).Select((p, index) => new {
                        rank = index + 1,
                        name = p.Name,
                        quantity = p.Quantity,
                        revenue = p.Revenue,
                        percentage = productStats.Sum(ps => ps.Revenue) > 0 ? 
                                   (p.Revenue / productStats.Sum(ps => ps.Revenue) * 100).ToString("F1") : "0.0"
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get business report", details = ex.Message });
            }
        }

        /// <summary>
        /// Get sales trend data for charts
        /// </summary>
        [HttpGet("sales-trend")]
        public async Task<ActionResult> GetSalesTrend([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var vendorEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return BadRequest(new { error = "User not authenticated" });
                }

                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .Where(o => o.OrderItems.Any(oi => oi.MenuItem.VendorEmail == vendorEmail) && 
                               o.OrderDate >= startDate && o.OrderDate <= endDate.AddDays(1))
                    .ToListAsync();

                var salesData = new List<object>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    var dayOrders = orders.Where(o => o.OrderDate.Date == currentDate.Date);
                    var dayRevenue = dayOrders.Sum(o => o.TotalAmount);

                    salesData.Add(new {
                        date = currentDate.ToString("yyyy-MM-dd"),
                        revenue = dayRevenue
                    });

                    currentDate = currentDate.AddDays(1);
                }

                return Ok(new {
                    labels = salesData.Select(s => ((dynamic)s).date).ToList(),
                    data = salesData.Select(s => ((dynamic)s).revenue).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get sales trend", details = ex.Message });
            }
        }

        /// <summary>
        /// Get popular items data for charts
        /// </summary>
        [HttpGet("popular-items")]
        public async Task<ActionResult> GetPopularItems([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var vendorEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return BadRequest(new { error = "User not authenticated" });
                }

                var popularItems = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .Where(o => o.OrderItems.Any(oi => oi.MenuItem.VendorEmail == vendorEmail) && 
                               o.OrderDate >= startDate && o.OrderDate <= endDate.AddDays(1))
                    .SelectMany(o => o.OrderItems.Where(oi => oi.MenuItem.VendorEmail == vendorEmail))
                    .GroupBy(oi => oi.MenuItem.Name)
                    .Select(g => new {
                        name = g.Key,
                        quantity = g.Sum(oi => oi.Quantity)
                    })
                    .OrderByDescending(item => item.quantity)
                    .Take(5)
                    .ToListAsync();

                return Ok(new {
                    labels = popularItems.Select(item => item.name).ToList(),
                    data = popularItems.Select(item => item.quantity).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get popular items", details = ex.Message });
            }
        }

        /// <summary>
        /// Get today's dashboard statistics for current merchant
        /// </summary>
        [HttpGet("dashboard-stats")]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            try
            {
                var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized(new { error = "Unable to get merchant information" });
                }

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // Today's orders for this vendor
                var ordersTodayQuery = _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= today && o.OrderDate < tomorrow);

                var todayOrders = await ordersTodayQuery.CountAsync();

                // Revenue only counts completed orders
                var todayRevenue = await ordersTodayQuery
                    .Where(o => o.Status == OrderStatus.Completed)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

                // Total menu items for this vendor
                var totalMenuItems = await _context.MenuItems
                    .Where(m => m.VendorEmail == vendorEmail)
                    .CountAsync();

                // Average rating for this vendor
                var avgRatingDec = await _context.Reviews
                    .Include(r => r.MenuItem)
                    .Where(r => r.MenuItem.VendorEmail == vendorEmail)
                    .AverageAsync(r => (decimal?)r.Rating) ?? 0m;

                var avgRating = Math.Round(avgRatingDec, 1);

                return Ok(new
                {
                    todayOrders,
                    todayRevenue,
                    totalMenuItems,
                    avgRating
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get dashboard stats", details = ex.Message });
            }
        }
    }

    /// <summary>
    /// Reply request model
    /// </summary>
    public class ReplyRequest
    {
        public string Reply { get; set; } = string.Empty;
    }
}