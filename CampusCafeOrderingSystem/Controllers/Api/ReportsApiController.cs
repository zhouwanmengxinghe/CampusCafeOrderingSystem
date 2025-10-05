using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampusCafeOrderingSystem.Services;
using CampusCafeOrderingSystem.Models;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);
        private readonly Random _random = new Random();
        private readonly bool _useDemoData = false; // Force use of real data

        public ReportsApiController(ApplicationDbContext context, IOrderService orderService, IMemoryCache cache)
        {
            _context = context;
            _orderService = orderService;
            _cache = cache;
        }
        
        #region Helper methods for generating demo data
        
        private object GenerateDemoOverviewData(DateTime startDate, DateTime endDate)
        {
            // Generate total revenue (random value between 5000-15000)
            decimal totalRevenue = _random.Next(5000, 15000) + (decimal)_random.NextDouble();
            totalRevenue = Math.Round(totalRevenue, 2);
            
            // Generate total orders (random value between 100-300)
            int totalOrders = _random.Next(100, 300);
            
            // Calculate average order value
            decimal avgOrderValue = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0;
            
            // Generate most popular items
            string[] popularItems = new[] { "Latte", "Americano", "Cappuccino", "Espresso", "Croissant", "Caesar Salad", "Chocolate Cookie" };
            string topProduct = popularItems[_random.Next(popularItems.Length)];
            
            return new
            {
                totalRevenue,
                totalOrders,
                avgOrderValue,
                topProduct
            };
        }
        
        private object GenerateDemoDailyData(DateTime startDate, DateTime endDate)
        {
            var dailyData = new List<object>();
            var labels = new List<string>();
            var revenueData = new List<decimal>();
            var ordersData = new List<int>();
            
            // Generate daily data
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Generate daily revenue (random value between 200-1000)
                decimal revenue = _random.Next(200, 1000) + (decimal)_random.NextDouble();
                revenue = Math.Round(revenue, 2);
                
                // Generate daily orders (random value between 10-50)
                int orders = _random.Next(10, 50);
                
                // Calculate average order value
                decimal avgOrderValue = orders > 0 ? Math.Round(revenue / orders, 2) : 0;
                
                // Generate growth rate (random value between -10% to 20%)
                decimal growth = (_random.Next(-10, 20) + (decimal)_random.NextDouble());
                growth = Math.Round(growth, 1);
                
                dailyData.Add(new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    revenue,
                    orders,
                    avgOrderValue,
                    growth
                });
                
                labels.Add(date.ToString("MM/dd"));
                revenueData.Add(revenue);
                ordersData.Add(orders);
            }
            
            return new
            {
                dailyData,
                chartData = new
                {
                    labels,
                    revenue = revenueData,
                    orders = ordersData
                }
            };
        }
        
        private object GenerateDemoHourlyData()
        {
            var hourlyData = new List<object>();
            var labels = new List<string>();
            var ordersData = new List<int>();
            var revenueData = new List<decimal>();
            
            // Generate hourly data
            for (int hour = 0; hour < 24; hour++)
            {
                // Morning and afternoon peak hours have higher order volumes
                int orderMultiplier = 1;
                if (hour >= 7 && hour <= 9) orderMultiplier = 3; // Morning peak
                if (hour >= 11 && hour <= 13) orderMultiplier = 4; // Lunch peak
                if (hour >= 17 && hour <= 19) orderMultiplier = 3; // Evening peak
                
                // Generate current hour order count
                int orders = _random.Next(1, 10) * orderMultiplier;
                
                // Generate current hour revenue
                decimal revenue = orders * (_random.Next(20, 50) + (decimal)_random.NextDouble());
                revenue = Math.Round(revenue, 2);
                
                hourlyData.Add(new
                {
                    hour,
                    orders,
                    revenue
                });
                
                labels.Add($"{hour}:00");
                ordersData.Add(orders);
                revenueData.Add(revenue);
            }
            
            return new
            {
                hourlyData,
                chartData = new
                {
                    labels,
                    orders = ordersData,
                    revenue = revenueData
                }
            };
        }
        
        private object GenerateDemoProductRanking()
        {
            var products = new[]
            {
                new { name = "Latte", category = "Coffee" },
                new { name = "Americano", category = "Coffee" },
                new { name = "Cappuccino", category = "Coffee" },
                new { name = "Matcha Latte", category = "Tea" },
                new { name = "Black Tea Latte", category = "Tea" },
                new { name = "Strawberry Cake", category = "Dessert" },
                new { name = "Chocolate Cookie", category = "Dessert" },
                new { name = "Tiramisu", category = "Dessert" },
                new { name = "Sandwich", category = "Snack" },
                new { name = "Fruit Salad", category = "Snack" }
            };
            
            var productRanking = new List<object>();
            var labels = new List<string>();
            var quantityData = new List<int>();
            var revenueData = new List<decimal>();
            
            // Generate sales and revenue data for each product
            foreach (var product in products)
            {
                // Generate sales quantity (random value between 10-100)
                int quantity = _random.Next(10, 100);
                
                // Generate unit price (random value between 20-50)
                decimal unitPrice = _random.Next(20, 50) + (decimal)_random.NextDouble();
                unitPrice = Math.Round(unitPrice, 2);
                
                // Calculate total revenue
                decimal revenue = quantity * unitPrice;
                revenue = Math.Round(revenue, 2);
                
                productRanking.Add(new
                {
                    name = product.name,
                    category = product.category,
                    quantity,
                    revenue,
                    unitPrice
                });
                
                labels.Add(product.name);
                quantityData.Add(quantity);
                revenueData.Add(revenue);
            }
            
            // Sort by sales quantity
            productRanking = productRanking.OrderByDescending(p => ((dynamic)p).quantity).ToList();
            
            // Get top 5 products data for chart
            var top5Products = productRanking.Take(5).ToList();
            var top5Labels = top5Products.Select(p => ((dynamic)p).name).ToList();
            var top5Quantity = top5Products.Select(p => (int)((dynamic)p).quantity).ToList();
            var top5Revenue = top5Products.Select(p => (decimal)((dynamic)p).revenue).ToList();
            
            return new
            {
                productRanking,
                chartData = new
                {
                    labels = top5Labels,
                    quantity = top5Quantity,
                    revenue = top5Revenue
                }
            };
        }
        
        private object GenerateDemoCategoryData()
        {
            var categories = new[] { "Coffee", "Tea", "Dessert", "Snack" };
            var categoryData = new List<object>();
            var labels = new List<string>();
            var salesData = new List<decimal>();
            var percentageData = new List<decimal>();
            
            decimal totalSales = 0;
            
            // Generate sales data for each category
            foreach (var category in categories)
            {
                // Generate sales amount (random value between 500-2000)
                decimal sales = _random.Next(500, 2000) + (decimal)_random.NextDouble();
                sales = Math.Round(sales, 2);
                
                totalSales += sales;
                
                categoryData.Add(new
                {
                    category,
                    sales,
                    percentage = 0m // Set to 0 first, calculate later
                });
                
                labels.Add(category);
                salesData.Add(sales);
            }
            
            // Calculate percentage for each category
            if (totalSales > 0)
            {
                for (int i = 0; i < categoryData.Count; i++)
                {
                    var item = (dynamic)categoryData[i];
                    decimal percentage = Math.Round(item.sales / totalSales * 100, 1);
                    
                    // Create new object to replace the original object
                    categoryData[i] = new
                    {
                        category = item.category,
                        sales = item.sales,
                        percentage
                    };
                    
                    percentageData.Add(percentage);
                }
            }
            
            return new
            {
                categoryData,
                chartData = new
                {
                    labels,
                    sales = salesData,
                    percentage = percentageData
                }
            };
        }
        
        private List<object> GenerateDemoRecentOrders()
        {
            var statuses = new[] { "Pending", "Preparing", "Ready", "InDelivery", "Completed" };
            var customerNames = new[] { "John Smith", "Emma Johnson", "Michael Brown", "Sophia Davis", "William Wilson", "Olivia Taylor", "James Anderson", "Emily Thomas" };
            var recentOrders = new List<object>();
            
            // Generate 10 recent orders
            for (int i = 0; i < 10; i++)
            {
                // Generate order number
                string orderNumber = $"ORD{DateTime.Now.ToString("yyyyMMdd")}{_random.Next(1000, 9999)}";
                
                // Randomly select customer name
                string customerName = customerNames[_random.Next(customerNames.Length)];
                
                // Generate order amount (random value between 50-200)
                decimal totalAmount = _random.Next(50, 200) + (decimal)_random.NextDouble();
                totalAmount = Math.Round(totalAmount, 2);
                
                // Randomly select order status
                string status = statuses[_random.Next(statuses.Length)];
                
                // Generate order creation time (random time within the past 24 hours)
                DateTime createdAt = DateTime.Now.AddHours(-_random.Next(0, 24)).AddMinutes(-_random.Next(0, 60));
                
                recentOrders.Add(new
                {
                    orderNumber,
                    customerName,
                    totalAmount,
                    status,
                    createdAt
                });
            }
            
            // Sort by creation time, newest first
            return recentOrders.OrderByDescending(o => ((dynamic)o).createdAt).ToList();
        }
        
        #endregion

        [HttpPost("clear-cache")]
        // Note: This endpoint intentionally does not add [Authorize] attribute, allowing internal service calls
        // Used for inter-service communication within the system, clearing cache
        public IActionResult ClearCache([FromBody] JsonElement requestBody)
        {
            try
            {
                // Extract vendorEmail from request body
                string vendorEmail = null;
                
                // Try to get vendorEmail property from JSON object
                if (requestBody.TryGetProperty("vendorEmail", out JsonElement vendorEmailElement) && 
                    vendorEmailElement.ValueKind == JsonValueKind.String)
                {
                    vendorEmail = vendorEmailElement.GetString();
                }
                // If no vendorEmail property, try to use the entire request body as string
                else if (requestBody.ValueKind == JsonValueKind.String)
                {
                    vendorEmail = requestBody.GetString();
                }
                
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return BadRequest(new { message = "Invalid vendor email" });
                }

                // Clear all report cache for the specified vendor
                var today = DateTime.Today;
                var patterns = new[]
                {
                    $"dashboard_data_{vendorEmail}_",
                    $"overview_{vendorEmail}_",
                    $"daily_data_{vendorEmail}_",
                    $"category_data_{vendorEmail}_",
                    $"sales_data_{vendorEmail}_",
                    $"popular_items_{vendorEmail}_"
                };

                // Since IMemoryCache doesn't have direct pattern matching deletion method, we need to clear possible date ranges
                for (int i = -30; i <= 30; i++)
                {
                    var date = today.AddDays(i);
                    var dateStr = date.ToString("yyyyMMdd");
                    
                    foreach (var pattern in patterns)
                    {
                        // Clear single day cache
                        _cache.Remove($"{pattern}{dateStr}_{dateStr}");
                        _cache.Remove($"{pattern}{dateStr}"); // popular_items_ key only contains single day date
                        
                        // Clear possible date range cache
                        for (int j = i; j <= 30; j++)
                        {
                            var endDate = today.AddDays(j);
                            var endDateStr = endDate.ToString("yyyyMMdd");
                            _cache.Remove($"{pattern}{dateStr}_{endDateStr}");
                        }
                    }
                }

                return Ok(new { message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to clear cache", error = ex.Message });
            }
        }

        [HttpGet("dashboard-data")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetDashboardData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // If demo data is enabled, return demo data
                if (_useDemoData)
                {
                    var demoOverviewData = GenerateDemoOverviewData(startDate, endDate);
                    var demoDailyData = GenerateDemoDailyData(startDate, endDate);
                    var demoHourlyData = GenerateDemoHourlyData();
                    var demoProductRanking = GenerateDemoProductRanking();
                    var demoCategoryData = GenerateDemoCategoryData();
                    
                    var demoResult = new
                    {
                        overview = demoOverviewData,
                        dailyData = ((dynamic)demoDailyData).dailyData,
                        hourlyData = ((dynamic)demoHourlyData).hourlyData,
                        productRanking = ((dynamic)demoProductRanking).productRanking,
                        categoryData = ((dynamic)demoCategoryData).categoryData,
                        chartData = new
                        {
                            daily = ((dynamic)demoDailyData).chartData,
                            hourly = ((dynamic)demoHourlyData).chartData,
                            products = ((dynamic)demoProductRanking).chartData,
                            category = ((dynamic)demoCategoryData).chartData
                        }
                    };
                    
                    return Ok(demoResult);
                }
                
                // The following is the original actual data retrieval logic
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // Generate cache key
                var cacheKey = $"dashboard_data_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // Try to get data from cache
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                // Get current vendor's related order data (including all statuses)
                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .ToListAsync();

                // Only use completed orders for statistical calculations
                var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();

                // 1. Overview data
                var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
                var totalOrders = completedOrders.Count;
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
                var topProduct = completedOrders
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.MenuItemName)
                    .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                    .Select(g => g.Key)
                    .FirstOrDefault();

                var overview = new
                {
                    totalRevenue,
                    totalOrders,
                    avgOrderValue,
                    topProduct
                };

                // 2. Daily data
                var dailyDataTemp = completedOrders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        revenue = g.Sum(o => o.TotalAmount),
                        orders = g.Count(),
                        avgOrderValue = g.Average(o => o.TotalAmount)
                    })
                    .OrderBy(d => d.date)
                    .ToList();

                // Calculate growth rate and create final data
                var dailyData = new List<object>();
                for (int i = 0; i < dailyDataTemp.Count; i++)
                {
                    var current = dailyDataTemp[i];
                    var previous = i > 0 ? dailyDataTemp[i - 1] : null;
                    
                    var revenueGrowth = previous != null && previous.revenue > 0 
                        ? ((current.revenue - previous.revenue) / previous.revenue * 100) 
                        : 0;
                    
                    var orderGrowth = previous != null && previous.orders > 0 
                        ? ((current.orders - previous.orders) / (decimal)previous.orders * 100) 
                        : 0;

                    dailyData.Add(new
                    {
                        date = current.date,
                        revenue = current.revenue,
                        orders = current.orders,
                        avgOrderValue = current.avgOrderValue,
                        growth = revenueGrowth
                    });
                }

                // 3. Hourly analysis data
                var hourlyData = completedOrders
                    .GroupBy(o => o.OrderDate.Hour)
                    .Select(g => new
                    {
                        hour = g.Key,
                        orders = g.Count(),
                        revenue = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(h => h.hour)
                    .ToList();

                // 4. Product ranking data
                var productRanking = completedOrders
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.MenuItemName)
                    .Select(g => new
                    {
                        name = g.Key,
                        quantity = g.Sum(oi => oi.Quantity),
                        revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                    })
                    .OrderByDescending(p => p.quantity)
                    .Take(10)
                    .ToList();

                // 5. Category data
                var categoryData = completedOrders
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => GetCategoryByName(oi.MenuItemName))
                    .Select(g => new
                    {
                        category = g.Key,
                        quantity = g.Sum(oi => oi.Quantity),
                        revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                    })
                    .ToList();

                var result = new
                {
                    overview,
                    dailyData,
                    hourlyData,
                    productRanking,
                    categoryData
                };

                // Store result in cache
                _cache.Set(cacheKey, result, _cacheExpiration);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get report data", error = ex.Message });
            }
        }

        [HttpGet("overview")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetReportOverview([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // If demo data is enabled, return demo data
                if (_useDemoData)
                {
                    var demoOverview = GenerateDemoOverviewData(startDate, endDate);
                    return Ok(demoOverview);
                }
                
                // The following is the original actual data retrieval logic
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // Generate cache key
                var cacheKey = $"overview_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // Try to get data from cache
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                    .Include(o => o.OrderItems)
                    .ToListAsync();

                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var totalOrders = orders.Count;
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                // Get best-selling product
                var topProduct = await _context.OrderItems
                    .Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId))
                    .GroupBy(oi => oi.MenuItemName)
                    .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                    .Select(g => g.Key)
                    .FirstOrDefaultAsync();

                var overview = new
                {
                    totalRevenue,
                    totalOrders,
                    avgOrderValue,
                    topProduct = topProduct ?? "No data available"
                };

                // Store result in cache
                _cache.Set(cacheKey, overview, _cacheExpiration);

                return Ok(overview);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to get report overview", error = ex.Message });
            }
        }

        [HttpGet("daily-data")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetDailyData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // If demo data is enabled, return demo data
                if (_useDemoData)
                {
                    var demoDailyData = GenerateDemoDailyData(startDate, endDate);
                    return Ok(demoDailyData);
                }
                
                // The following is the original actual data retrieval logic
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // Generate cache key
                var cacheKey = $"daily_data_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // Try to get data from cache
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                // Optimization: Use single query to get all data, avoid loop queries
                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                    .Select(o => new { o.OrderDate, o.TotalAmount })
                    .ToListAsync();

                // Group by date to calculate daily data
                var dailyStats = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .ToDictionary(g => g.Key, g => new
                    {
                        orders = g.Count(),
                        revenue = g.Sum(o => o.TotalAmount),
                        avgOrder = g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
                    });

                var dailyData = new List<object>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    var currentStats = dailyStats.ContainsKey(currentDate.Date) 
                        ? dailyStats[currentDate.Date] 
                        : new { orders = 0, revenue = 0m, avgOrder = 0m };

                    // Calculate growth rate (compared to previous day)
                    var previousDay = currentDate.AddDays(-1).Date;
                    var previousStats = dailyStats.ContainsKey(previousDay) 
                        ? dailyStats[previousDay] 
                        : new { orders = 0, revenue = 0m, avgOrder = 0m };
                    
                    var growth = previousStats.revenue > 0 
                        ? ((currentStats.revenue - previousStats.revenue) / previousStats.revenue) * 100 
                        : 0;

                    dailyData.Add(new
                    {
                        date = currentDate.ToString("yyyy-MM-dd"),
                        orders = currentStats.orders,
                        revenue = currentStats.revenue,
                        avgOrder = currentStats.avgOrder,
                        growth
                    });

                    currentDate = currentDate.AddDays(1);
                }

                // Store result in cache
                _cache.Set(cacheKey, dailyData, _cacheExpiration);

                return Ok(dailyData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to get daily sales data", error = ex.Message });
            }
        }

        [HttpGet("product-ranking")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetProductRanking([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // If demo data is enabled, return demo data
                if (_useDemoData)
                {
                    var demoProductRanking = GenerateDemoProductRanking();
                    return Ok(demoProductRanking);
                }
                
                // The following is the original actual data retrieval logic
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // Generate cache key
                var cacheKey = $"product_ranking_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // Try to get data from cache
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                    .Include(o => o.OrderItems)
                    .ToListAsync();

                var totalRevenue = orders.Sum(o => o.TotalAmount);

                var productRanking = await _context.OrderItems
                    .Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId))
                    .GroupBy(oi => oi.MenuItemName)
                    .Select(g => new
                    {
                        name = g.Key,
                        quantity = g.Sum(oi => oi.Quantity),
                        revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity),
                        percentage = totalRevenue > 0 ? (g.Sum(oi => oi.UnitPrice * oi.Quantity) / totalRevenue) * 100 : 0
                    })
                    .OrderByDescending(p => p.quantity)
                    .Take(10)
                    .ToListAsync();

                var rankedProducts = productRanking.Select((p, index) => new
                {
                    rank = index + 1,
                    p.name,
                    p.quantity,
                    p.revenue,
                    p.percentage
                });

                // Store result in cache
                _cache.Set(cacheKey, rankedProducts, _cacheExpiration);

                return Ok(rankedProducts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to get product ranking", error = ex.Message });
            }
        }

        [HttpGet("hourly-analysis")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetHourlyAnalysis([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // If demo data is enabled, return demo data
                if (_useDemoData)
                {
                    var hourlyData = GenerateDemoHourlyData();
                    return Ok(hourlyData);
                }
                
                // The following is the original actual data retrieval logic
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // Generate cache key
                var cacheKey = $"hourly_analysis_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // Try to get data from cache
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                    .ToListAsync();

                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var totalOrders = orders.Count;

                var hourlyAnalysis = orders
                    .GroupBy(o => new
                    {
                        Hour = o.OrderDate.Hour switch
                        {
                            >= 8 and < 10 => "08:00-10:00",
                            >= 10 and < 12 => "10:00-12:00",
                            >= 12 and < 14 => "12:00-14:00",
                            >= 14 and < 16 => "14:00-16:00",
                            >= 16 and < 18 => "16:00-18:00",
                            >= 18 and < 20 => "18:00-20:00",
                            _ => "Other time periods"
                        }
                    })
                    .Select(g => new
                    {
                        hour = g.Key.Hour,
                        orders = g.Count(),
                        revenue = g.Sum(o => o.TotalAmount),
                        percentage = totalRevenue > 0 ? (g.Sum(o => o.TotalAmount) / totalRevenue) * 100 : 0
                    })
                    .OrderBy(h => h.hour)
                    .ToList();

                // Store result in cache
                _cache.Set(cacheKey, hourlyAnalysis, _cacheExpiration);

                return Ok(hourlyAnalysis);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to get time period analysis", error = ex.Message });
            }
        }

        [HttpGet("category-data")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetCategoryData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // If demo data is enabled, return demo data
                if (_useDemoData)
                {
                    var demoCategoryData = GenerateDemoCategoryData();
                    return Ok(demoCategoryData);
                }
                
                // The following is the original actual data retrieval logic
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // Generate cache key
                var cacheKey = $"category_data_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // Try to get data from cache
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                    .Include(o => o.OrderItems)
                    .ToListAsync();

                var totalRevenue = orders.Sum(o => o.TotalAmount);

                // Infer category by menu item name (simplified processing)
                var categoryData = await _context.OrderItems
                    .Where(oi => orders.Select(o => o.Id).Contains(oi.OrderId))
                    .ToListAsync();

                var categoryRevenue = categoryData
                    .GroupBy(oi => GetCategoryByName(oi.MenuItemName))
                    .Select(g => new
                    {
                        category = g.Key,
                        revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity),
                        percentage = totalRevenue > 0 ? (g.Sum(oi => oi.UnitPrice * oi.Quantity) / totalRevenue) * 100 : 0
                    })
                    .OrderByDescending(c => c.revenue)
                    .ToList();

                // Store result in cache
                _cache.Set(cacheKey, categoryRevenue, _cacheExpiration);

                return Ok(categoryRevenue);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to get category data", error = ex.Message });
            }
        }

        private string GetCategoryByName(string menuItemName)
        {
            // Simplified category logic, in actual projects should get category from MenuItem table
            if (menuItemName.Contains("Coffee") || menuItemName.Contains("Latte") || menuItemName.Contains("Americano") || menuItemName.Contains("Cappuccino"))
                return "Coffee";
            if (menuItemName.Contains("Cake") || menuItemName.Contains("Dessert") || menuItemName.Contains("Tiramisu"))
                return "Desserts";
            if (menuItemName.Contains("Tea") || menuItemName.Contains("Milk Tea"))
                return "Tea";
            return "Snacks";
        }
        
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .ToListAsync();
                
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                using var package = new ExcelPackage();
                
                // Create business overview worksheet
                var overviewSheet = package.Workbook.Worksheets.Add("Business Overview");
                
                // Set title
                overviewSheet.Cells[1, 1].Value = "Campus Cafe Business Report";
                overviewSheet.Cells[1, 1, 1, 4].Merge = true;
                overviewSheet.Cells[1, 1].Style.Font.Size = 16;
                overviewSheet.Cells[1, 1].Style.Font.Bold = true;
                overviewSheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                overviewSheet.Cells[2, 1].Value = $"Report Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
                overviewSheet.Cells[2, 1, 2, 4].Merge = true;
                overviewSheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                // Business overview data
                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var totalOrders = orders.Count;
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
                
                overviewSheet.Cells[4, 1].Value = "Metric";
                overviewSheet.Cells[4, 2].Value = "Value";
                overviewSheet.Cells[5, 1].Value = "Total Revenue";
                overviewSheet.Cells[5, 2].Value = totalRevenue;
                overviewSheet.Cells[6, 1].Value = "Total Orders";
                overviewSheet.Cells[6, 2].Value = totalOrders;
                overviewSheet.Cells[7, 1].Value = "Average Order Amount";
                overviewSheet.Cells[7, 2].Value = avgOrderValue;
                
                // Set formatting
                overviewSheet.Cells[4, 1, 4, 2].Style.Font.Bold = true;
                overviewSheet.Cells[5, 2].Style.Numberformat.Format = "¥#,##0.00";
                overviewSheet.Cells[7, 2].Style.Numberformat.Format = "¥#,##0.00";
                
                // Create order details worksheet
                var ordersSheet = package.Workbook.Worksheets.Add("Order Details");
                
                // Set table headers
                ordersSheet.Cells[1, 1].Value = "Order Number";
                ordersSheet.Cells[1, 2].Value = "Order Time";
                ordersSheet.Cells[1, 3].Value = "Customer Phone";
                ordersSheet.Cells[1, 4].Value = "Item";
                ordersSheet.Cells[1, 5].Value = "Quantity";
                ordersSheet.Cells[1, 6].Value = "Unit Price";
                ordersSheet.Cells[1, 7].Value = "Subtotal";
                ordersSheet.Cells[1, 8].Value = "Order Total";
                
                ordersSheet.Cells[1, 1, 1, 8].Style.Font.Bold = true;
                ordersSheet.Cells[1, 1, 1, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ordersSheet.Cells[1, 1, 1, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                
                // Fill order data
                int row = 2;
                foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                {
                    foreach (var item in order.OrderItems)
                    {
                        ordersSheet.Cells[row, 1].Value = order.OrderNumber;
                        ordersSheet.Cells[row, 2].Value = order.OrderDate;
                        ordersSheet.Cells[row, 3].Value = order.CustomerPhone;
                        ordersSheet.Cells[row, 4].Value = item.MenuItemName;
                        ordersSheet.Cells[row, 5].Value = item.Quantity;
                        ordersSheet.Cells[row, 6].Value = item.UnitPrice;
                        ordersSheet.Cells[row, 7].Value = item.TotalPrice;
                        ordersSheet.Cells[row, 8].Value = order.TotalAmount;
                        
                        ordersSheet.Cells[row, 2].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";
                        ordersSheet.Cells[row, 6].Style.Numberformat.Format = "¥#,##0.00";
                        ordersSheet.Cells[row, 7].Style.Numberformat.Format = "¥#,##0.00";
                        ordersSheet.Cells[row, 8].Style.Numberformat.Format = "¥#,##0.00";
                        
                        row++;
                    }
                }
                
                // Auto-adjust column width
                overviewSheet.Cells.AutoFitColumns();
                ordersSheet.Cells.AutoFitColumns();
                
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                
                var fileName = $"Business_Report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Excel export failed", details = ex.Message });
            }
        }
        
        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportToPDF([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                    .Include(o => o.OrderItems)
                    .ToListAsync();
                
                using var stream = new MemoryStream();
                var document = new Document(PageSize.A4, 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(document, stream);
                
                document.Open();
                
                // Set font for PDF generation
                var baseFont = BaseFont.CreateFont("STSong-Light", "UniGB-UCS2-H", BaseFont.NOT_EMBEDDED);
                var titleFont = new Font(baseFont, 18, Font.BOLD);
                var headerFont = new Font(baseFont, 14, Font.BOLD);
                var normalFont = new Font(baseFont, 10, Font.NORMAL);
                
                // Add title
                var title = new Paragraph("Campus Cafe Business Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(title);
                
                var dateRange = new Paragraph($"Report Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(dateRange);
                
                // Business overview
                var overviewTitle = new Paragraph("Business Overview", headerFont)
                {
                    SpacingAfter = 10
                };
                document.Add(overviewTitle);
                
                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var totalOrders = orders.Count;
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
                
                var overviewTable = new PdfPTable(2)
                {
                    WidthPercentage = 50,
                    SpacingAfter = 20
                };
                overviewTable.SetWidths(new float[] { 1, 1 });
                
                overviewTable.AddCell(new PdfPCell(new Phrase("Total Revenue", normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase($"¥{totalRevenue:F2}", normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase("Total Orders", normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase(totalOrders.ToString(), normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase("Average Order Amount", normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase($"¥{avgOrderValue:F2}", normalFont)));
                
                document.Add(overviewTable);
                
                // Order details
                var ordersTitle = new Paragraph("Order Details", headerFont)
                {
                    SpacingAfter = 10
                };
                document.Add(ordersTitle);
                
                var ordersTable = new PdfPTable(6)
                {
                    WidthPercentage = 100
                };
                ordersTable.SetWidths(new float[] { 1.5f, 1.5f, 1f, 2f, 0.8f, 1f });
                
                // Table headers
                ordersTable.AddCell(new PdfPCell(new Phrase("Order Number", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("Order Time", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("Customer Phone", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("Items", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("Quantity", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("Amount", headerFont)));
                
                // Data rows
                foreach (var order in orders.OrderByDescending(o => o.OrderDate).Take(50)) // Limit to first 50 orders
                {
                    var itemsText = string.Join(", ", order.OrderItems.Select(oi => $"{oi.MenuItemName}×{oi.Quantity}"));
                    
                    ordersTable.AddCell(new PdfPCell(new Phrase(order.OrderNumber, normalFont)));
                    ordersTable.AddCell(new PdfPCell(new Phrase(order.OrderDate.ToString("MM-dd HH:mm"), normalFont)));
                    ordersTable.AddCell(new PdfPCell(new Phrase(order.CustomerPhone ?? "", normalFont)));
                    ordersTable.AddCell(new PdfPCell(new Phrase(itemsText, normalFont)));
                    ordersTable.AddCell(new PdfPCell(new Phrase(order.OrderItems.Sum(oi => oi.Quantity).ToString(), normalFont)));
                    ordersTable.AddCell(new PdfPCell(new Phrase($"¥{order.TotalAmount:F2}", normalFont)));
                }
                
                document.Add(ordersTable);
                
                if (orders.Count > 50)
                {
                    var note = new Paragraph($"Note: Only showing first 50 orders, total {orders.Count} orders", normalFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 10
                    };
                    document.Add(note);
                }
                
                document.Close();
                
                var fileName = $"Business_Report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to export PDF", details = ex.Message });
            }
        }

        // Admin dashboard data interface
        [HttpGet("admin-dashboard-data")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboardData([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                // Set default date range (if not provided)
                // Normalize: start at 00:00 of the day, end at 23:59:59.9999999 of the day to ensure full day data is included
                var start = (startDate ?? DateTime.Now.AddDays(-30)).Date;
                var end = ((endDate ?? DateTime.Now).Date).AddDays(1).AddTicks(-1);

                // Generate cache key
                var cacheKey = $"admin_dashboard_data_{start:yyyyMMdd}_{end:yyyyMMdd}";
                
                // Try to get data from cache
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                // Get all order data (no merchant restriction)
                var allOrders = await _context.Orders
                    .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .ToListAsync();

                // Only use completed orders for statistical calculations
                var completedOrders = allOrders.Where(o => o.Status == OrderStatus.Completed).ToList();

                // 1. Overview data
                var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
                var totalOrders = completedOrders.Count;
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
                var topProduct = completedOrders
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.MenuItemName)
                    .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "No data";

                var overview = new
                {
                    totalRevenue,
                    totalOrders,
                    avgOrderValue,
                    topProduct
                };

                // 2. Daily data
                var dailyDataTemp = completedOrders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        revenue = g.Sum(o => o.TotalAmount),
                        orders = g.Count(),
                        avgOrderValue = g.Average(o => o.TotalAmount)
                    })
                    .OrderBy(d => d.date)
                    .ToList();

                // Calculate growth rate and create final data
                var dailyData = new List<object>();
                for (int i = 0; i < dailyDataTemp.Count; i++)
                {
                    var current = dailyDataTemp[i];
                    var previous = i > 0 ? dailyDataTemp[i - 1] : null;
                    
                    var revenueGrowth = previous != null && previous.revenue > 0 
                        ? ((current.revenue - previous.revenue) / previous.revenue * 100) 
                        : 0;

                    dailyData.Add(new
                    {
                        date = current.date,
                        revenue = current.revenue,
                        orders = current.orders,
                        avgOrderValue = current.avgOrderValue,
                        growth = Math.Round(revenueGrowth, 1)
                    });
                }

                // 3. Hourly data (today's order distribution)
                var todayOrders = completedOrders.Where(o => o.OrderDate.Date == DateTime.Today).ToList();
                var hourlyData = new List<object>();
                for (int hour = 0; hour < 24; hour++)
                {
                    var hourOrders = todayOrders.Where(o => o.OrderDate.Hour == hour).ToList();
                    hourlyData.Add(new
                    {
                        hour = hour.ToString("D2") + ":00",
                        orders = hourOrders.Count,
                        revenue = hourOrders.Sum(o => o.TotalAmount)
                    });
                }

                // 4. Product ranking
                var productRanking = completedOrders
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.MenuItemName)
                    .Select(g => new
                    {
                        name = g.Key,
                        quantity = g.Sum(oi => oi.Quantity),
                        revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                    })
                    .OrderByDescending(p => p.quantity)
                    .Take(10)
                    .ToList();

                // 5. Vendor performance data
                var vendorPerformance = completedOrders
                    .GroupBy(o => o.VendorEmail)
                    .Select(g => new
                    {
                        vendorEmail = g.Key,
                        revenue = g.Sum(o => o.TotalAmount),
                        orders = g.Count(),
                        avgOrderValue = g.Average(o => o.TotalAmount)
                    })
                    .OrderByDescending(v => v.revenue)
                    .Take(10)
                    .ToList();

                var result = new
                {
                    overview,
                    dailyData,
                    hourlyData,
                    productRanking,
                    vendorPerformance,
                    chartData = new
                    {
                        daily = new
                        {
                            labels = dailyDataTemp.Select(d => d.date.Substring(5)).ToList(), // MM-dd format
                            revenue = dailyDataTemp.Select(d => d.revenue).ToList(),
                            orders = dailyDataTemp.Select(d => d.orders).ToList()
                        },
                        hourly = new
                        {
                            labels = hourlyData.Select(h => ((dynamic)h).hour).ToList(),
                            orders = hourlyData.Select(h => ((dynamic)h).orders).ToList(),
                            revenue = hourlyData.Select(h => ((dynamic)h).revenue).ToList()
                        },
                        products = new
                        {
                            labels = productRanking.Select(p => p.name).ToList(),
                            data = productRanking.Select(p => p.quantity).ToList()
                        },
                        vendors = new
                        {
                            labels = vendorPerformance.Select(v => v.vendorEmail).ToList(),
                            data = vendorPerformance.Select(v => v.revenue).ToList()
                        }
                    }
                };

                // Cache result
                _cache.Set(cacheKey, result, _cacheExpiration);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load dashboard data", details = ex.Message });
            }
        }
        
    }
}