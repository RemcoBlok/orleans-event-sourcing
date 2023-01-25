using Microsoft.AspNetCore.SignalR;

namespace Banking.GrainInterfaces.Hubs
{
    public class NotificationHub : Hub<INotificationClient>
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Constants.NotificationGroup);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Constants.NotificationGroup);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
