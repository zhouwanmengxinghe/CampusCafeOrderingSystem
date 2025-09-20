using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CampusCafeOrderingSystem.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public int MenuItemId { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
        
        public string? Images { get; set; } // JSON array of image URLs
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string? MerchantReply { get; set; }
        
        public DateTime? RepliedAt { get; set; }
        
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;
        
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
        
        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem { get; set; } = null!;
    }
    
    public enum ReviewStatus
    {
        Pending,
        Replied,
        Hidden
    }
    
    // ViewModel for API responses
    public class ReviewDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerAvatar { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Product { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public ReviewReplyDto? Reply { get; set; }
        public string Status { get; set; } = string.Empty;
    }
    
    public class ReviewReplyDto
    {
        public string Content { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Author { get; set; } = "Manager";
    }
    
    public class ReviewStatsDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public Dictionary<int, int> RatingBreakdown { get; set; } = new Dictionary<int, int>();
    }
    
    public class ReplyReviewRequest
    {
        [Required]
        public int ReviewId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string ReplyContent { get; set; } = string.Empty;
    }
    
    public class SubmitReviewsRequest
    {
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public List<ReviewSubmissionDto> Reviews { get; set; } = new List<ReviewSubmissionDto>();
    }
    
    public class ReviewSubmissionDto
    {
        [Required]
        public int MenuItemId { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
}