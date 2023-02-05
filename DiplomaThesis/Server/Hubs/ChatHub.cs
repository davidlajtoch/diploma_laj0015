using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace DiplomaThesis.Server.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(UserGroupMessageContract userMessage, string groupName)
        {
            await Clients.Group(groupName).SendAsync("RecieveMessage", userMessage);
        }

        public async Task SendConnectedUserId(Guid userId, string groupName)
        {
            await Clients.OthersInGroup(groupName).SendAsync("RecieveConnectedUserId", userId);
        }



        public async Task RequestConnectedUserId(string groupName)
        {
            await Clients.OthersInGroup(groupName).SendAsync("SendConnectedUserId");
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
