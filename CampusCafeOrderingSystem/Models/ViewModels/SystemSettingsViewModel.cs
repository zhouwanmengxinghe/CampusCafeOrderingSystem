using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.ViewModels
{
    public class SystemSettingsViewModel
    {
        [Required(ErrorMessage = "请设置营业时间")]
        [Display(Name = "营业时间")]
        public string BusinessHours { get; set; }

        [Display(Name = "启用供应商注册")]
        public bool EnableVendorRegistration { get; set; }

        [Required(ErrorMessage = "请设置最大团体订餐人数")]
        [Range(1, 1000, ErrorMessage = "团体订餐人数必须在1-1000之间")]
        [Display(Name = "最大团体订餐人数")]
        public int MaxGroupOrderSize { get; set; }
    }
}
