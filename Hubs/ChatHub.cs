using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace chatserver.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, User> _names = new ConcurrentDictionary<string, User>();

        public async Task Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            await Clients.All.SendAsync("broadcastMessage", Util.Sanitize(name), Util.MdToHtml(message));
        }

        public async Task SendFile(string name, string fileName, string fileData)
        {
            if (string.IsNullOrEmpty(fileData) || !fileData.StartsWith("data:"))
                return;
            await Clients.All.SendAsync("broadcastFile", Util.Sanitize(name), Util.Sanitize(fileName), fileData);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            User u;
                // Client closed their browser or otherwise intentionally disconnected
                _names.TryRemove(Context.ConnectionId, out u);
                await SendNames();
                await base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterName(string name)
        {
            // Add a name to the list.
            string newName = Util.Sanitize(name);
            var userExists = from u in _names.Values where u.ConnectionId == Context.ConnectionId select u;
            var nameExists = from u in _names.Values where u.Name == newName select u;
            if (userExists.Count() == 0 && newName != string.Empty)
            {
                User u = new User();
                u.ConnectionId = Context.ConnectionId;
                if (nameExists.Count() > 0)
                {
                    // if the name exists, append an identifier to this new one
                    u.Name = newName + "-" + Context.ConnectionId.Substring(0, 3);
                }
                else
                {
                    u.Name = newName;
                }
                u.ConnectionTime = DateTime.Now;
                u.Status = "Connected";
                u.StatusTime = DateTime.Now;
                _names.TryAdd(Context.ConnectionId, u);

            }
            await SendNames();
        }

        public async Task SendNames()
        {
            // Send the name list to all clients.
            var names = _names.Values
                .Select(u => new { u.Name, u.Status })
                .OrderBy(u => u.Name)
                .ToArray();
            var namesToSend = JsonConvert.SerializeObject(names);
            await Clients.All.SendAsync("nameList", namesToSend);

        }

    }
}