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
        
        // 添加视图需要的属性
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        
        public decimal TotalAmount { get; set; }
        
        // 为视图兼容性添加的属性
        public int PeopleCount => ExpectedGuests;
    }
}