using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusCafeOrderingSystem.Services
{
    public interface ICreditService
    {
        Task<bool> AddCreditsAsync(string userId, decimal amount, string description, int? orderId = null);
        Task<bool> SpendCreditsAsync(string userId, decimal amount, string description, int? orderId = null);
        Task<UserCredit?> GetUserCreditsAsync(string userId);
        Task<List<CreditHistory>> GetCreditHistoryAsync(string userId, int take = 20);
    }
    
    public class CreditService : ICreditService
    {
        private readonly ApplicationDbContext _context;
        
        public CreditService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<bool> AddCreditsAsync(string userId, decimal amount, string description, int? orderId = null)
        {
            if (amount <= 0) return false;
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 获取或创建用户积分记录
                var userCredit = await GetOrCreateUserCreditAsync(userId);
                
                // 更新积分
                userCredit.CurrentCredits += amount;
                userCredit.TotalEarned += amount;
                userCredit.UpdatedAt = DateTime.UtcNow;
                
                // 创建积分历史记录
                var creditHistory = new CreditHistory
                {
                    UserId = userId,
                    Amount = amount,
                    Type = "Earned",
                    Description = description,
                    BalanceAfter = userCredit.CurrentCredits,
                    OrderId = orderId,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.CreditHistories.Add(creditHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        
        public async Task<bool> SpendCreditsAsync(string userId, decimal amount, string description, int? orderId = null)
        {
            if (amount <= 0) return false;
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 获取用户积分记录
                var userCredit = await GetOrCreateUserCreditAsync(userId);
                
                // 检查积分是否足够
                if (userCredit.CurrentCredits < amount)
                {
                    return false;
                }
                
                // 扣除积分
                userCredit.CurrentCredits -= amount;
                userCredit.TotalSpent += amount;
                userCredit.UpdatedAt = DateTime.UtcNow;
                
                // 创建积分历史记录
                var creditHistory = new CreditHistory
                {
                    UserId = userId,
                    Amount = amount,
                    Type = "Spent",
                    Description = description,
                    BalanceAfter = userCredit.CurrentCredits,
                    OrderId = orderId,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.CreditHistories.Add(creditHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        
        public async Task<UserCredit?> GetUserCreditsAsync(string userId)
        {
            return await _context.UserCredits
                .FirstOrDefaultAsync(uc => uc.UserId == userId);
        }
        
        public async Task<List<CreditHistory>> GetCreditHistoryAsync(string userId, int take = 20)
        {
            return await _context.CreditHistories
                .Where(ch => ch.UserId == userId)
                .OrderByDescending(ch => ch.CreatedAt)
                .Take(take)
                .ToListAsync();
        }
        
        private async Task<UserCredit> GetOrCreateUserCreditAsync(string userId)
        {
            var userCredit = await _context.UserCredits
                .FirstOrDefaultAsync(uc => uc.UserId == userId);
                
            if (userCredit == null)
            {
                userCredit = new UserCredit
                {
                    UserId = userId,
                    CurrentCredits = 0,
                    TotalEarned = 0,
                    TotalSpent = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserCredits.Add(userCredit);
            }
            
            return userCredit;
        }
    }
}