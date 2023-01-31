using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace DiplomaThesis.Server.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(UserMessageContract userMessage, string groupName)
        {
            await Clients.Group(groupName).SendAsync("RecieveMessage", userMessage);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
