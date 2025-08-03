using System;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.ViewModels
{
    public class GroupOrderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "请输入团体名称")]
        [Display(Name = "团体名称")]
        public string GroupName { get; set; }

        [Required(ErrorMessage = "请输入申请人邮箱")]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        [Display(Name = "申请人邮箱")]
        public string RequesterEmail { get; set; }

        [Display(Name = "申请时间")]
        public DateTime RequestDate { get; set; }

        [Display(Name = "状态")]
        public string Status { get; set; }
    }
}