using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    public class CateringApplicationViewModel
    {
        // Contact
        [Required(ErrorMessage = "Contact name is required.")]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact phone is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;

        // Event
        [Required, Range(1, 500, ErrorMessage = "Number of people must be between 1 and 500.")]
        [Display(Name = "Number of People")]
        public int NumberOfPeople { get; set; } = 10;

        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        public DateTime? EventDate { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan? StartTime { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan? EndTime { get; set; }

        [Display(Name = "Service Type")]
        public ServiceType ServiceType { get; set; } = ServiceType.Delivery;

        [Display(Name = "Venue / Delivery Address")]
        public string? Address { get; set; }

        [Display(Name = "Dietary Requirements")]
        public List<string> Dietary { get; set; } = new();

        [Display(Name = "Budget per Person")]
        [Range(0, 1000)]
        public decimal? BudgetPerPerson { get; set; }

        [Display(Name = "Preferred Menu Style")]
        public MenuStyle? MenuStyle { get; set; }

        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }
    }
}
