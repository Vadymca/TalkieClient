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

            // Subscribe to receive regular messages
            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                OnMessageReceived?.Invoke(user, message);
            });

            // Subscribe to receive private messages
            _connection.On<string, string>("ReceivePrivateMessage", (fromUser, message) =>
            {
                OnPrivateMessageReceived?.Invoke(fromUser, message);
            });

            // Subscribe to receive notifications
            _connection.On<string>("ReceiveNotification", (notification) =>
            {
                OnNotificationReceived?.Invoke(notification);
            });

            // Subscribe to change user status to online
            _connection.On<string>("UserOnline", (user) =>
            {
                OnUserOnline?.Invoke(user);
            });

            // Subscribe to change user status to offline
            _connection.On<string>("UserOffline", (user) =>
            {
                OnUserOffline?.Invoke(user);
            });

            // Subscribe to receive a file
            _connection.On<string, string, byte[]>("ReceiveFile", (user, fileName, fileData) =>
            {
                OnFileReceived?.Invoke(user, fileName, fileData);
            });
        }

        public async Task StartAsync()
        {
            try
            {
                await _connection.StartAsync();
                Console.WriteLine("Подключение к SignalR установлено успешно.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при старте SignalR: {ex.Message}");
            }
        }

        // Method to close the connection if necessary
        public async Task StopAsync()
        {
            try
            {
                await _connection.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при остановке SignalR: {ex.Message}");
            }
        }

        public async Task NotifyUserStatus(string username, bool isOnline)
        {
            await _connection.InvokeAsync("UpdateUserStatus", username, isOnline);
        }

        // Sending a normal message
        public async Task SendMessageAsync(string user, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Сообщение не может быть пустым.");

            await _connection.InvokeAsync("SendMessage", user, message);
        }

        // Sending a private message
        public async Task SendPrivateMessageAsync(string fromUser, string toUser, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Сообщение не может быть пустым.");

            await _connection.InvokeAsync("SendPrivateMessage", fromUser, toUser, message);
        }

        // Connecting to a group
        public async Task JoinGroupAsync(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentNullException(nameof(groupName), "Название группы не может быть пустым.");

            await _connection.InvokeAsync("JoinGroup", groupName);
        }

        // Group disconnection
        public async Task LeaveGroupAsync(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentNullException(nameof(groupName), "Название группы не может быть пустым.");

            await _connection.InvokeAsync("LeaveGroup", groupName);
        }

        // Sending a message to a group
        public async Task SendGroupMessageAsync(string groupName, string user, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Сообщение не может быть пустым.");

            try
            {
                await _connection.InvokeAsync("SendGroupMessage", groupName, user, message);
                Console.WriteLine($"Message sent to group {groupName} by {user}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to group {groupName}: {ex.Message}");
            }
        }

        // Sending a file
        public async Task SendFileAsync(string user, string fileName, byte[] fileData)
        {
            if (fileData == null || fileData.Length == 0)
                throw new ArgumentNullException(nameof(fileData), "Файл не может быть пустым.");

            await _connection.InvokeAsync("SendFile", user, fileName, fileData);
        }

        // Events for user status change notifications
        public event Action<string> OnUserOnline;
        public event Action<string> OnUserOffline;

        // Events for processing the receipt of messages
        public event Action<string, string> OnMessageReceived;
        public event Action<string, string> OnPrivateMessageReceived;
        public event Action<string> OnNotificationReceived;

        // Event for processing file receipt
        public event Action<string, string, byte[]> OnFileReceived;
    }
}