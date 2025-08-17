using CafeApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CafeApp.Controllers
{
    public class MenuController : Controller
    {
        // GET: /Menu
        public IActionResult Index()
        {
            var menuItems = GetSampleMenuItems();
            return View("~/Views/user_order_pay/Menu/userIndex.cshtml", menuItems);

        }

        private List<MenuItem> GetSampleMenuItems()
        {
            return new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    Name = "Latte",
                    Description = "A smooth blend of espresso and steamed milk.",
                    Price = 4.5M,
                    ImageUrl = "Content/images/latte.jpg" 
                },
                new MenuItem
                {
                    Id = 2,
                    Name = "Cappuccino",
                    Description = "Espresso with frothy milk.",
                    Price = 4.0M,
                    ImageUrl = "Content/images/cappuccino.jpg"
                },
                new MenuItem
                {
                    Id = 3,
                    Name = "Espresso",
                    Description = "Strong classic espresso shot.",
                    Price = 3.0M,
                    ImageUrl = "Content/images/espresso.jpg"  // Fixed image path
                }
            };
        }
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetString("Cart");
        
            return View();
        }
    }
}