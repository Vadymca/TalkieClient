using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

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

            // Подписка на получение обычных сообщений
            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                OnMessageReceived?.Invoke(user, message);
            });

            // Подписка на получение приватных сообщений
            _connection.On<string, string>("ReceivePrivateMessage", (fromUser, message) =>
            {
                OnPrivateMessageReceived?.Invoke(fromUser, message);
            });

            // Подписка на получение уведомлений
            _connection.On<string>("ReceiveNotification", (notification) =>
            {
                OnNotificationReceived?.Invoke(notification);
            });

            // Подписка на изменение статуса пользователя на онлайн
            _connection.On<string>("UserOnline", (user) =>
            {
                OnUserOnline?.Invoke(user);
            });

            // Подписка на изменение статуса пользователя на оффлайн
            _connection.On<string>("UserOffline", (user) =>
            {
                OnUserOffline?.Invoke(user);
            });

            // Подписка на получение файла
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

        // Метод для закрытия соединения при необходимости
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

        // Отправка обычного сообщения
        public async Task SendMessageAsync(string user, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Сообщение не может быть пустым.");

            await _connection.InvokeAsync("SendMessage", user, message);
        }

        // Отправка приватного сообщения
        public async Task SendPrivateMessageAsync(string fromUser, string toUser, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Сообщение не может быть пустым.");

            await _connection.InvokeAsync("SendPrivateMessage", fromUser, toUser, message);
        }

        // Подключение к группе
        public async Task JoinGroupAsync(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentNullException(nameof(groupName), "Название группы не может быть пустым.");

            await _connection.InvokeAsync("JoinGroup", groupName);
        }

        // Отключение от группы
        public async Task LeaveGroupAsync(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentNullException(nameof(groupName), "Название группы не может быть пустым.");

            await _connection.InvokeAsync("LeaveGroup", groupName);
        }

        // Отправка сообщения в группу
        public async Task SendGroupMessageAsync(string groupName, string user, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Сообщение не может быть пустым.");

            await _connection.InvokeAsync("SendGroupMessage", groupName, user, message);
        }

        // Отправка файла
        public async Task SendFileAsync(string user, string fileName, byte[] fileData)
        {
            if (fileData == null || fileData.Length == 0)
                throw new ArgumentNullException(nameof(fileData), "Файл не может быть пустым.");

            await _connection.InvokeAsync("SendFile", user, fileName, fileData);
        }

        // События для уведомлений об изменении статуса пользователя
        public event Action<string> OnUserOnline;
        public event Action<string> OnUserOffline;

        // События для обработки получения сообщений
        public event Action<string, string> OnMessageReceived;
        public event Action<string, string> OnPrivateMessageReceived;
        public event Action<string> OnNotificationReceived;

        // Событие для обработки получения файла
        public event Action<string, string, byte[]> OnFileReceived;
    }
}