using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999.99)]
        public decimal Price { get; set; }

        [StringLength(200)]
        public string ImageUrl { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        
        public bool IsAvailable { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        [Required]
        [StringLength(100)]
        public string VendorEmail { get; set; } = string.Empty;
    }
}
