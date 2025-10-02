using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CampusCafeOrderingSystem.Hubs;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;
        
        public ReviewApiController(ApplicationDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        
        [HttpGet("list")]
        public async Task<ActionResult<object>> GetReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? rating = null,
            [FromQuery] string? status = null,
            [FromQuery] string? product = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.MenuItem)
                    .AsQueryable();
                
                // Apply filters
                if (rating.HasValue)
                {
                    query = query.Where(r => r.Rating == rating.Value);
                }
                
                if (!string.IsNullOrEmpty(status))
                {
                    if (status == "pending")
                        query = query.Where(r => r.Status == ReviewStatus.Pending);
                    else if (status == "replied")
                        query = query.Where(r => r.Status == ReviewStatus.Replied);
                }
                
                if (!string.IsNullOrEmpty(product))
                {
                    query = query.Where(r => r.MenuItem.Name.Contains(product));
                }
                
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r => r.Comment.Contains(search) || 
                                           r.User.UserName!.Contains(search));
                }
                
                var totalCount = await query.CountAsync();
                
                var reviews = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new ReviewDto
                    {
                        Id = r.Id,
                        CustomerName = r.User.UserName ?? "匿名用户",
                        CustomerAvatar = r.User.UserName != null ? r.User.UserName.Substring(0, 1).ToUpper() : "A",
                        Rating = r.Rating,
                        Product = r.MenuItem.Name,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        Images = string.IsNullOrEmpty(r.Images) ? new List<string>() : 
                                JsonSerializer.Deserialize<List<string>>(r.Images, (JsonSerializerOptions)null) ?? new List<string>(),
                        Reply = r.MerchantReply != null ? new ReviewReplyDto
                        {
                            Content = r.MerchantReply,
                            Date = r.RepliedAt ?? DateTime.Now,
                            Author = "店长"
                        } : null,
                        Status = r.Status == ReviewStatus.Pending ? "pending" : "replied"
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
                return StatusCode(500, new { error = "获取评价列表失败", details = ex.Message });
            }
        }
        
        [HttpGet("stats")]
        public async Task<ActionResult<ReviewStatsDto>> GetReviewStats()
        {
            try
            {
                var reviews = await _context.Reviews.ToListAsync();
                
                if (!reviews.Any())
                {
                    return Ok(new ReviewStatsDto
                    {
                        AverageRating = 0,
                        TotalReviews = 0,
                        PendingReviews = 0,
                        RatingBreakdown = new Dictionary<int, int> { {1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0} }
                    });
                }
                
                var stats = new ReviewStatsDto
                {
                    AverageRating = reviews.Average(r => r.Rating),
                    TotalReviews = reviews.Count,
                    PendingReviews = reviews.Count(r => r.Status == ReviewStatus.Pending),
                    RatingBreakdown = reviews.GroupBy(r => r.Rating)
                                           .ToDictionary(g => g.Key, g => g.Count())
                };
                
                // Ensure all rating levels are present
                for (int i = 1; i <= 5; i++)
                {
                    if (!stats.RatingBreakdown.ContainsKey(i))
                        stats.RatingBreakdown[i] = 0;
                }
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "获取评价统计失败", details = ex.Message });
            }
        }
        
        [HttpPost("submit")]
        [Authorize]
        public async Task<ActionResult> SubmitReviews([FromBody] SubmitReviewsRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "用户未登录" });
                }
                
                // Verify the order belongs to the user
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId);
                    
                if (order == null)
                {
                    return NotFound(new { error = "订单不存在或无权限" });
                }
                
                if (order.Status != OrderStatus.Completed)
                {
                    return BadRequest(new { error = "只能对已完成的订单进行评价" });
                }
                
                // Check if reviews already exist for this order
                var existingReviews = await _context.Reviews
                    .Where(r => r.OrderId == request.OrderId && r.UserId == userId)
                    .ToListAsync();
                    
                if (existingReviews.Any())
                {
                    return BadRequest(new { error = "该订单已经评价过了" });
                }
                
                // Create reviews for each item
                var reviews = new List<Review>();
                foreach (var reviewData in request.Reviews)
                {
                    // Verify the menu item exists in the order
                    var orderItem = order.OrderItems.FirstOrDefault(oi => oi.MenuItemId == reviewData.MenuItemId);
                    if (orderItem == null)
                    {
                        continue; // Skip items not in the order
                    }
                    
                    var review = new Review
                    {
                        UserId = userId,
                        OrderId = request.OrderId,
                        MenuItemId = reviewData.MenuItemId,
                        Rating = reviewData.Rating,
                        Comment = reviewData.Comment,
                        CreatedAt = DateTime.Now,
                        Status = ReviewStatus.Pending
                    };
                    
                    reviews.Add(review);
                }
                
                if (reviews.Any())
                {
                    _context.Reviews.AddRange(reviews);
                    await _context.SaveChangesAsync();
                    
                    // Send SignalR notifications to merchants
                    foreach (var review in reviews)
                    {
                        // Get the menu item to find the vendor email
                        var menuItem = await _context.MenuItems
                            .FirstOrDefaultAsync(m => m.Id == review.MenuItemId);
                            
                        if (menuItem != null && !string.IsNullOrEmpty(menuItem.VendorEmail))
                        {
                            var reviewData = new
                            {
                                id = review.Id,
                                customerName = order.User?.UserName ?? "匿名用户",
                                rating = review.Rating,
                                product = menuItem.Name,
                                comment = review.Comment,
                                createdAt = review.CreatedAt,
                                orderId = review.OrderId
                            };
                            
                            // Send notification to specific merchant
                            await _hubContext.Clients.Group($"Merchant_{menuItem.VendorEmail}")
                                .SendAsync("NewReview", reviewData);
                        }
                    }
                }
                
                return Ok(new { message = "评价提交成功", reviewCount = reviews.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "提交评价失败", details = ex.Message });
            }
        }
        
        [HttpPost("reply")]
        public async Task<ActionResult> ReplyToReview([FromBody] ReplyReviewRequest request)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.MenuItem)
                    .FirstOrDefaultAsync(r => r.Id == request.ReviewId);
                    
                if (review == null)
                {
                    return NotFound(new { error = "评价不存在" });
                }
                
                review.MerchantReply = request.ReplyContent;
                review.RepliedAt = DateTime.Now;
                review.Status = ReviewStatus.Replied;
                
                await _context.SaveChangesAsync();
                
                // Send SignalR notification to customer
                var replyData = new
                {
                    reviewId = review.Id,
                    productName = review.MenuItem?.Name ?? "Unknown Product",
                    replyContent = request.ReplyContent,
                    repliedAt = review.RepliedAt
                };
                
                await _hubContext.Clients.Group($"Customer_{review.UserId}")
                    .SendAsync("ReviewReply", replyData);
                
                return Ok(new { message = "回复成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "回复失败", details = ex.Message });
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                {
                    return NotFound(new { error = "评价不存在" });
                }
                
                review.Status = ReviewStatus.Hidden;
                await _context.SaveChangesAsync();
                
                return Ok(new { message = "评价已隐藏" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "删除失败", details = ex.Message });
            }
        }
    }
}