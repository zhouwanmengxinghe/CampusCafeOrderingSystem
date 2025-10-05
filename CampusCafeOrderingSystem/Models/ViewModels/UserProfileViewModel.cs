using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = string.Empty;
        
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;
        
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
        
        public UserPreference? UserPreference { get; set; }
    }

    // Removed duplicate definition from this file
}
