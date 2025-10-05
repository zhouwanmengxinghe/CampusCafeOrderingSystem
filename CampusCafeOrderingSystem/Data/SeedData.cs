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
            if (!await context.MenuItems.AnyAsync(m => m.VendorEmail == vendorEmail))
            {
                var menuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Americano",
                        Description = "Rich and bold coffee with hot water",
                        Price = 3.50m,
                        Category = "Coffee",
                        ImageUrl = "/images/americano.jpg",
                        IsAvailable = true,
                        VendorEmail = vendorEmail,
                        CreatedAt = DateTime.UtcNow
                    },
                    new MenuItem
                    {
                        Name = "Latte",
                        Description = "Smooth espresso with steamed milk",
                        Price = 4.50m,
                        Category = "Coffee",
                        ImageUrl = "/images/latte.jpg",
                        IsAvailable = true,
                        VendorEmail = vendorEmail,
                        CreatedAt = DateTime.UtcNow
                    },
                    new MenuItem
                    {
                        Name = "Cappuccino",
                        Description = "Espresso with frothy milk foam",
                        Price = 4.00m,
                        Category = "Coffee",
                        ImageUrl = "/images/cappuccino.jpg",
                        IsAvailable = true,
                        VendorEmail = vendorEmail,
                        CreatedAt = DateTime.UtcNow
                    },
                    new MenuItem
                    {
                        Name = "Green Tea",
                        Description = "Fresh and healthy green tea",
                        Price = 2.50m,
                        Category = "Tea",
                        ImageUrl = "/images/green-tea.jpg",
                        IsAvailable = true,
                        VendorEmail = vendorEmail,
                        CreatedAt = DateTime.UtcNow
                    },
                    new MenuItem
                    {
                        Name = "Chocolate Cake",
                        Description = "Rich chocolate cake slice",
                        Price = 5.50m,
                        Category = "Dessert",
                        ImageUrl = "/images/chocolate-cake.jpg",
                        IsAvailable = true,
                        VendorEmail = vendorEmail,
                        CreatedAt = DateTime.UtcNow
                    },
                    new MenuItem
                    {
                        Name = "Croissant",
                        Description = "Buttery and flaky pastry",
                        Price = 3.00m,
                        Category = "Snack",
                        ImageUrl = "/images/croissant.jpg",
                        IsAvailable = true,
                        VendorEmail = vendorEmail,
                        CreatedAt = DateTime.UtcNow
                    }
                };
                
                context.MenuItems.AddRange(menuItems);
                await context.SaveChangesAsync();
            }
        }
        
        private static async Task SeedOrders(ApplicationDbContext context)
        {
            var vendorEmail = "vendor@campuscafe.com";
            
            if (!await context.Orders.AnyAsync(o => o.VendorEmail == vendorEmail))
            {
                var menuItems = await context.MenuItems
                    .Where(m => m.VendorEmail == vendorEmail)
                    .ToListAsync();
                
                if (menuItems.Any())
                {
                    var orders = new List<Order>();
                    var random = new Random();
                    
                    for (int i = 1; i <= 10; i++)
                    {
                        var orderDate = DateTime.UtcNow.AddDays(-random.Next(1, 30));
                        // Use existing user ID (admin or customer)
                        var existingUser = await context.Users.FirstOrDefaultAsync();
                        if (existingUser == null) continue;
                        
                        var order = new Order
                        {
                            OrderNumber = $"ORD{DateTime.Now.Ticks}{i:D3}",
                            UserId = existingUser.Id,
                            CustomerPhone = $"138{i:D8}",
                            VendorEmail = vendorEmail,
                            Status = GetRandomOrderStatus(random),
                            TotalAmount = 0,
                            OrderDate = orderDate,
                            CreatedAt = orderDate,
                            OrderItems = new List<OrderItem>()
                        };
                        
                        // Add 1-3 random items to each order
                        var itemCount = random.Next(1, 4);
                        var selectedItems = menuItems.OrderBy(x => random.Next()).Take(itemCount);
                        
                        foreach (var menuItem in selectedItems)
                        {
                            var quantity = random.Next(1, 3);
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
                        
                        orders.Add(order);
                    }
                    
                    context.Orders.AddRange(orders);
                    await context.SaveChangesAsync();
                }
            }
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

                foreach (var order in completedOrders.Take(15)) // Create reviews for first 15 completed orders
                {
                    foreach (var orderItem in order.OrderItems.Take(2)) // Maximum 2 product reviews per order
                    {
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
    }
}