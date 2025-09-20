using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.DTOs
{
    /// <summary>
    /// 创建评价请求DTO
    /// </summary>
    public class CreateReviewRequest
    {
        [Required(ErrorMessage = "订单ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "订单ID必须大于0")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "菜品ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "菜品ID必须大于0")]
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "评分不能为空")]
        [Range(1, 5, ErrorMessage = "评分必须在1-5之间")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "评价内容不能超过1000个字符")]
        public string? Comment { get; set; }

        public List<string> Images { get; set; } = new List<string>();
    }

    /// <summary>
    /// 商家回复评价请求DTO
    /// </summary>
    public class ReplyToReviewRequest
    {
        [Required(ErrorMessage = "回复内容不能为空")]
        [StringLength(500, ErrorMessage = "回复内容不能超过500个字符")]
        public string Reply { get; set; } = string.Empty;
    }

    /// <summary>
    /// 评价响应DTO
    /// </summary>
    public class ReviewResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerAvatar { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public ReviewReplyResponse? Reply { get; set; }
        public string Status { get; set; } = string.Empty; // pending, replied
    }

    /// <summary>
    /// 评价回复响应DTO
    /// </summary>
    public class ReviewReplyResponse
    {
        public string Content { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Author { get; set; } = string.Empty;
    }

    /// <summary>
    /// 评价查询参数DTO
    /// </summary>
    public class ReviewQueryParams
    {
        [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "每页数量必须在1-100之间")]
        public int PageSize { get; set; } = 10;

        [Range(1, 5, ErrorMessage = "评分筛选必须在1-5之间")]
        public int? Rating { get; set; }

        public string? Status { get; set; } = "all"; // all, pending, replied

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "菜品ID必须大于0")]
        public int? MenuItemId { get; set; }

        public string? CustomerEmail { get; set; }

        public string? SortBy { get; set; } = "created"; // created, rating

        public string? SortOrder { get; set; } = "desc"; // asc, desc
    }

    /// <summary>
    /// 评价统计响应DTO
    /// </summary>
    public class ReviewStatsResponse
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public int RepliedReviews { get; set; }
        public List<int> RatingBreakdown { get; set; } = new List<int>(); // [1星数量, 2星数量, 3星数量, 4星数量, 5星数量]
        public List<ReviewTrendData> TrendData { get; set; } = new List<ReviewTrendData>();
    }

    /// <summary>
    /// 评价趋势数据DTO
    /// </summary>
    public class ReviewTrendData
    {
        public DateTime Date { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }

    /// <summary>
    /// 菜品评价汇总DTO
    /// </summary>
    public class MenuItemReviewSummary
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public List<int> RatingBreakdown { get; set; } = new List<int>();
        public DateTime? LastReviewDate { get; set; }
    }

    /// <summary>
    /// 批量回复评价请求DTO
    /// </summary>
    public class BatchReplyRequest
    {
        [Required(ErrorMessage = "评价ID列表不能为空")]
        [MinLength(1, ErrorMessage = "至少需要一个评价ID")]
        public List<int> ReviewIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "回复内容不能为空")]
        [StringLength(500, ErrorMessage = "回复内容不能超过500个字符")]
        public string Reply { get; set; } = string.Empty;
    }
}