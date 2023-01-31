using Microsoft.AspNetCore.SignalR;

namespace DiplomaThesis.Server.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(Guid userId, string message)
        {
            await Clients.All.SendAsync("RecieveMessage", userId, message);
        }
    }
}
