using System;

namespace CampusCafeOrderingSystem.Models
{
    public class VendorViewModel
    {
        public string Id { get; set; } = string.Empty;     // АэИз "V001"
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        /// <summary>Pending / Active / Disabled / Rejected</summary>
        public string Status { get; set; } = "Pending";
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public string? Address { get; set; }
        public int MenuItems { get; set; }
        public decimal Rating { get; set; } // 0 ~ 5
    }
}
