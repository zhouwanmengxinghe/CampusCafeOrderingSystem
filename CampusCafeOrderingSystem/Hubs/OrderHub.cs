using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using CampusCafeOrderingSystem.Models;

namespace CampusCafeOrderingSystem.Hubs
{
    [Authorize]
    public class OrderHub : Hub
    {
        public async Task JoinMerchantGroup(string merchantId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Merchant_{merchantId}");
        }

        public async Task LeaveMerchantGroup(string merchantId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Merchant_{merchantId}");
        }

        public async Task JoinCustomerGroup(string customerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
        }

        public async Task LeaveCustomerGroup(string customerId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
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

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}