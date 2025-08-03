using System;
using System.Collections.Generic;

namespace CampusCafeOrderingSystem.Models.Entities
{
    public class OrderEntity
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Pending, Completed, Cancelled
        public List<OrderItemEntity> OrderItems { get; set; }
    }
}
