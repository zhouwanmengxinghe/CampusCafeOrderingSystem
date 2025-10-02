using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.ViewModels
{
    // Vendor management ViewModel
    public class VendorViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public int MenuItems { get; set; }
        public decimal Rating { get; set; }
        public int OrderCount { get; set; }
    }

    // Order management ViewModel
    public class OrderManagementViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    // Order item ViewModel
    public class OrderItemViewModel
    {
        public string MenuItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // Review management ViewModel
    public class ReviewManagementViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string MenuItemName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty; // For compatibility with existing views
    }

    // Review ViewModel for Admin Review Center
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsApproved { get; set; }
        public string VendorEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // Report ViewModel
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalVendors { get; set; }
        public List<DailyRevenueData> DailyRevenue { get; set; } = new List<DailyRevenueData>();
        public List<PopularItemData> PopularItems { get; set; } = new List<PopularItemData>();
        public List<VendorPerformanceData> VendorPerformance { get; set; } = new List<VendorPerformanceData>();
    }

    public class PopularItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class PopularItemData
    {
        public string ItemName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class VendorPerformanceData
    {
        public string VendorName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class DailyRevenueData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}