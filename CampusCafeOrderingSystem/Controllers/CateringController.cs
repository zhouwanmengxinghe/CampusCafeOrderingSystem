using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CafeApp.Models;
using System.Collections.Generic;

namespace CampusCafeOrderingSystem.Controllers
{
    public class CateringController : Controller
    {
        // GET: /Catering/Apply
        public IActionResult Apply()
        {
            return View();
        }

        // POST: /Catering/Apply
        [HttpPost]
        public IActionResult Apply(CateringApplication application)
        {
            if (ModelState.IsValid)
            {
                // 这里可以保存申请信息到数据库
                // 暂时重定向到详情页
                return RedirectToAction("Details", new { id = 1 });
            }
            return View(application);
        }

        // GET: /Catering/Details
        public IActionResult Details(int id)
        {
            // 模拟团餐详情数据
            var cateringDetails = new CateringDetails
            {
                Id = id,
                EventName = "公司年会团餐",
                EventDate = DateTime.Now.AddDays(7),
                PeopleCount = 50,
                MenuItems = GetCateringMenuItems(),
                TotalAmount = 2500.00m,
                Status = "待确认"
            };
            
            return View(cateringDetails);
        }

        // POST: /Catering/Checkout
        [HttpPost]
        [Authorize]
        public IActionResult Checkout(int id)
        {
            // 处理团餐结算逻辑
            // 这里可以集成支付系统
            TempData["SuccessMessage"] = "团餐订单已提交，我们将尽快与您联系确认详情！";
            return RedirectToAction("Apply");
        }

        private List<MenuItem> GetCateringMenuItems()
        {
            return new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    Name = "精品咖啡套餐",
                    Description = "包含拿铁、卡布奇诺、美式咖啡各种口味",
                    Price = 25.0M,
                    ImageUrl = "/images/coffee-set.jpg"
                },
                new MenuItem
                {
                    Id = 2,
                    Name = "下午茶点心套餐",
                    Description = "精美糕点、三明治、水果拼盘",
                    Price = 35.0M,
                    ImageUrl = "/images/afternoon-tea.jpg"
                },
                new MenuItem
                {
                    Id = 3,
                    Name = "商务简餐套餐",
                    Description = "营养均衡的商务套餐，适合会议用餐",
                    Price = 45.0M,
                    ImageUrl = "/images/business-meal.jpg"
                }
            };
        }
    }
}