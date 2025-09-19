using CampusCafeOrderingSystem.Models;

namespace CampusCafeOrderingSystem.Models
{
    public class CartItem
    {
        public MenuItem Product { get; set; } = new MenuItem();
        public int Quantity { get; set; }

        public int Id => Product.Id;
        public string Name => Product.Name;
        public string Description => Product.Description;
        public decimal Price => Product.Price;
        
        // Computed property to get image URL from Product
        public string ImageUrl => Product?.ImageUrl ?? "/images/default-food.jpg";
    }
}
