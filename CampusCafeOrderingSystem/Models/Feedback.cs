using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CampusCafeOrderingSystem.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser? User { get; set; }

        // New: for display/contact purposes
        public string? UserEmail { get; set; }   // ★ New

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public FeedbackCategory Category { get; set; }

        // Rating (retained)
        [Range(1, 5)]
        public int? Rating { get; set; }

        // New: priority
        public FeedbackPriority Priority { get; set; } = FeedbackPriority.Medium; // ★ New

        public FeedbackStatus Status { get; set; } = FeedbackStatus.Open;

        public string? AdminResponse { get; set; }
        public string? AdminUserId { get; set; }
        public DateTime? ResponseDate { get; set; }

        public int? OrderId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum FeedbackCategory
    {
        General = 0,
        OrderIssue = 1,
        FoodQuality = 2,
        DeliveryService = 3,
        PaymentIssue = 4,
        TechnicalSupport = 5,
        Suggestion = 6,
        Complaint = 7,
        Contact = 8 // Optional: for "Contact" form
    }

    public enum FeedbackStatus
    {
        Open = 0,
        InProgress = 1,
        Resolved = 2,
        Closed = 3
    }

    // ★ New
    public enum FeedbackPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }
}
