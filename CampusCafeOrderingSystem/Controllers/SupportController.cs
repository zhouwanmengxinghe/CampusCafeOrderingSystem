using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Controllers
{
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SupportController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Support main page
        public IActionResult Index()
        {
            var model = new SupportIndexViewModel
            {
                QuickActions = new List<QuickAction>
                {
                    new QuickAction { Title = "Submit Feedback", Description = "Share your experience with us", ActionUrl = "/Support/Feedback", Icon = "fas fa-comment" },
                    new QuickAction { Title = "View FAQ", Description = "Find answers to common questions", ActionUrl = "/Support/FAQ", Icon = "fas fa-question-circle" },
                    new QuickAction { Title = "Contact Us", Description = "Get in touch with our support team", ActionUrl = "/Support/Contact", Icon = "fas fa-envelope" },
                    new QuickAction { Title = "Live Chat", Description = "Chat with our support team", ActionUrl = "/Support/LiveChat", Icon = "fas fa-comments" }
                },
                RecentAnnouncements = new List<string>
                {
                    "New menu items available this week!",
                    "Extended hours during exam period",
                    "Mobile app now available for download"
                },
                ContactInfo = new ContactInformation
                {
                    Phone = "+64 6 350 5701",
                    Email = "support@campuscafe.ac.nz",
                    Address = "Massey University, Palmerston North Campus",
                    Hours = "Monday - Friday: 7:00 AM - 8:00 PM\nSaturday - Sunday: 9:00 AM - 6:00 PM",
                    SocialMedia = new Dictionary<string, string>
                    {
                        ["Facebook"] = "https://facebook.com/masseycafe",
                        ["Instagram"] = "https://instagram.com/masseycafe",
                        ["X/Twitter"] = "https://twitter.com/masseycafe"
                    }
                },
                FAQItems = new List<FAQItem>
                {
                    new FAQItem { Category = "Ordering",   Question = "How do I place an order?",        Answer = "Browse menu, add to cart, then checkout." },
                    new FAQItem { Category = "Payment",    Question = "What payment methods are accepted?", Answer = "Campus cards, credit cards, mobile payments." },
                    new FAQItem { Category = "Delivery",   Question = "How long does delivery take?",   Answer = "Typically 15–30 minutes depending on location and size." }
                }
            };

            return View(model);
        }

        // Feedback submission page
        [Authorize]
        public IActionResult Feedback()
        {
            return View(new FeedbackSubmissionViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(FeedbackSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            Enum.TryParse<FeedbackCategory>(model.Category, ignoreCase: true, out var categoryEnum);
            Enum.TryParse<FeedbackPriority>(model.Priority, ignoreCase: true, out var priorityEnum);

            if (!Enum.IsDefined(typeof(FeedbackCategory), categoryEnum))
                categoryEnum = FeedbackCategory.General;
            if (!Enum.IsDefined(typeof(FeedbackPriority), priorityEnum))
                priorityEnum = FeedbackPriority.Medium;

            var feedback = new Feedback
            {
                UserId = user.Id,
                UserEmail = user.Email ?? string.Empty,
                Subject = model.Subject,
                Message = model.Message,
                Category = categoryEnum,
                Priority = priorityEnum,
                Status = FeedbackStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your feedback! We'll review it shortly.";
            return RedirectToAction(nameof(FeedbackConfirmation), new { id = feedback.Id });
        }

        [Authorize]
        public async Task<IActionResult> FeedbackConfirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == user.Id);

            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        [Authorize]
        public async Task<IActionResult> MyFeedback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var feedbacks = await _context.Feedbacks
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(feedbacks);
        }

        public IActionResult LiveChat()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            var model = new List<FAQItem>
            {
                new FAQItem { Category = "Ordering", Question = "How do I place an order?", Answer = "You can place an order by browsing our menu, adding items to your cart, and proceeding to checkout." },
                new FAQItem { Category = "Payment", Question = "What payment methods do you accept?", Answer = "We accept campus cards, credit cards, and mobile payments." },
                new FAQItem { Category = "Delivery", Question = "How long does delivery take?", Answer = "Delivery typically takes 15-30 minutes depending on your location and order size." },
                new FAQItem { Category = "Orders", Question = "Can I cancel my order?", Answer = "Orders can be cancelled within 5 minutes of placement. After that, please contact support." },
                new FAQItem { Category = "Dietary", Question = "Do you offer vegetarian/vegan options?", Answer = "Yes! We have a variety of vegetarian and vegan options clearly marked on our menu." },
                new FAQItem { Category = "Tracking", Question = "How do I track my order?", Answer = "You can track your order status in the 'My Orders' section of your account." }
            };

            return View(model);
        }

        public IActionResult Contact()
        {
            var model = new ContactUsViewModel
            {
                ContactInfo = new ContactInformation
                {
                    Phone = "+64 6 350 5701",
                    Email = "support@campuscafe.ac.nz",
                    Address = "Massey University, Palmerston North Campus",
                    Hours = "Monday - Friday: 7:00 AM - 8:00 PM\nSaturday - Sunday: 9:00 AM - 6:00 PM",
                    SocialMedia = new Dictionary<string, string>
                    {
                        ["Facebook"] = "https://facebook.com/masseycafe",
                        ["Instagram"] = "https://instagram.com/masseycafe",
                        ["X/Twitter"] = "https://twitter.com/masseycafe"
                    }
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactUsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ContactInfo = new ContactInformation
                {
                    Phone = "+64 6 350 5701",
                    Email = "support@campuscafe.ac.nz",
                    Address = "Massey University, Palmerston North Campus",
                    Hours = "Monday - Friday: 7:00 AM - 8:00 PM\nSaturday - Sunday: 9:00 AM - 6:00 PM",
                    SocialMedia = new Dictionary<string, string>
                    {
                        ["Facebook"] = "https://facebook.com/masseycafe",
                        ["Instagram"] = "https://instagram.com/masseycafe",
                        ["X/Twitter"] = "https://twitter.com/masseycafe"
                    }
                };
                return View(model);
            }

            var categoryStr = "Contact";
            Enum.TryParse<FeedbackCategory>(categoryStr, true, out var categoryEnum);
            if (!Enum.IsDefined(typeof(FeedbackCategory), categoryEnum))
                categoryEnum = FeedbackCategory.General;

            var priorityStr = "Medium";
            Enum.TryParse<FeedbackPriority>(priorityStr, true, out var priorityEnum);
            if (!Enum.IsDefined(typeof(FeedbackPriority), priorityEnum))
                priorityEnum = FeedbackPriority.Medium;

            var feedback = new Feedback
            {
                UserId = User.Identity?.IsAuthenticated == true ? (await _userManager.GetUserAsync(User))?.Id : null,
                UserEmail = model.Email,
                Subject = model.Subject,
                Message = model.Message,
                Category = categoryEnum,
                Priority = priorityEnum,
                Status = FeedbackStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for contacting us! We'll get back to you soon.";
            return RedirectToAction(nameof(Contact));
        }
    }

    // View Models
    public class SupportIndexViewModel
    {
        public List<QuickAction> QuickActions { get; set; } = new List<QuickAction>();
        public List<string> RecentAnnouncements { get; set; } = new List<string>();
        public ContactInformation? ContactInfo { get; set; }
        public List<FAQItem> FAQItems { get; set; } = new List<FAQItem>();
    }

    public class QuickAction
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class FeedbackSubmissionViewModel
    {
        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        public string Priority { get; set; } = "Medium";

        public List<string> Categories { get; } = new List<string>
        {
            "General", "General Inquiry", "Order Issue", "Payment Problem", "Delivery Issue",
            "Food Quality", "Technical Support", "Suggestion", "Complaint", "Contact"
        };

        public List<string> Priorities { get; } = new List<string>
        {
            "Low", "Medium", "High", "Urgent"
        };
    }

    public class ContactUsViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = string.Empty;

        public ContactInformation? ContactInfo { get; set; }
    }

    public class FAQItem
    {
        public string Category { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }

    public class ContactInformation
    {
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Hours { get; set; } = string.Empty;

        // 改成字典
        public Dictionary<string, string> SocialMedia { get; set; } = new Dictionary<string, string>();
    }
}
