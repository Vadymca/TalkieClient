using Microsoft.AspNetCore.SignalR.Client;

namespace TalkieClient.SignalR
{
    public class SignalRClient
    {
        private readonly HubConnection _connection;
        private readonly string _username;

        public SignalRClient(string username)
        {
            _username = username;
            _connection = new HubConnectionBuilder()
                .WithUrl($"http://localhost:5133/chathub?username={username}")
                .Build();

            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                OnMessageReceived?.Invoke(user, message);
            });

            _connection.On<string, string>("ReceivePrivateMessage", (fromUser, message) =>
            {
                OnPrivateMessageReceived?.Invoke(fromUser, message);
            });

            _connection.On<string>("ReceiveNotification", (notification) =>
            {
                OnNotificationReceived?.Invoke(notification);
            });
        }

        public async Task StartAsync()
        {
            await _connection.StartAsync();
        }

        public async Task SendMessageAsync(string user, string message)
        {
            await _connection.InvokeAsync("SendMessage", user, message);
        }

        public async Task SendPrivateMessageAsync(string fromUser, string toUser, string message)
        {
            await _connection.InvokeAsync("SendPrivateMessage", fromUser, toUser, message);
        }

        public async Task JoinGroupAsync(string groupName)
        {
            await _connection.InvokeAsync("JoinGroup", groupName);
        }

        public async Task LeaveGroupAsync(string groupName)
        {
            await _connection.InvokeAsync("LeaveGroup", groupName);
        }

        public async Task SendGroupMessageAsync(string groupName, string user, string message)
        {
            await _connection.InvokeAsync("SendGroupMessage", groupName, user, message);
        }

        public event Action<string, string> OnMessageReceived;
        public event Action<string, string> OnPrivateMessageReceived;
        public event Action<string> OnNotificationReceived;
    }
}
