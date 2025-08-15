using System.ComponentModel.DataAnnotations;

namespace CafeApp.Models
{
    public class CateringApplication
    {
        [Required(ErrorMessage = "活动名称不能为空")]
        [Display(Name = "活动名称")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "联系人姓名不能为空")]
        [Display(Name = "联系人姓名")]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "联系电话不能为空")]
        [Phone(ErrorMessage = "请输入有效的电话号码")]
        [Display(Name = "联系电话")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "活动日期不能为空")]
        [Display(Name = "活动日期")]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "人数不能为空")]
        [Range(10, 500, ErrorMessage = "团餐人数应在10-500人之间")]
        [Display(Name = "预计人数")]
        public int PeopleCount { get; set; }

        [Display(Name = "活动地址")]
        public string EventAddress { get; set; } = string.Empty;

        [Display(Name = "特殊要求")]
        [StringLength(500, ErrorMessage = "特殊要求不能超过500字符")]
        public string SpecialRequirements { get; set; } = string.Empty;

        [Display(Name = "预算范围")]
        public string BudgetRange { get; set; } = string.Empty;
    }

    public class CateringDetails
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int PeopleCount { get; set; }
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string EventAddress { get; set; } = string.Empty;
        public string SpecialRequirements { get; set; } = string.Empty;
    }
}