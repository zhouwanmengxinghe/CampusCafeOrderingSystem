using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    // 商家管理ViewModel
    public class VendorViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public string Address { get; set; } = string.Empty;
        public int MenuItems { get; set; }
        public decimal Rating { get; set; }
        public int OrderCount { get; set; }
    }

    // 订单管理ViewModel
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string VendorEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    // 订单项ViewModel
    public class OrderItemViewModel
    {
        public string MenuItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    // 评论管理ViewModel
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
        public string Status { get; set; } = string.Empty; // 为了兼容现有视图
    }

    // 报告ViewModel
    public class ReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalVendors { get; set; }
        public List<DailyRevenueData> DailyRevenue { get; set; } = new List<DailyRevenueData>();
        public List<PopularItemData> PopularItems { get; set; } = new List<PopularItemData>();
        public List<VendorPerformanceData> VendorPerformance { get; set; } = new List<VendorPerformanceData>();
    }

    public class DailyRevenueData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
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
}