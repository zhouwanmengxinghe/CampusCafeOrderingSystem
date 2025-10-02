using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.DTOs
{
    /// <summary>
    /// Create review request DTO
    /// </summary>
    public class CreateReviewRequest
    {
        [Required(ErrorMessage = "Order ID cannot be empty")]
        [Range(1, int.MaxValue, ErrorMessage = "Order ID must be greater than 0")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Menu item ID cannot be empty")]
        [Range(1, int.MaxValue, ErrorMessage = "Menu item ID must be greater than 0")]
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "Rating cannot be empty")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1-5")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Review content cannot exceed 1000 characters")]
        public string? Comment { get; set; }

        public List<string> Images { get; set; } = new List<string>();
    }

    /// <summary>
    /// Merchant reply to review request DTO
    /// </summary>
    public class ReplyToReviewRequest
    {
        [Required(ErrorMessage = "Reply content cannot be empty")]
        [StringLength(500, ErrorMessage = "Reply content cannot exceed 500 characters")]
        public string Reply { get; set; } = string.Empty;
    }

    /// <summary>
    /// Review response DTO
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
    /// Review reply response DTO
    /// </summary>
    public class ReviewReplyResponse
    {
        public string Content { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Author { get; set; } = string.Empty;
    }

    /// <summary>
    /// Review query parameters DTO
    /// </summary>
    public class ReviewQueryParams
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1-100")]
        public int PageSize { get; set; } = 10;

        [Range(1, 5, ErrorMessage = "Rating filter must be between 1-5")]
        public int? Rating { get; set; }

        public string? Status { get; set; } = "all"; // all, pending, replied

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Menu item ID must be greater than 0")]
        public int? MenuItemId { get; set; }

        public string? CustomerEmail { get; set; }

        public string? SortBy { get; set; } = "created"; // created, rating

        public string? SortOrder { get; set; } = "desc"; // asc, desc
    }

    /// <summary>
    /// Review statistics response DTO
    /// </summary>
    public class ReviewStatsResponse
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public int RepliedReviews { get; set; }
        public List<int> RatingBreakdown { get; set; } = new List<int>(); // [1-star count, 2-star count, 3-star count, 4-star count, 5-star count]
        public List<ReviewTrendData> TrendData { get; set; } = new List<ReviewTrendData>();
    }

    /// <summary>
    /// Review trend data DTO
    /// </summary>
    public class ReviewTrendData
    {
        public DateTime Date { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }

    /// <summary>
    /// Menu item review summary DTO
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
    /// Batch reply to reviews request DTO
    /// </summary>
    public class BatchReplyRequest
    {
        [Required(ErrorMessage = "Review ID list cannot be empty")]
        [MinLength(1, ErrorMessage = "At least one review ID is required")]
        public List<int> ReviewIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Reply content cannot be empty")]
        [StringLength(500, ErrorMessage = "Reply content cannot exceed 500 characters")]
        public string Reply { get; set; } = string.Empty;
    }
}