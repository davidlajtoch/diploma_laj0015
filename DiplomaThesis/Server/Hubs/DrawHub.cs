using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.SignalR;
using System.Web;

namespace DiplomaThesis.Server.Hubs
{
    public class DrawHub : Hub
    {
        public async Task AddToGroup(string group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
        }

        public async Task RemoveFromGroup(string group)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }

        public async Task LoadMessagesRequest(string group)
        {
            await Clients.OthersInGroup(group).SendAsync("LoadMessagesRequest", Context.ConnectionId);
        }

        public async Task SendMessages(List<string> messages, string caller)
        {
            List<string> encodedMessages = new List<string>();
            foreach(string item in messages)
            {
                encodedMessages.Add(HttpUtility.HtmlAttributeEncode(item));
            }
            await Clients.Client(caller).SendAsync("LoadMessages", encodedMessages);
        }

        public async Task SendMessage(string username, string message, string group)
        {
            if (message != "")
            {
                var encodedUsername = HttpUtility.HtmlEncode(username);
                var encodedMessage = HttpUtility.HtmlAttributeEncode(message);
                await Clients.OthersInGroup(group).SendAsync("LoadMessage", Context.ConnectionId, encodedUsername, encodedMessage);
            }
        }
        public async Task SendAddPage(string group)
        {
            await Clients.OthersInGroup(group).SendAsync("AddPage");
        }

        public async Task SendPointer(double pointer_x, double pointer_y, string username, int page, string group)
        {
                var encodedUsername = HttpUtility.HtmlEncode(username);
                await Clients.OthersInGroup(group).SendAsync("LoadPointer", pointer_x, pointer_y, Context.ConnectionId, encodedUsername, page);
        }

        public async Task LoadCanvasRequest(string group)
        {
            await Clients.OthersInGroup(group).SendAsync("LoadCanvasRequest", Context.ConnectionId);
        }

        public async Task SendCanvas(string json, int id_counter, int page, string caller)
        {
            await Clients.Client(caller).SendAsync("DrawCanvas", json, id_counter, page);
        }

        public async Task SendCanvasAll(string whiteboard_data, int id_counter, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawCanvas", whiteboard_data, id_counter, page);
        }

        public async Task SendBringObjectForward(int id, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawBringObjectForward", id, page);
        }

        public async Task SendSendObjectBackwards(int id, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawSendObjectBackwards", id, page);
        }

        public async Task SendObjectAdd(object json, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectAdd", json, page);
        }

        public async Task SendObjectMove(float x, float y, int id, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectMove", x, y, id, page);
        }

        public async Task SendObjectScale(float x, float y, float scaleX, float scaleY, int id, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectScale", x, y, scaleX, scaleY, id, page);
        }

        public async Task SendObjectRotate(float angle, int id, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectRotate", angle, id, page);
        }

        public async Task SendObjectRemove(int id, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectRemove", id, page);
        }

        public async Task SendObjectBucket(string fill, int id, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectBucket", fill, id, page);
        }

        public async Task SendObjectRecolor(string stroke, int id, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectRecolor", stroke, id, page);
        }

        public async Task SendObjectGroup(object objects, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectGroup", objects, page);
        }

        public async Task SendImg(string img_data, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawImg", img_data, page);
        }

        public async Task SendTextModify(string text, int id, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawTextModify", text, id, page);
        }

        public async Task SendHistoryCounter(int history_counter, int page, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawHistoryCounter", history_counter, page);
        }
    }
}
