using System;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    public class CateringApplication
    {
        public int Id { get; set; }

        // Contact
        [Required, StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required, Phone, StringLength(40)]
        public string ContactPhone { get; set; } = string.Empty;

        // Event
        [Range(1, 500)]
        public int NumberOfPeople { get; set; }

        public DateTime? EventDate { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? StartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? EndTime { get; set; }

        public ServiceType ServiceType { get; set; } = ServiceType.Delivery;

        [StringLength(200)]
        public string? Address { get; set; }

     
        [StringLength(300)]
        public string? DietaryCsv { get; set; }

        [Range(0, 1000)]
        public decimal? BudgetPerPerson { get; set; }

        public MenuStyle? MenuStyle { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Event details
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [StringLength(200)]
        public string EventLocation { get; set; } = string.Empty;

        [StringLength(500)]
        public string SpecialRequirements { get; set; } = string.Empty;

        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        // System fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public DateTime? ReviewedAt { get; set; }
        public string? AdminNotes { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
