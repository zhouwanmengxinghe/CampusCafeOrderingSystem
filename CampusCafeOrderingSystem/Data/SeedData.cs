using CampusCafeOrderingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusCafeOrderingSystem.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Seed menu items for vendor@qq.com
            await SeedMenuItems(context);
            
            // Seed orders data
            await SeedOrders(context);
            
            // Seed reviews data
            await SeedReviews(context);

            // Seed feedback data
            await SeedFeedbacks(context);
        }
        
        private static async Task SeedMenuItems(ApplicationDbContext context)
        {
            // Seed for vendor@campuscafe.com
            var vendorEmail1 = "vendor@campuscafe.com";
            await SeedMenuItemsForVendor(context, vendorEmail1);
            
            // Seed for vendor@qq.com
            var vendorEmail2 = "vendor@qq.com";
            await SeedMenuItemsForVendor(context, vendorEmail2);
        }
        
        private static async Task SeedMenuItemsForVendor(ApplicationDbContext context, string vendorEmail)
        {
            // Curated menu items by category with English descriptions and Wikimedia images
            var curatedItems = new List<MenuItem>
            {
                // Coffee
                new MenuItem { Name = "Espresso", Description = "A strong, concentrated coffee brewed under high pressure, forming the base for many espresso-based drinks.", Price = 3.00m, Category = "Coffee", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Espresso%20coffee.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Cappuccino", Description = "A 1:1:1 ratio of espresso, steamed milk, and frothy milk foam, creating a smooth and creamy texture.", Price = 4.00m, Category = "Coffee", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Classic%20Cappuccino.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Latte", Description = "Espresso with a larger amount of steamed milk, often topped with latte art for a creamy and mild taste.", Price = 4.50m, Category = "Coffee", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Latte%20art.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Cold Brew", Description = "Coffee brewed with cold water for an extended period, resulting in a smooth, less acidic flavor.", Price = 4.50m, Category = "Coffee", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Cold%20Brew%20Coffee.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Mocha", Description = "A combination of espresso, steamed milk, and chocolate syrup, delivering a sweet and rich flavor.", Price = 4.80m, Category = "Coffee", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Mocha%20coffee.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Macchiato", Description = "An espresso with just a touch of milk foam on top, offering a layered and bold flavor.", Price = 3.80m, Category = "Coffee", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Caff%C3%A8%20macchiato.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },

                // Dessert
                new MenuItem { Name = "NY Cheesecake", Description = "A rich and dense baked cheesecake with a smooth, creamy texture.", Price = 6.00m, Category = "Dessert", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/My%20first%20cheesecake.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Tiramisu", Description = "A classic Italian dessert made with layers of coffee-soaked ladyfingers and mascarpone cheese.", Price = 6.50m, Category = "Dessert", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Tiramisu%20dessert.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Brownie", Description = "A chocolatey, dense dessert with a fudgy texture, often paired with ice cream.", Price = 4.50m, Category = "Dessert", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Brownie%20dessert%20(43110731564).jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Macaron", Description = "A delicate French pastry made with almond meringue, filled with buttercream or ganache.", Price = 3.50m, Category = "Dessert", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Macaron%201.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Apple Pie", Description = "A traditional American dessert with a flaky pie crust and spiced apple filling.", Price = 5.50m, Category = "Dessert", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/American%20apple%20pie.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Panna Cotta", Description = "A creamy, Italian dessert made from sweetened cream, often served with berries or fruit sauce.", Price = 5.50m, Category = "Dessert", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Panna%20Cotta.JPG?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },

                // Snack
                new MenuItem { Name = "Club Sandwich", Description = "A classic triple-layer sandwich with bacon, chicken, lettuce, and tomato, often served with fries.", Price = 8.50m, Category = "Snack", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Club-sandwich.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Bagel Sandwich", Description = "A sandwich made with a bagel, filled with bacon, eggs, and cheese, offering a hearty breakfast.", Price = 6.50m, Category = "Snack", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/BEC%20Sandwich%20on%20Everything%20Bagel.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Quiche Lorraine", Description = "A savory pie filled with eggs, cream, cheese, and bacon — a classic French dish.", Price = 7.00m, Category = "Snack", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Quiche%20lorraine%2001.JPG?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Pretzel", Description = "A twisted dough snack, often sprinkled with salt, perfect for pairing with coffee.", Price = 3.00m, Category = "Snack", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Pretzel.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Scone", Description = "A British baked good, typically served with clotted cream and jam, often enjoyed with tea.", Price = 3.50m, Category = "Snack", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Scone%20varieties.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Yogurt Parfait", Description = "A layered snack of yogurt, granola, and fresh fruit, offering a healthy and refreshing option.", Price = 5.00m, Category = "Snack", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/-2020-01-13%20Fruit%20and%20Yogurt%20Breakfast%20Parfait%2C%20Trimingham.JPG?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },

                // Tea
                new MenuItem { Name = "Earl Grey", Description = "A black tea flavored with bergamot orange, offering a fragrant, citrusy taste.", Price = 3.00m, Category = "Tea", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Cup%20of%20Earl%20Gray.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Sencha", Description = "A Japanese green tea, known for its grassy, fresh flavor, often enjoyed after meals.", Price = 3.20m, Category = "Tea", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Sencha%20tea.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Oolong", Description = "A partially fermented tea, blending floral and roasted notes for a complex flavor profile.", Price = 3.50m, Category = "Tea", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Oolong%20Tea.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Matcha Latte", Description = "A combination of matcha green tea and steamed milk, often served with vibrant green color and artful presentation.", Price = 4.50m, Category = "Tea", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Matcha%20Latte.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Masala Chai", Description = "A spiced tea made with black tea, milk, and aromatic spices, providing warmth and comfort.", Price = 4.00m, Category = "Tea", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Masala%20Tea%201.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow },
                new MenuItem { Name = "Chamomile", Description = "A caffeine-free herbal tea, known for its calming properties and light floral flavor.", Price = 3.00m, Category = "Tea", ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Glass%20of%20chamomile%20tea.jpg?width=1200", IsAvailable = true, VendorEmail = vendorEmail, CreatedAt = DateTime.UtcNow }
            };


            // Upsert by Name for the given vendor, and disable outdated items
            var existingItems = await context.MenuItems.Where(m => m.VendorEmail == vendorEmail).ToListAsync();
            var curatedNames = new HashSet<string>(curatedItems.Select(i => i.Name), StringComparer.OrdinalIgnoreCase);

            foreach (var item in curatedItems)
            {
                var match = existingItems.FirstOrDefault(e => e.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    match.Description = item.Description;
                    match.Price = item.Price;
                    match.Category = item.Category;
                    match.ImageUrl = item.ImageUrl;
                    match.IsAvailable = true;
                    match.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    context.MenuItems.Add(item);
                }
            }

            foreach (var existing in existingItems)
            {
                if (!curatedNames.Contains(existing.Name))
                {
                    existing.IsAvailable = false;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }

            await context.SaveChangesAsync();
        }
        
        private static async Task SeedOrders(ApplicationDbContext context)
        {
            // Vendors to distribute orders across
            var vendors = new[] { "vendor@campuscafe.com", "vendor@qq.com" };

            // Aggregate existing orders for the targeted vendors
            var existingCount = await context.Orders
                .Where(o => vendors.Contains(o.VendorEmail))
                .CountAsync();

            // Target total of 100 orders; only create the gap to avoid duplication
            var toCreate = Math.Max(0, 100 - existingCount);
            if (toCreate == 0) return;

            // Load menu items per vendor
            var menuItemsByVendor = new Dictionary<string, List<MenuItem>>();
            foreach (var v in vendors)
            {
                var items = await context.MenuItems
                    .Where(m => m.VendorEmail == v && m.IsAvailable)
                    .ToListAsync();
                if (items.Any()) menuItemsByVendor[v] = items;
            }
            if (!menuItemsByVendor.Any()) return;

            // Choose a user to own these seeded orders (existing customer if available)
            var existingUser = await context.Users
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync();
            if (existingUser == null) return;

            var random = new Random();
            var orders = new List<Order>();

            // Currency tags for payment method (amounts remain in default NZD for reports)
            var currencies = new[] { "NZD", "USD", "CNY" };

            // Payment methods
            var paymentMethods = new[] { "Credit Card", "Cash", "Online" };

            // Helper to pick weighted order status (more completed to show reports)
            OrderStatus WeightedStatus()
            {
                var roll = random.Next(100); // 0-99
                if (roll < 70) return OrderStatus.Completed; // 70%
                var others = new[] { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Ready };
                return others[random.Next(others.Length)];
            }

            // Helper: random business time within the last 60 days, 8:00-18:00
            DateTime RandomBusinessTimeUtc()
            {
                var daysBack = random.Next(0, 60); // within two months
                var date = DateTime.UtcNow.Date.AddDays(-daysBack);
                var hour = random.Next(8, 19); // 8-18 inclusive
                var minute = random.Next(0, 60);
                var second = random.Next(0, 60);
                return new DateTime(date.Year, date.Month, date.Day, hour, minute, second, DateTimeKind.Utc);
            }

            for (int i = 0; i < toCreate; i++)
            {
                // Pick a vendor with available items
                var vendorEmail = vendors[random.Next(vendors.Length)];
                if (!menuItemsByVendor.TryGetValue(vendorEmail, out var menuItems) || !menuItems.Any())
                {
                    // fallback to any vendor with items
                    var kv = menuItemsByVendor.FirstOrDefault();
                    vendorEmail = kv.Key;
                    menuItems = kv.Value;
                }

                var orderDate = RandomBusinessTimeUtc();
                var status = WeightedStatus();

                // Select currency with small probability for USD/CNY
                string currencyTag;
                var currencyRoll = random.Next(100);
                if (currencyRoll < 10) currencyTag = "USD";        // ~10%
                else if (currencyRoll < 18) currencyTag = "CNY";   // ~8%
                else currencyTag = "NZD";                          // majority

                var payment = paymentMethods[random.Next(paymentMethods.Length)];
                var paymentWithCurrency = $"{payment} ({currencyTag})";

                var order = new Order
                {
                    OrderNumber = $"ORD{DateTime.UtcNow.Ticks}{i:D3}",
                    UserId = existingUser.Id,
                    CustomerPhone = $"021{random.Next(1000000, 9999999)}",
                    VendorEmail = vendorEmail,
                    Status = status,
                    TotalAmount = 0,
                    OrderDate = orderDate,
                    CreatedAt = orderDate,
                    UpdatedAt = orderDate,
                    PaymentMethod = paymentWithCurrency,
                    TransactionId = $"TX-{Guid.NewGuid().ToString("N").Substring(12)}",
                    DeliveryType = random.Next(100) < 60 ? DeliveryType.Pickup : DeliveryType.Delivery,
                    DeliveryAddress = null,
                    OrderItems = new List<OrderItem>()
                };

                if (order.DeliveryType == DeliveryType.Delivery)
                {
                    order.DeliveryAddress = $"{random.Next(1, 200)} Campus Ave, Palmerston North";
                }

                // Add 1-5 random items to each order
                var itemCount = random.Next(1, 6);
                var selectedItems = menuItems.OrderBy(_ => random.Next()).Take(itemCount).ToList();

                foreach (var menuItem in selectedItems)
                {
                    var quantity = random.Next(1, 4);
                    var orderItem = new OrderItem
                    {
                        MenuItemId = menuItem.Id,
                        MenuItemName = menuItem.Name,
                        UnitPrice = menuItem.Price,
                        Quantity = quantity
                    };

                    order.OrderItems.Add(orderItem);
                    order.TotalAmount += orderItem.TotalPrice;
                }

                // Estimated completion and completed time
                var prepMinutes = random.Next(5, 45);
                order.EstimatedCompletionTime = order.OrderDate.AddMinutes(prepMinutes);
                if (order.Status == OrderStatus.Completed)
                {
                    order.CompletedTime = order.EstimatedCompletionTime?.AddMinutes(random.Next(0, 30));
                }

                orders.Add(order);
            }

            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();

            // Ensure each vendor has at least a few orders today for dashboard display
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            foreach (var vendor in vendors)
            {
                var existingToday = await context.Orders
                    .Where(o => o.VendorEmail == vendor && o.OrderDate >= today && o.OrderDate < tomorrow)
                    .CountAsync();

                var ensureCount = 3; // target minimum per vendor for today
                var missing = Math.Max(0, ensureCount - existingToday);
                if (missing <= 0) continue;

                if (!menuItemsByVendor.TryGetValue(vendor, out var todaysMenuItems) || !todaysMenuItems.Any())
                    continue;

                var todaysOrders = new List<Order>();
                for (int j = 0; j < missing; j++)
                {
                    // Local business time today so UI stats pick them up
                    var orderDateToday = today
                        .AddHours(random.Next(8, 19))
                        .AddMinutes(random.Next(0, 60))
                        .AddSeconds(random.Next(0, 60));

                    // Ensure at least one completed order for revenue per vendor
                    var statusToday = (j == 0) ? OrderStatus.Completed : WeightedStatus();

                    var paymentToday = paymentMethods[random.Next(paymentMethods.Length)];
                    var paymentWithCurrencyToday = $"{paymentToday} (NZD)"; // keep NZD for dashboard revenue

                    var todayOrder = new Order
                    {
                        OrderNumber = $"ORD{DateTime.UtcNow.Ticks}{vendor.GetHashCode():X}{j:D2}",
                        UserId = existingUser.Id,
                        CustomerPhone = $"021{random.Next(1000000, 9999999)}",
                        VendorEmail = vendor,
                        Status = statusToday,
                        TotalAmount = 0,
                        OrderDate = orderDateToday,
                        CreatedAt = orderDateToday,
                        UpdatedAt = orderDateToday,
                        PaymentMethod = paymentWithCurrencyToday,
                        TransactionId = $"TX-{Guid.NewGuid().ToString("N").Substring(12)}",
                        DeliveryType = random.Next(100) < 60 ? DeliveryType.Pickup : DeliveryType.Delivery,
                        DeliveryAddress = null,
                        OrderItems = new List<OrderItem>()
                    };

                    if (todayOrder.DeliveryType == DeliveryType.Delivery)
                    {
                        todayOrder.DeliveryAddress = $"{random.Next(1, 200)} Campus Ave, Palmerston North";
                    }

                    // Add 1-3 items from this vendor
                    var itemCountToday = random.Next(1, 4);
                    var selectedItemsToday = todaysMenuItems.OrderBy(_ => random.Next()).Take(itemCountToday).ToList();
                    foreach (var mi in selectedItemsToday)
                    {
                        var quantity = random.Next(1, 3);
                        var oi = new OrderItem
                        {
                            MenuItemId = mi.Id,
                            MenuItemName = mi.Name,
                            UnitPrice = mi.Price,
                            Quantity = quantity
                        };
                        todayOrder.OrderItems.Add(oi);
                        todayOrder.TotalAmount += oi.TotalPrice;
                    }

                    // Times
                    var prepMinutesToday = random.Next(5, 45);
                    todayOrder.EstimatedCompletionTime = todayOrder.OrderDate.AddMinutes(prepMinutesToday);
                    if (todayOrder.Status == OrderStatus.Completed)
                    {
                        todayOrder.CompletedTime = todayOrder.EstimatedCompletionTime?.AddMinutes(random.Next(0, 30));
                    }

                    todaysOrders.Add(todayOrder);
                }

                if (todaysOrders.Any())
                {
                    context.Orders.AddRange(todaysOrders);
                }
            }

            await context.SaveChangesAsync();
        }
        
        private static OrderStatus GetRandomOrderStatus(Random random)
        {
            var statuses = new[] { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.Completed };
            return statuses[random.Next(statuses.Length)];
        }
        
        private static async Task SeedReviews(ApplicationDbContext context)
        {
            // Clear existing reviews to ensure fresh English data
            var existingReviews = await context.Reviews.ToListAsync();
            if (existingReviews.Any())
            {
                context.Reviews.RemoveRange(existingReviews);
                await context.SaveChangesAsync();
            }
            
            {
                var completedOrders = await context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.Status == OrderStatus.Completed)
                    .ToListAsync();
                
                if (!completedOrders.Any()) return;

                var menuItems = await context.MenuItems.ToListAsync();
                var users = await context.Users.ToListAsync();
                var random = new Random();
                
                var reviewComments = new[]
                {
                    "The coffee is very fragrant, the milk foam is delicate, and the service attitude is great! Will come again.",
                    "The taste is good, but it's a bit bitter. I hope you can provide more sugar packets.",
                    "The dessert is exquisitely made, with rich taste layers and beautiful packaging. Highly recommended!",
                    "The coffee is average, the milk foam is a bit rough, but the environment is okay.",
                    "Waited a long time for the coffee, and the temperature was not hot enough. A bit disappointed.",
                    "Service is fast, coffee taste is rich, and staff attitude is friendly.",
                    "Reasonable price, sufficient portion, taste meets expectations.",
                    "Clean and tidy environment, stable coffee quality, will recommend to friends.",
                    "First time trying this store, overall experience is good, will come again next time.",
                    "The coffee temperature is just right, smooth taste, and the accompanying small cookies are also delicious."
                };
                
                var merchantReplies = new[]
                {
                    "Thank you for your positive review! We will continue to strive to provide quality coffee and service.",
                    "Very sorry for the poor experience. We have strengthened our service speed and temperature control. Welcome to visit again.",
                    "Thank you for your recommendation! Our pastry chef will be happy to hear your praise.",
                    "Thank you for the feedback. We will pay attention to improving the milk foam making process.",
                    "Thank you for your patience. We are optimizing the production process to improve service efficiency."
                };

                var reviews = new List<Review>();
                var reviewId = 1;

                foreach (var order in completedOrders) // Create reviews across all completed orders, capped at ~100
                {
                    if (reviews.Count >= 100) break;
                    foreach (var orderItem in order.OrderItems.Take(2)) // Maximum 2 product reviews per order
                    {
                        if (reviews.Count >= 100) break;
                        var menuItem = menuItems.FirstOrDefault(m => m.Id == orderItem.MenuItemId);
                        if (menuItem == null) continue;

                        var rating = random.Next(3, 6); // 3-5 star ratings
                        var commentIndex = random.Next(reviewComments.Length);
                        var hasReply = random.Next(1, 101) <= 70; // 70% probability of merchant reply
                        
                        var review = new Review
                        {
                            UserId = order.UserId,
                            OrderId = order.Id,
                            MenuItemId = orderItem.MenuItemId,
                            Rating = rating,
                            Comment = reviewComments[commentIndex],
                            CreatedAt = order.OrderDate.AddDays(random.Next(1, 5)), // Reviews 1-5 days after order completion
                            Status = hasReply ? ReviewStatus.Replied : ReviewStatus.Pending
                        };

                        if (hasReply)
                        {
                            review.MerchantReply = merchantReplies[random.Next(merchantReplies.Length)];
                            review.RepliedAt = review.CreatedAt.AddHours(random.Next(2, 24)); // Reply 2-24 hours after review
                        }

                        reviews.Add(review);
                    }
                }

                if (reviews.Any())
                {
                    context.Reviews.AddRange(reviews);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedFeedbacksZhDeprecated(ApplicationDbContext context)
        {
            // Wipe existing feedback to avoid duplication in dev/demo environments
            var existingFeedbacks = await context.Feedbacks.ToListAsync();
            if (existingFeedbacks.Any())
            {
                context.Feedbacks.RemoveRange(existingFeedbacks);
                await context.SaveChangesAsync();
            }

            // Load orders and users to associate feedback entries
            var orders = await context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            var users = await context.Users.ToListAsync();
            if (!users.Any()) return;

            var random = new Random();

            var subjects = new[]
            {
                "Taste Feedback on Order",
                "Delivery Speed Suggestions",
                "Payment Issue",
                "Technical Support Inquiry",
                "Service Experience Review",
                "Merchant Reply Suggestion",
                "Menu Improvement Suggestions",
                "Packaging Quality Feedback"
            };

            var messages = new[]
            {
                "Coffee tastes great but slightly bitter; please provide more sugar packets.",
                "Overall good; delivery could be faster, hot drinks arrive slightly cooled.",
                "Network fluctuation during payment caused delayed charge; please improve stability.",
                "App loads slowly at times; suggest caching to optimize page transitions.",
                "Staff were friendly and efficient; keep it up!",
                "Desserts have rich layers and creamy flavor; packaging is exquisite, recommended.",
                "Please add low-sugar and dairy-free options to the menu.",
                "Takeaway packaging was a bit loose; please reinforce to prevent leaks."
            };

            var categories = new[]
            {
                FeedbackCategory.FoodQuality,
                FeedbackCategory.DeliveryService,
                FeedbackCategory.PaymentIssue,
                FeedbackCategory.TechnicalSupport,
                FeedbackCategory.General,
                FeedbackCategory.Suggestion,
                FeedbackCategory.Suggestion,
                FeedbackCategory.OrderIssue
            };

            var priorities = new[]
            {
                FeedbackPriority.Low,
                FeedbackPriority.Medium,
                FeedbackPriority.High,
                FeedbackPriority.Urgent
            };

            // Create ~100 feedback entries, many linked to orders
            var targetCount = 100;
            var feedbacks = new List<Feedback>();

            DateTime RandomBusinessTimeUtc()
            {
                var daysBack = random.Next(0, 60);
                var date = DateTime.UtcNow.Date.AddDays(-daysBack);
                var hour = random.Next(8, 19); // 8-18
                var minute = random.Next(0, 60);
                var second = random.Next(0, 60);
                return new DateTime(date.Year, date.Month, date.Day, hour, minute, second, DateTimeKind.Utc);
            }

            for (int i = 0; i < targetCount; i++)
            {
                // pick a user and optionally an order to link
                var user = users[random.Next(users.Count)];
                Order? order = null;
                if (orders.Any() && random.Next(100) < 65) // 65% feedbacks relate to an order
                {
                    order = orders[random.Next(orders.Count)];
                }

                var idx = random.Next(subjects.Length);
                var created = RandomBusinessTimeUtc();
                var rating = random.Next(1, 6); // optional rating
                var priority = priorities[random.Next(priorities.Length)];
                var category = categories[idx % categories.Length];

                var fb = new Feedback
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    Subject = subjects[idx],
                    Message = messages[idx],
                    Category = category,
                    Rating = rating,
                    Priority = priority,
                    Status = FeedbackStatus.Open,
                    OrderId = order?.Id,
                    CreatedAt = created,
                    UpdatedAt = created
                };

                // Some feedback entries receive an admin response to showcase UI
                if (random.Next(100) < 40)
                {
                    fb.AdminResponse = category switch
                    {
                        FeedbackCategory.PaymentIssue => "We have contacted the payment provider to check stability. Thank you.",
                        FeedbackCategory.DeliveryService => "We have optimized rider dispatch and heat-preserving packaging. Thanks for the suggestion.",
                        FeedbackCategory.FoodQuality => "We will work with the kitchen to adjust taste. Please keep the feedback coming.",
                        FeedbackCategory.TechnicalSupport => "Tech team is improving loading and caching strategies. Thank you for your patience.",
                        _ => "Thanks for your feedback. We have recorded it and will continuously improve."
                    };
                    fb.AdminUserId = user.Id; // demo: reuse existing user id as admin placeholder
                    fb.ResponseDate = fb.CreatedAt.AddHours(random.Next(2, 48));
                    fb.Status = FeedbackStatus.Resolved;
                }

                feedbacks.Add(fb);
            }

            if (feedbacks.Any())
            {
                context.Feedbacks.AddRange(feedbacks);
                await context.SaveChangesAsync();
            }
        }
        private static async Task SeedFeedbacks(ApplicationDbContext context)
        {
            // Wipe existing feedback to avoid duplication in dev/demo environments
            var existingFeedbacks = await context.Feedbacks.ToListAsync();
            if (existingFeedbacks.Any())
            {
                context.Feedbacks.RemoveRange(existingFeedbacks);
                await context.SaveChangesAsync();
            }

            // Load orders and users to associate feedback entries
            var orders = await context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            var users = await context.Users.ToListAsync();
            if (!users.Any()) return;

            var random = new Random();

            var subjects = new[]
            {
                "Taste Feedback on Order",
                "Delivery Speed Suggestions",
                "Payment Issue",
                "Technical Support Inquiry",
                "Service Experience Review",
                "Merchant Reply Suggestion",
                "Menu Improvement Suggestions",
                "Packaging Quality Feedback"
            };

            var messages = new[]
            {
                "Coffee tastes great but slightly bitter; please provide more sugar packets.",
                "Overall good; delivery could be faster, hot drinks arrive slightly cooled.",
                "Network fluctuation during payment caused delayed charge; please improve stability.",
                "App loads slowly at times; suggest caching to optimize page transitions.",
                "Staff were friendly and efficient; keep it up!",
                "Desserts have rich layers and creamy flavor; packaging is exquisite, recommended.",
                "Please add low-sugar and dairy-free options to the menu.",
                "Takeaway packaging was a bit loose; please reinforce to prevent leaks."
            };

            var categories = new[]
            {
                FeedbackCategory.FoodQuality,
                FeedbackCategory.DeliveryService,
                FeedbackCategory.PaymentIssue,
                FeedbackCategory.TechnicalSupport,
                FeedbackCategory.General,
                FeedbackCategory.Suggestion,
                FeedbackCategory.Suggestion,
                FeedbackCategory.OrderIssue
            };

            var priorities = new[]
            {
                FeedbackPriority.Low,
                FeedbackPriority.Medium,
                FeedbackPriority.High,
                FeedbackPriority.Urgent
            };

            // Create ~100 feedback entries, many linked to orders
            var targetCount = 100;
            var feedbacks = new List<Feedback>();

            DateTime RandomBusinessTimeUtc()
            {
                var daysBack = random.Next(0, 60);
                var date = DateTime.UtcNow.Date.AddDays(-daysBack);
                var hour = random.Next(8, 19); // 8-18
                var minute = random.Next(0, 60);
                var second = random.Next(0, 60);
                return new DateTime(date.Year, date.Month, date.Day, hour, minute, second, DateTimeKind.Utc);
            }

            for (int i = 0; i < targetCount; i++)
            {
                // pick a user and optionally an order to link
                var user = users[random.Next(users.Count)];
                Order? order = null;
                if (orders.Any() && random.Next(100) < 65) // 65% feedbacks relate to an order
                {
                    order = orders[random.Next(orders.Count)];
                }

                var idx = random.Next(subjects.Length);
                var created = RandomBusinessTimeUtc();
                var rating = random.Next(1, 6); // optional rating
                var priority = priorities[random.Next(priorities.Length)];
                var category = categories[idx % categories.Length];

                var fb = new Feedback
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    Subject = subjects[idx],
                    Message = messages[idx],
                    Category = category,
                    Rating = rating,
                    Priority = priority,
                    Status = FeedbackStatus.Open,
                    OrderId = order?.Id,
                    CreatedAt = created,
                    UpdatedAt = created
                };

                // Some feedback entries receive an admin response to showcase UI
                if (random.Next(100) < 40)
                {
                    fb.AdminResponse = category switch
                    {
                        FeedbackCategory.PaymentIssue => "We have contacted the payment provider to check stability. Thank you.",
                        FeedbackCategory.DeliveryService => "We have optimized rider dispatch and heat-preserving packaging. Thanks for the suggestion.",
                        FeedbackCategory.FoodQuality => "We will work with the kitchen to adjust taste. Please keep the feedback coming.",
                        FeedbackCategory.TechnicalSupport => "Tech team is improving loading and caching strategies. Thank you for your patience.",
                        _ => "Thanks for your feedback. We have recorded it and will continuously improve."
                    };
                    fb.AdminUserId = user.Id; // demo: reuse existing user id as admin placeholder
                    fb.ResponseDate = fb.CreatedAt.AddHours(random.Next(2, 48));
                    fb.Status = FeedbackStatus.Resolved;
                }

                feedbacks.Add(fb);
            }

            if (feedbacks.Any())
            {
                context.Feedbacks.AddRange(feedbacks);
                await context.SaveChangesAsync();
            }
        }
    }
}