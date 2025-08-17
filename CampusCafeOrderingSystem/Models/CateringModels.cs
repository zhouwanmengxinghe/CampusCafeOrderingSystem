using System.ComponentModel.DataAnnotations;

namespace CafeApp.Models
{
    public class CateringApplication
    {
        [Required(ErrorMessage = "Event name is required")]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact name is required")]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact phone is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "")]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Number of people is required")]
        [Range(10, 500, ErrorMessage = "Number of people should be between 10-500")]
        [Display(Name = "Expected Number of People")]
        public int PeopleCount { get; set; }

        [Display(Name = "Event Address")]
        public string EventAddress { get; set; } = string.Empty;

        [Display(Name = "Special Requirements")]
        [StringLength(500, ErrorMessage = "Special requirements cannot exceed 500 characters")]
        public string SpecialRequirements { get; set; } = string.Empty;

        [Display(Name = "Budget Range")]
        public string BudgetRange { get; set; } = string.Empty;
    }

    public class CateringDetails
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int PeopleCount { get; set; }
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string EventAddress { get; set; } = string.Empty;
        public string SpecialRequirements { get; set; } = string.Empty;
    }
}