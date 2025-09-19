using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    public enum ServiceType
    {
        Delivery = 0,
        Pickup = 1,
        [Display(Name = "On-site Service")]
        OnSiteService = 2
    }

    public enum MenuStyle
    {
        [Display(Name = "Coffee & Pastries")]
        CoffeePastries = 0,
        [Display(Name = "Sandwich & Salad")]
        SandwichSalad = 1,
        [Display(Name = "Hot Dishes")]
        HotDishes = 2,
        [Display(Name = "Mixed / Custom")]
        MixedCustom = 3
    }

    public enum ApplicationStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Paid = 3
    }
}
