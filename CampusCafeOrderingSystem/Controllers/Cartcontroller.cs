using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using CafeApp.Models;
using CafeApp.Models.user_order_pay;
namespace CampusCafeOrderingSystem.Controllers
{
    public class CartController : Controller
    {
        private const string CartKey = "Cart";

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(json);
        }

        private void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartKey, json);
        }

        public IActionResult Index()
        {
            var cartItems = GetCart();
            return View("~/Views/user_order_pay/Menu/cart.cshtml", cartItems);
        }

        [HttpPost]
        public IActionResult Add(int id)
        {
            var cart = GetCart();
            var product = GetMockMenu().Find(m => m.Id == id);
            if (product != null)
            {
                var existingItem = cart.Find(c => c.Product.Id == id);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        Product = product,
                        Quantity = 1
                    });
                }
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Checkout()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cartJson)
                ?new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson);

            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Index"); // Return to cart page
            }

            var paymentModel = new PaymentModel
            {
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity)
            };

            return View("~/Views/user_order_pay/Payment/Checkout.cshtml", paymentModel);
        }

        [HttpPost]
        public IActionResult ProcessPayment(string paymentMethod)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson);

            var totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);
            
            if (paymentMethod == "CampusCard")
            {
                TempData["Message"] = "Paid successfully using Campus Card!";
            }
            else if (paymentMethod == "CreditCard")
            {
                TempData["Message"] = "Paid successfully using Credit/Debit Card!";
            }
            else
            {
                TempData["Message"] = "Invalid payment method selected.";
            }

            TempData["PaymentMethod"] = paymentMethod;
            TempData["TotalAmount"] = totalAmount;
            
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Confirmation");
        }

        public IActionResult Confirmation()
        {
            var message = TempData["Message"] as string ?? "Payment completed successfully!";
            var paymentMethod = TempData["PaymentMethod"] as string ?? "Unknown";
            var totalAmount = TempData["TotalAmount"] as decimal? ?? 0;
            
            var paymentResult = new PaymentResult
            {
                IsSuccess = true,
                Message = message,
                TransactionId = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                TransactionTime = DateTime.Now,
                Amount = totalAmount,
                PaymentMethod = paymentMethod
            };
            return View("~/Views/user_order_pay/Payment/PaymentSuccess.cshtml", paymentResult);
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.Product.Id == id);
            SaveCart(cart);
            return RedirectToAction("Index");
        }
       
        private List<MenuItem> GetMockMenu()
        {
            return new List<MenuItem>
            {
                new MenuItem { Id = 1, Name = "Latte", Price = 4.5M, Description = "Smooth blend", ImageUrl = "/images/latte.jpg" },
                new MenuItem { Id = 2, Name = "Cappuccino", Price = 4.0M, Description = "Frothy", ImageUrl = "/images/cappuccino.jpg" },
                new MenuItem { Id = 3, Name = "Espresso", Price = 3.0M, Description = "Strong shot", ImageUrl = "/images/espresso.jpg" },
            };
        }
    }
}
