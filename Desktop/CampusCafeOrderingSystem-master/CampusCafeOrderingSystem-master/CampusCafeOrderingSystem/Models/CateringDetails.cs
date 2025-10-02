using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    public class CateringDetails
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;
        
        [Required]
        public DateTime EventDate { get; set; }
        
        [Required]
        [Range(1, 1000)]
        public int ExpectedGuests { get; set; }
        
        [Required]
        [StringLength(200)]
        public string EventLocation { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string SpecialRequirements { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;
        
        [Required]
        public decimal EstimatedBudget { get; set; }
        
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string? AdminNotes { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        
        // Add properties needed for views
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }

        // Properties added for view compatibility
        public string? ImageUrl { get; set; }
        public string? VendorName { get; set; }
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        
        public decimal TotalAmount { get; set; }
        
        // Properties added for view compatibility
        public int PeopleCount => ExpectedGuests;
    }
}