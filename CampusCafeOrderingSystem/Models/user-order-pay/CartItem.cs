using CafeApp.Models;

namespace CafeApp.Models.user_order_pay
{
    public class CartItem
    {
        public MenuItem Product { get; set; } = new MenuItem();
        public int Quantity { get; set; }

        public int Id => Product.Id;
        public string Name => Product.Name;
        public string Description => Product.Description;
        public decimal Price => Product.Price;
    }
}
