using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.ViewModels
{
    public class VendorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "请输入供应商名称")]
        [Display(Name = "供应商名称")]
        public string Name { get; set; }

        [Required(ErrorMessage = "请输入联系邮箱")]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        [Display(Name = "联系邮箱")]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "请输入联系电话")]
        [Phone(ErrorMessage = "请输入有效的电话号码")]
        [Display(Name = "联系电话")]
        public string PhoneNumber { get; set; }

        [Display(Name = "状态")]
        public string Status { get; set; }

        public List<VendorQualificationFileViewModel> QualificationFiles { get; set; }
    }

    public class VendorQualificationFileViewModel
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}