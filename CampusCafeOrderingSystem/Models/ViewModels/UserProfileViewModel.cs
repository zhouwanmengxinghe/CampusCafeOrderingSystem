using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "用户名")]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [Display(Name = "邮箱地址")]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [Display(Name = "手机号码")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Display(Name = "全名")]
        public string FullName { get; set; } = string.Empty;
        
        [Display(Name = "学号")]
        public string StudentId { get; set; } = string.Empty;
        
        [Display(Name = "院系")]
        public string Department { get; set; } = string.Empty;
        
        [Display(Name = "地址")]
        public string Address { get; set; } = string.Empty;
        
        public UserPreference? UserPreference { get; set; }
    }

    // 已从此文件删除重复定义
}
