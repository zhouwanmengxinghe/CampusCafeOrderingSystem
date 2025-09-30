using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using CampusCafeOrderingSystem.Models;

namespace CampusCafeOrderingSystem.Hubs
{
    [Authorize]
    public class OrderHub : Hub
    {
        // 商家加入组
        public async Task JoinMerchantGroup(string merchantEmail)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Merchant_{merchantEmail}");
        }

        // 商家离开组
        public async Task LeaveMerchantGroup(string merchantEmail)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Merchant_{merchantEmail}");
        }

        // 用户加入组
        public async Task JoinCustomerGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{userId}");
        }

        // 用户离开组
        public async Task LeaveCustomerGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{userId}");
        }

        // 管理员加入组
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        // 管理员离开组
        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }

        // 连接时自动加入相应组
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    // 根据用户角色自动加入相应组
                    if (user.IsInRole("Admin"))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                    }
                    else if (user.IsInRole("Vendor"))
                    {
                        var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                        if (!string.IsNullOrEmpty(email))
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, $"Merchant_{email}");
                        }
                    }
                    else if (user.IsInRole("Customer"))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{userId}");
                    }
                }
            }
            await base.OnConnectedAsync();
        }

        // 断开连接时清理
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // SignalR会自动处理组的清理
            await base.OnDisconnectedAsync(exception);
        }



        // Method to send new order notification to merchant
        public async Task SendNewOrderToMerchant(string merchantEmail, object orderData)
        {
            await Clients.Group($"Merchant_{merchantEmail}").SendAsync("NewOrder", orderData);
        }

        // Method to send order status update
        public async Task SendOrderStatusUpdate(string merchantEmail, string customerId, int orderId, string newStatus)
        {
            await Clients.Group($"Merchant_{merchantEmail}").SendAsync("OrderStatusUpdated", orderId, newStatus);
            await Clients.Group($"Customer_{customerId}").SendAsync("OrderStatusUpdated", orderId, newStatus);
        }

        // Method to send new review notification to merchant
        public async Task SendNewReviewToMerchant(string merchantEmail, object reviewData)
        {
            await Clients.Group($"Merchant_{merchantEmail}").SendAsync("NewReview", reviewData);
        }

        // Method to send review reply notification to customer
        public async Task SendReviewReplyToCustomer(string customerId, object replyData)
        {
            await Clients.Group($"Customer_{customerId}").SendAsync("ReviewReply", replyData);
        }

        // Method to update merchant dashboard stats
        public async Task UpdateMerchantStats(string merchantEmail, object statsData)
        {
            await Clients.Group($"Merchant_{merchantEmail}").SendAsync("StatsUpdated", statsData);
        }
    }
}