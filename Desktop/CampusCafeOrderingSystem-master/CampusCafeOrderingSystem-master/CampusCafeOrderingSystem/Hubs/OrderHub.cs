using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using CampusCafeOrderingSystem.Models;

namespace CampusCafeOrderingSystem.Hubs
{
    [Authorize]
    public class OrderHub : Hub
    {
        // Merchant join group
        public async Task JoinMerchantGroup(string merchantEmail)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Merchant_{merchantEmail}");
        }

        // Merchant leave group
        public async Task LeaveMerchantGroup(string merchantEmail)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Merchant_{merchantEmail}");
        }

        // User join group
        public async Task JoinCustomerGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{userId}");
        }

        // User leave group
        public async Task LeaveCustomerGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{userId}");
        }

        // Admin join group
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        // Admin leave group
        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }

        // Automatically join appropriate group on connection
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    // Automatically join appropriate group based on user role
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

        // Cleanup on disconnect
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // SignalR automatically handles group cleanup
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