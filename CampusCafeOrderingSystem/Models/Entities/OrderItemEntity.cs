using System;

namespace CampusCafeOrderingSystem.Models.Entities
{
    public class OrderItemEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        
        public OrderEntity Order { get; set; }
        public MenuItemEntity MenuItem { get; set; }
    }
}