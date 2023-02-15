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

        public async Task SendPointer(double pointer_x, double pointer_y, string username, string group)
        {
                var encodedUsername = HttpUtility.HtmlEncode(username);
                await Clients.OthersInGroup(group).SendAsync("LoadPointer", pointer_x, pointer_y, Context.ConnectionId, encodedUsername);
        }

        public async Task LoadCanvasRequest(string group)
        {
            await Clients.OthersInGroup(group).SendAsync("LoadCanvasRequest", Context.ConnectionId);
        }

        public async Task SendCanvas(string json, int id_counter, string caller)
        {
            await Clients.Client(caller).SendAsync("DrawCanvas", json, id_counter);
        }

        public async Task SendCanvasAll(string whiteboard_data, int id_counter, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawCanvas", whiteboard_data, id_counter);
        }

        public async Task SendBringObjectForward(int id, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawBringObjectForward", id);
        }

        public async Task SendSendObjectBackwards(int id, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawSendObjectBackwards", id);
        }

        public async Task SendObjectAdd(object json, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectAdd", json);
        }

        public async Task SendObjectMove(float x, float y, int id, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectMove", x, y, id);
        }

        public async Task SendObjectScale(float x, float y, float scaleX, float scaleY, int id, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectScale", x, y, scaleX, scaleY, id);
        }

        public async Task SendObjectRotate(float angle, int id, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectRotate", angle, id);
        }

        public async Task SendObjectRemove(int id, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectRemove", id);
        }

        public async Task SendObjectBucket(string fill, int id, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectBucket", fill, id);
        }

        public async Task SendObjectRecolor(string stroke, int id, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectRecolor", stroke, id);
        }

        public async Task SendObjectGroup(object objects, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawObjectGroup", objects);
        }

        public async Task SendImg(string img_data, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawImg", img_data);
        }

        public async Task SendTextModify(string text, int id, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawTextModify", text, id);
        }

        public async Task SendHistoryCounter(int history_counter, string group)
        {
            await Clients.OthersInGroup(group).SendAsync("DrawHistoryCounter", history_counter);
        }
    }
}
