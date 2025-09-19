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
        private readonly bool _useDemoData = false; // 强制使用真实数据

        public ReportsApiController(ApplicationDbContext context, IOrderService orderService, IMemoryCache cache)
        {
            _context = context;
            _orderService = orderService;
            _cache = cache;
        }
        
        #region 生成演示数据的辅助方法
        
        private object GenerateDemoOverviewData(DateTime startDate, DateTime endDate)
        {
            // 生成总收入（5000-15000之间的随机值）
            decimal totalRevenue = _random.Next(5000, 15000) + (decimal)_random.NextDouble();
            totalRevenue = Math.Round(totalRevenue, 2);
            
            // 生成总订单数（100-300之间的随机值）
            int totalOrders = _random.Next(100, 300);
            
            // 计算平均订单价值
            decimal avgOrderValue = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0;
            
            // 生成最热销商品
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
            
            // 生成每日数据
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // 生成当日收入（200-1000之间的随机值）
                decimal revenue = _random.Next(200, 1000) + (decimal)_random.NextDouble();
                revenue = Math.Round(revenue, 2);
                
                // 生成当日订单数（10-50之间的随机值）
                int orders = _random.Next(10, 50);
                
                // 计算平均订单价值
                decimal avgOrderValue = orders > 0 ? Math.Round(revenue / orders, 2) : 0;
                
                // 生成增长率（-10%到20%之间的随机值）
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
            
            // 生成每小时数据
            for (int hour = 0; hour < 24; hour++)
            {
                // 早上和下午的高峰时段订单量更高
                int orderMultiplier = 1;
                if (hour >= 7 && hour <= 9) orderMultiplier = 3; // Morning peak
                if (hour >= 11 && hour <= 13) orderMultiplier = 4; // Lunch peak
                if (hour >= 17 && hour <= 19) orderMultiplier = 3; // Evening peak
                
                // 生成当小时订单数
                int orders = _random.Next(1, 10) * orderMultiplier;
                
                // 生成当小时收入
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
            
            // 为每个产品生成销量和收入数据
            foreach (var product in products)
            {
                // 生成销量（10-100之间的随机值）
                int quantity = _random.Next(10, 100);
                
                // 生成单价（20-50之间的随机值）
                decimal unitPrice = _random.Next(20, 50) + (decimal)_random.NextDouble();
                unitPrice = Math.Round(unitPrice, 2);
                
                // 计算总收入
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
            
            // 按销量排序
            productRanking = productRanking.OrderByDescending(p => ((dynamic)p).quantity).ToList();
            
            // 获取前5名产品的数据用于图表
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
            
            // 为每个类别生成销售数据
            foreach (var category in categories)
            {
                // 生成销售额（500-2000之间的随机值）
                decimal sales = _random.Next(500, 2000) + (decimal)_random.NextDouble();
                sales = Math.Round(sales, 2);
                
                totalSales += sales;
                
                categoryData.Add(new
                {
                    category,
                    sales,
                    percentage = 0m // 先设为0，后面再计算
                });
                
                labels.Add(category);
                salesData.Add(sales);
            }
            
            // 计算每个类别的百分比
            if (totalSales > 0)
            {
                for (int i = 0; i < categoryData.Count; i++)
                {
                    var item = (dynamic)categoryData[i];
                    decimal percentage = Math.Round(item.sales / totalSales * 100, 1);
                    
                    // 创建新对象替换原来的对象
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
            
            // 生成10个最近订单
            for (int i = 0; i < 10; i++)
            {
                // 生成订单号
                string orderNumber = $"ORD{DateTime.Now.ToString("yyyyMMdd")}{_random.Next(1000, 9999)}";
                
                // 随机选择客户名
                string customerName = customerNames[_random.Next(customerNames.Length)];
                
                // 生成订单金额（50-200之间的随机值）
                decimal totalAmount = _random.Next(50, 200) + (decimal)_random.NextDouble();
                totalAmount = Math.Round(totalAmount, 2);
                
                // 随机选择订单状态
                string status = statuses[_random.Next(statuses.Length)];
                
                // 生成订单创建时间（过去24小时内的随机时间）
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
            
            // 按创建时间排序，最新的排在前面
            return recentOrders.OrderByDescending(o => ((dynamic)o).createdAt).ToList();
        }
        
        #endregion

        [HttpPost("clear-cache")]
        // 注意：此端点故意不添加[Authorize]特性，允许内部服务调用
        // 用于系统内部服务间通信，清除缓存
        public IActionResult ClearCache([FromBody] JsonElement requestBody)
        {
            try
            {
                // 从请求体中提取vendorEmail
                string vendorEmail = null;
                
                // 尝试从JSON对象中获取vendorEmail属性
                if (requestBody.TryGetProperty("vendorEmail", out JsonElement vendorEmailElement) && 
                    vendorEmailElement.ValueKind == JsonValueKind.String)
                {
                    vendorEmail = vendorEmailElement.GetString();
                }
                // 如果没有vendorEmail属性，尝试将整个请求体作为字符串
                else if (requestBody.ValueKind == JsonValueKind.String)
                {
                    vendorEmail = requestBody.GetString();
                }
                
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return BadRequest(new { message = "Invalid vendor email" });
                }

                // 清除指定商家的所有报表缓存
                var today = DateTime.Today;
                var patterns = new[]
                {
                    $"dashboard_data_{vendorEmail}_",
                    $"overview_{vendorEmail}_",
                    $"daily_data_{vendorEmail}_",
                    $"category_data_{vendorEmail}_"
                };

                // 由于IMemoryCache没有直接的模式匹配删除方法，我们需要清除可能的日期范围
                for (int i = -7; i <= 7; i++)
                {
                    var date = today.AddDays(i);
                    var dateStr = date.ToString("yyyyMMdd");
                    
                    foreach (var pattern in patterns)
                    {
                        // 清除单日缓存
                        _cache.Remove($"{pattern}{dateStr}_{dateStr}");
                        
                        // 清除可能的日期范围缓存
                        for (int j = i; j <= 7; j++)
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
                // 如果启用了演示数据，则返回演示数据
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
                
                // 以下是原有的实际数据获取逻辑
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // 生成缓存键
                var cacheKey = $"dashboard_data_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // 尝试从缓存获取数据
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                // 获取当前商家的相关订单数据（包括所有状态）
                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .ToListAsync();

                // 只使用已完成的订单进行统计计算
                var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();

                // 1. 概览数据
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

                // 2. 每日数据
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

                // 计算增长率并创建最终数据
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

                // 3. 小时分析数据
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

                // 4. 商品排行数据
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

                // 5. 分类数据
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

                // 将结果存入缓存
                _cache.Set(cacheKey, result, _cacheExpiration);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取报表数据失败", error = ex.Message });
            }
        }

        [HttpGet("overview")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetReportOverview([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var demoOverview = GenerateDemoOverviewData(startDate, endDate);
                    return Ok(demoOverview);
                }
                
                // 以下是原有的实际数据获取逻辑
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // 生成缓存键
                var cacheKey = $"overview_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // 尝试从缓存获取数据
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

                // 获取最热销商品
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
                    topProduct = topProduct ?? "暂无数据"
                };

                // 将结果存入缓存
                _cache.Set(cacheKey, overview, _cacheExpiration);

                return Ok(overview);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取报表概览失败", error = ex.Message });
            }
        }

        [HttpGet("daily-data")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetDailyData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var demoDailyData = GenerateDemoDailyData(startDate, endDate);
                    return Ok(demoDailyData);
                }
                
                // 以下是原有的实际数据获取逻辑
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // 生成缓存键
                var cacheKey = $"daily_data_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // 尝试从缓存获取数据
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                // 优化：使用单个查询获取所有数据，避免循环查询
                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                    .Select(o => new { o.OrderDate, o.TotalAmount })
                    .ToListAsync();

                // 按日期分组计算每日数据
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

                    // 计算增长率（与前一天比较）
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

                // 将结果存入缓存
                _cache.Set(cacheKey, dailyData, _cacheExpiration);

                return Ok(dailyData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取日销售数据失败", error = ex.Message });
            }
        }

        [HttpGet("product-ranking")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetProductRanking([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var demoProductRanking = GenerateDemoProductRanking();
                    return Ok(demoProductRanking);
                }
                
                // 以下是原有的实际数据获取逻辑
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // 生成缓存键
                var cacheKey = $"product_ranking_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // 尝试从缓存获取数据
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

                // 将结果存入缓存
                _cache.Set(cacheKey, rankedProducts, _cacheExpiration);

                return Ok(rankedProducts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取商品排行失败", error = ex.Message });
            }
        }

        [HttpGet("hourly-analysis")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetHourlyAnalysis([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var hourlyData = GenerateDemoHourlyData();
                    return Ok(hourlyData);
                }
                
                // 以下是原有的实际数据获取逻辑
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // 生成缓存键
                var cacheKey = $"hourly_analysis_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // 尝试从缓存获取数据
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
                            _ => "其他时段"
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

                // 将结果存入缓存
                _cache.Set(cacheKey, hourlyAnalysis, _cacheExpiration);

                return Ok(hourlyAnalysis);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取时段分析失败", error = ex.Message });
            }
        }

        [HttpGet("category-data")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetCategoryData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // 如果启用了演示数据，则返回演示数据
                if (_useDemoData)
                {
                    var demoCategoryData = GenerateDemoCategoryData();
                    return Ok(demoCategoryData);
                }
                
                // 以下是原有的实际数据获取逻辑
                // Get current vendor email
                var vendorEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(vendorEmail))
                {
                    return Unauthorized();
                }

                // 生成缓存键
                var cacheKey = $"category_data_{vendorEmail}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                
                // 尝试从缓存获取数据
                if (_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    return Ok(cachedResult);
                }

                var orders = await _context.Orders
                    .Where(o => o.VendorEmail == vendorEmail && o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Completed)
                    .Include(o => o.OrderItems)
                    .ToListAsync();

                var totalRevenue = orders.Sum(o => o.TotalAmount);

                // 通过菜品名称推断分类（简化处理）
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

                // 将结果存入缓存
                _cache.Set(cacheKey, categoryRevenue, _cacheExpiration);

                return Ok(categoryRevenue);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "获取分类数据失败", error = ex.Message });
            }
        }

        private string GetCategoryByName(string menuItemName)
        {
            // 简化的分类逻辑，实际项目中应该从MenuItem表获取分类
            if (menuItemName.Contains("咖啡") || menuItemName.Contains("拿铁") || menuItemName.Contains("美式") || menuItemName.Contains("卡布奇诺"))
                return "咖啡";
            if (menuItemName.Contains("蛋糕") || menuItemName.Contains("甜品") || menuItemName.Contains("提拉米苏"))
                return "甜品";
            if (menuItemName.Contains("茶") || menuItemName.Contains("奶茶"))
                return "茶饮";
            return "小食";
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
                
                // 创建营业概览工作表
                var overviewSheet = package.Workbook.Worksheets.Add("营业概览");
                
                // 设置标题
                overviewSheet.Cells[1, 1].Value = "校园咖啡厅营业报表";
                overviewSheet.Cells[1, 1, 1, 4].Merge = true;
                overviewSheet.Cells[1, 1].Style.Font.Size = 16;
                overviewSheet.Cells[1, 1].Style.Font.Bold = true;
                overviewSheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                overviewSheet.Cells[2, 1].Value = $"报表期间：{startDate:yyyy-MM-dd} 至 {endDate:yyyy-MM-dd}";
                overviewSheet.Cells[2, 1, 2, 4].Merge = true;
                overviewSheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                // 营业概览数据
                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var totalOrders = orders.Count;
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
                
                overviewSheet.Cells[4, 1].Value = "指标";
                overviewSheet.Cells[4, 2].Value = "数值";
                overviewSheet.Cells[5, 1].Value = "总营业额";
                overviewSheet.Cells[5, 2].Value = totalRevenue;
                overviewSheet.Cells[6, 1].Value = "订单总数";
                overviewSheet.Cells[6, 2].Value = totalOrders;
                overviewSheet.Cells[7, 1].Value = "平均订单金额";
                overviewSheet.Cells[7, 2].Value = avgOrderValue;
                
                // 设置格式
                overviewSheet.Cells[4, 1, 4, 2].Style.Font.Bold = true;
                overviewSheet.Cells[5, 2].Style.Numberformat.Format = "¥#,##0.00";
                overviewSheet.Cells[7, 2].Style.Numberformat.Format = "¥#,##0.00";
                
                // 创建订单详情工作表
                var ordersSheet = package.Workbook.Worksheets.Add("订单详情");
                
                // 设置表头
                ordersSheet.Cells[1, 1].Value = "订单号";
                ordersSheet.Cells[1, 2].Value = "下单时间";
                ordersSheet.Cells[1, 3].Value = "客户电话";
                ordersSheet.Cells[1, 4].Value = "商品";
                ordersSheet.Cells[1, 5].Value = "数量";
                ordersSheet.Cells[1, 6].Value = "单价";
                ordersSheet.Cells[1, 7].Value = "小计";
                ordersSheet.Cells[1, 8].Value = "订单总额";
                
                ordersSheet.Cells[1, 1, 1, 8].Style.Font.Bold = true;
                ordersSheet.Cells[1, 1, 1, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ordersSheet.Cells[1, 1, 1, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                
                // 填充订单数据
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
                
                // 自动调整列宽
                overviewSheet.Cells.AutoFitColumns();
                ordersSheet.Cells.AutoFitColumns();
                
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                
                var fileName = $"营业报表_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "导出Excel失败", details = ex.Message });
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
                
                // 设置中文字体
                var baseFont = BaseFont.CreateFont("STSong-Light", "UniGB-UCS2-H", BaseFont.NOT_EMBEDDED);
                var titleFont = new Font(baseFont, 18, Font.BOLD);
                var headerFont = new Font(baseFont, 14, Font.BOLD);
                var normalFont = new Font(baseFont, 10, Font.NORMAL);
                
                // 添加标题
                var title = new Paragraph("校园咖啡厅营业报表", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(title);
                
                var dateRange = new Paragraph($"报表期间：{startDate:yyyy年MM月dd日} 至 {endDate:yyyy年MM月dd日}", normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(dateRange);
                
                // 营业概览
                var overviewTitle = new Paragraph("营业概览", headerFont)
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
                
                overviewTable.AddCell(new PdfPCell(new Phrase("总营业额", normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase($"¥{totalRevenue:F2}", normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase("订单总数", normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase(totalOrders.ToString(), normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase("平均订单金额", normalFont)));
                overviewTable.AddCell(new PdfPCell(new Phrase($"¥{avgOrderValue:F2}", normalFont)));
                
                document.Add(overviewTable);
                
                // 订单详情
                var ordersTitle = new Paragraph("订单详情", headerFont)
                {
                    SpacingAfter = 10
                };
                document.Add(ordersTitle);
                
                var ordersTable = new PdfPTable(6)
                {
                    WidthPercentage = 100
                };
                ordersTable.SetWidths(new float[] { 1.5f, 1.5f, 1f, 2f, 0.8f, 1f });
                
                // 表头
                ordersTable.AddCell(new PdfPCell(new Phrase("订单号", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("下单时间", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("客户电话", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("商品", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("数量", headerFont)));
                ordersTable.AddCell(new PdfPCell(new Phrase("金额", headerFont)));
                
                // 数据行
                foreach (var order in orders.OrderByDescending(o => o.OrderDate).Take(50)) // 限制显示前50个订单
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
                    var note = new Paragraph($"注：仅显示前50个订单，总共{orders.Count}个订单", normalFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 10
                    };
                    document.Add(note);
                }
                
                document.Close();
                
                var fileName = $"营业报表_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "导出PDF失败", details = ex.Message });
            }
        }
    }
}