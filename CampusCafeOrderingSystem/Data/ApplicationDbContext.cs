using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CampusCafeOrderingSystem.Models.Entities; // 引用实体类命名空间
using CampusCafeOrderingSystem.Models; // 如果 Order 保留在 Models 根目录

namespace CampusCafeOrderingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 订单数据接口
        public DbSet<OrderEntity> Orders { get; set; }

        // 系统设置数据接口，使用重命名后的实体类
        public DbSet<SystemSettingsEntity> SystemSettingsEntity { get; set; }

        // 菜单项数据接口
        public DbSet<MenuItemEntity> MenuItems { get; set; }

        // 团体订餐数据接口
        public DbSet<GroupOrderEntity> GroupOrders { get; set; }

        // 供应商数据接口
        public DbSet<VendorEntity> Vendors { get; set; }

        // 供应商资质文件数据接口
        public DbSet<VendorQualificationFileEntity> VendorQualificationFiles { get; set; }

    }
}
