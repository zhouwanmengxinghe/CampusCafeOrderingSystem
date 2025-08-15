using System;

namespace CampusCafeOrderingSystem.Models
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }              // 1-5
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Pending"; // Pending/Approved/Rejected
    }
}
