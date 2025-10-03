using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using CampusCafeOrderingSystem.Models;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Services;

namespace CampusCafeOrderingSystem.Controllers
{
    public class CartController : Controller
    {
        private const string CartKey = "Cart";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICreditService _creditService;
        private readonly IOrderService _orderService;
        
        public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ICreditService creditService, IOrderService orderService)
        {
            _context = context;
            _userManager = userManager;
            _creditService = creditService;
            _orderService = orderService;
        }

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
        public async Task<IActionResult> Add(int id)
        {
            var cart = GetCart();
            var product = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.IsAvailable);
            
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
                TempData["SuccessMessage"] = $"{product.Name} added to cart!";
            }
            else
            {
                TempData["ErrorMessage"] = "Product not found or unavailable.";
            }
            
            return RedirectToAction("Index", "Menu");
        }
        public async Task<IActionResult> Checkout()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cartJson)
                ?new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson);

            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Index"); // Return to cart page
            }

            var totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);
            var availableCredits = 0m;
            
            // Get current user's available credits
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userCredit = await _creditService.GetUserCreditsAsync(user.Id);
                availableCredits = userCredit?.CurrentCredits ?? 0;
            }

            var paymentModel = new PaymentModel
            {
                CartItems = cartItems,
                TotalAmount = totalAmount,
                AvailableCredits = availableCredits,
                FinalAmount = totalAmount
            };

            return View("~/Views/user_order_pay/Payment/Checkout.cshtml", paymentModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(PaymentModel model)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson);

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            var totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                TempData["ErrorMessage"] = "Please log in to complete your order.";
                return RedirectToAction("Login", "Account");
            }
            
            // Process credit usage
            var finalAmount = totalAmount;
            var creditsUsed = 0m;
            
            if (model.UseCredits && model.CreditsToUse > 0)
            {
                var userCredit = await _creditService.GetUserCreditsAsync(user.Id);
                var availableCredits = userCredit?.CurrentCredits ?? 0;
                
                // Validate credit usage amount
                creditsUsed = Math.Min(model.CreditsToUse, Math.Min(availableCredits, totalAmount));
                finalAmount = totalAmount - creditsUsed;
                
                // Deduct credits
                if (creditsUsed > 0)
                {
                    await _creditService.SpendCreditsAsync(
                        user.Id, 
                        creditsUsed, 
                        "Used for order payment"
                    );
                }
            }

            // Set payment method from selected payment method
            model.PaymentMethod = model.SelectedPaymentMethod;
            
            // Validate payment information
            var validationResult = ValidatePayment(model);
            if (!validationResult.IsValid)
            {
                TempData["ErrorMessage"] = validationResult.ErrorMessage;
                return RedirectToAction("Checkout");
            }

            // Process payment (only for the final amount after credits)
            PaymentResult paymentResult;
            if (finalAmount > 0)
            {
                paymentResult = ProcessPaymentMethod(model, finalAmount);
                if (!paymentResult.IsSuccessful)
                {
                    // If payment fails and credits were used, need to refund credits
                    if (creditsUsed > 0)
                    {
                        await _creditService.AddCreditsAsync(
                            user.Id, 
                            creditsUsed, 
                            "Refund for failed payment"
                        );
                    }
                    TempData["ErrorMessage"] = paymentResult.Message;
                    return RedirectToAction("Checkout");
                }
            }
            else
            {
                // If final amount is 0 (fully paid with credits), create successful payment result
                paymentResult = new PaymentResult
                {
                    IsSuccess = true,
                    Message = "Payment completed successfully using credits!",
                    TransactionId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
                    TransactionTime = DateTime.Now,
                    Amount = 0,
                    PaymentMethod = "Credits"
                };
            }

            // Create order
            // Re-query MenuItem information from database to ensure VendorEmail is correct
            var firstMenuItemId = cartItems.FirstOrDefault()?.Product.Id;
            var vendorEmail = "vendor@campuscafe.com"; // Default value
            
            if (firstMenuItemId.HasValue)
            {
                var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == firstMenuItemId.Value);
                if (menuItem != null && !string.IsNullOrEmpty(menuItem.VendorEmail))
                {
                    vendorEmail = menuItem.VendorEmail;
                }
            }
            
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount, // Keep original total amount
                PaymentMethod = finalAmount > 0 ? model.PaymentMethod : "Credits",
                TransactionId = paymentResult.TransactionId,
                Status = OrderStatus.Pending,
                DeliveryType = model.DeliveryType,
                DeliveryAddress = model.DeliveryType == DeliveryType.Delivery ? model.DeliveryAddress : null,
                VendorEmail = vendorEmail, // Set vendor email
                OrderItems = cartItems.Select(item => new OrderItem
                {
                    MenuItemId = item.Product.Id,
                    MenuItemName = item.Product.Name,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity
                }).ToList()
            };
            
            // If credits were used, record in order notes
            if (creditsUsed > 0)
            {
                // A notes field can be added to the Order model here, or record credit usage through other means
                // Temporarily track through credit history records
            }

            try
            {
                // Use OrderService to create order (includes SignalR notifications)
                var createdOrder = await _orderService.CreateOrderAsync(order);
                
                // Award credits to user (5% of order total)
                var creditAmount = totalAmount * 0.05m;
                await _creditService.AddCreditsAsync(
                    user.Id, 
                    creditAmount, 
                    $"Order reward for order #{createdOrder.OrderNumber}", 
                    createdOrder.Id
                );
                
                // Clear cart after successful order
                HttpContext.Session.Remove("Cart");
                
                // Store payment result in TempData for success page
                TempData["PaymentResult"] = JsonSerializer.Serialize(paymentResult);
                TempData["OrderNumber"] = createdOrder.OrderNumber;
                
                return RedirectToAction("PaymentSuccess", "Payment", new { orderId = createdOrder.Id });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error processing order: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while processing your order. Please try again.";
                return RedirectToAction("Checkout");
            }
        }
        
        private PaymentResult ProcessPaymentMethod(PaymentModel model, decimal amount)
        {
            // Simulate payment processing - any input succeeds
            var message = model.PaymentMethod switch
            {
                "CampusCard" => "Payment completed successfully using Campus Card!",
                "CreditCard" => "Payment completed successfully using Credit/Debit Card!",
                "MobileWallet" => "Payment completed successfully using Mobile Wallet!",
                _ => "Payment completed successfully!"
            };
            
            return new PaymentResult
            {
                IsSuccess = true,
                Message = message,
                TransactionId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
                TransactionTime = DateTime.Now,
                PaymentMethod = model.PaymentMethod,
                Amount = amount
            };
        }
        
        private PaymentResult ProcessCampusCardPayment(PaymentModel model, decimal amount)
        {
            // Simulate campus card payment validation
            if (string.IsNullOrEmpty(model.CampusCardNumber) || model.CampusCardNumber.Length != 10)
            {
                return new PaymentResult { IsSuccess = false, Message = "Invalid campus card number" };
            }
            
            return new PaymentResult
            {
                IsSuccess = true,
                Message = "Payment successful using Campus Card!",
                TransactionId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
                PaymentMethod = "Campus Card",
                Amount = amount
            };
        }
        
        private PaymentResult ProcessCreditCardPayment(PaymentModel model, decimal amount)
        {
            // Simulate credit card payment validation
            if (string.IsNullOrEmpty(model.CardNumber) || model.CardNumber.Length < 13)
            {
                return new PaymentResult { IsSuccess = false, Message = "Invalid card number" };
            }
            
            return new PaymentResult
            {
                IsSuccess = true,
                Message = "Payment successful using Credit/Debit Card!",
                TransactionId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
                PaymentMethod = "Credit Card",
                Amount = amount
            };
        }
        
        private PaymentResult ProcessMobileWalletPayment(PaymentModel model, decimal amount)
        {
            // Simulate mobile wallet payment validation
            if (string.IsNullOrEmpty(model.MobileNumber) || model.MobileNumber.Length < 10)
            {
                return new PaymentResult { IsSuccess = false, Message = "Invalid mobile number" };
            }
            
            return new PaymentResult
            {
                IsSuccess = true,
                Message = "Payment successful using Mobile Wallet!",
                TransactionId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
                PaymentMethod = "Mobile Wallet",
                Amount = amount
            };
        }
        
        private ValidationResult ValidatePayment(PaymentModel model)
        {
            // Simplified validation logic - pass as long as payment method is selected
            if (string.IsNullOrWhiteSpace(model.SelectedPaymentMethod))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Please select a payment method" };
            }
            
            return new ValidationResult { IsValid = true, ErrorMessage = string.Empty };
        }
        
        private ValidationResult ValidateCampusCard(PaymentModel model)
        {
            if (string.IsNullOrEmpty(model.CampusCardNumber))
                return new ValidationResult { IsValid = false, ErrorMessage = "Campus card number is required" };
            
            if (model.CampusCardNumber.Length != 10)
                return new ValidationResult { IsValid = false, ErrorMessage = "Campus card number must be 10 digits" };
            
            return new ValidationResult { IsValid = true };
        }
        
        private ValidationResult ValidateCreditCard(PaymentModel model)
        {
            if (string.IsNullOrEmpty(model.CardNumber))
                return new ValidationResult { IsValid = false, ErrorMessage = "Card number is required" };
            
            if (string.IsNullOrEmpty(model.ExpiryDate))
                return new ValidationResult { IsValid = false, ErrorMessage = "Expiry date is required" };
            
            if (string.IsNullOrEmpty(model.CVV))
                return new ValidationResult { IsValid = false, ErrorMessage = "CVV is required" };
            
            return new ValidationResult { IsValid = true };
        }
        
        private ValidationResult ValidateMobileWallet(PaymentModel model)
        {
            if (string.IsNullOrEmpty(model.MobileNumber))
                return new ValidationResult { IsValid = false, ErrorMessage = "Mobile number is required" };
            
            if (model.MobileNumber.Length < 10)
                return new ValidationResult { IsValid = false, ErrorMessage = "Invalid mobile number" };
            
            return new ValidationResult { IsValid = true };
        }
        
        private string GenerateOrderNumber()
        {
            return $"ORD{DateTime.Now:yyyyMMdd}{DateTime.Now.Ticks.ToString()[^6..]}";
        }

        public IActionResult Confirmation()
        {
            var message = TempData["Message"] as string ?? "Payment completed successfully!";
            var paymentMethod = TempData["PaymentMethod"] as string ?? "Unknown";
            var totalAmount = TempData["TotalAmount"] as decimal? ?? 0;
            var orderNumber = TempData["OrderNumber"] as string ?? "N/A";
            var transactionId = TempData["TransactionId"] as string ?? Guid.NewGuid().ToString("N")[..8].ToUpper();
            
            var paymentResult = new PaymentResult
            {
                IsSuccess = true,
                Message = message,
                TransactionId = transactionId,
                TransactionTime = DateTime.Now,
                Amount = totalAmount,
                PaymentMethod = paymentMethod
            };
            
            ViewBag.OrderNumber = orderNumber;
            return View("~/Views/user_order_pay/Payment/PaymentSuccess.cshtml", paymentResult);
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.Product.Id == id);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            if (quantity <= 0)
            {
                return Remove(id);
            }

            var cart = GetCart();
            var item = cart.Find(c => c.Product.Id == id);
            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

    }
}
