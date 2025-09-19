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
        }
        
        private static async Task SeedMenuItems(ApplicationDbContext context)
        {
            var vendorEmail = "vendor@campuscafe.com";
            
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
                        // 使用已存在的用户ID（admin或customer）
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
    }
}