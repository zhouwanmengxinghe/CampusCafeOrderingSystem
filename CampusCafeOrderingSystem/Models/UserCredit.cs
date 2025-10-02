using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusCafeOrderingSystem.Models
{
    // 用户积分表
    public class UserCredit
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal CurrentCredits { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalEarned { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalSpent { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    
    // 积分历史记录表
    public class CreditHistory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // "Earned" or "Spent"
        
        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal BalanceAfter { get; set; }
        
        public int? OrderId { get; set; } // 关联的订单ID（可选）
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}