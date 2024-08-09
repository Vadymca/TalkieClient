using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TalkieClient.Data;
using TalkieClient.Models;
using TalkieClient.SignalR;
using TalkieClient.Views;

namespace TalkieClient.Views
{
    public partial class MainWindow : Window
    {
        private User _loggedInUser;
        private SignalRClient _signalRClient;
        private ObservableCollection<Message> _messages;

        public MainWindow()
        {
            InitializeComponent();
            this.Close();
        }

        public MainWindow(User loggedInUser)
        {
            InitializeComponent();
            _loggedInUser = loggedInUser;
            App.CurrentUserId = _loggedInUser.UserId;
            CurrentUserTextBlock.Text = _loggedInUser.Username;

            _signalRClient = new SignalRClient(_loggedInUser.Username);

            _signalRClient.OnMessageReceived += SignalRClient_OnMessageReceived;
            _signalRClient.OnPrivateMessageReceived += SignalRClient_OnPrivateMessageReceived;
            _signalRClient.OnNotificationReceived += SignalRClient_OnNotificationReceived;

            _messages = new ObservableCollection<Message>();
            MessageList.ItemsSource = _messages;

            LoadUsersAndGroups();
            StartSignalRClient();
        }

        private async void StartSignalRClient()
        {
            await _signalRClient.StartAsync();
        }

        private void SignalRClient_OnMessageReceived(string user, string message)
        {
            Dispatcher.Invoke(() =>
            {
                _messages.Add(new Message { Sender = new User { Username = user }, Content = message, Timestamp = DateTime.Now });
                ShowNotification($"New message from {user}", message);
            });
        }

        private void SignalRClient_OnPrivateMessageReceived(string fromUser, string message)
        {
            Dispatcher.Invoke(() =>
            {
                _messages.Add(new Message { Sender = new User { Username = fromUser }, Content = $"(Private) {message}", Timestamp = DateTime.Now });
                ShowNotification($"Private message from {fromUser}", message);
            });
        }

        private void SignalRClient_OnNotificationReceived(string notification)
        {
            Dispatcher.Invoke(() =>
            {
                ShowNotification("Notification", notification);
            });
        }

        private void LoadUsersAndGroups()
        {
            using (var context = new AppDbContext())
            {
                var users = context.Users.ToList();
                UserList.ItemsSource = users;

                var groups = context.Chats.Where(c => c.IsGroup).ToList();
                GroupList.ItemsSource = groups;
            }
        }

        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserList.SelectedItem is User selectedUser)
            {
                OpenChat(selectedUser);
            }
        }

        private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupList.SelectedItem is Chat selectedGroup)
            {
                OpenGroupChat(selectedGroup);
            }
        }

        private void OpenChat(User selectedUser)
        {
            using (var context = new AppDbContext())
            {
                var chat = context.UserChats
                    .Where(uc => (uc.UserId == _loggedInUser.UserId && uc.Chat.UserChats.Any(uc2 => uc2.UserId == selectedUser.UserId)) ||
                                 (uc.UserId == selectedUser.UserId && uc.Chat.UserChats.Any(uc2 => uc2.UserId == _loggedInUser.UserId)))
                    .Select(uc => uc.Chat)
                    .FirstOrDefault();

                if (chat == null)
                {
                    chat = new Chat
                    {
                        ChatName = $"{_loggedInUser.Username}, {selectedUser.Username}",
                        IsGroup = false,
                        UserChats = new List<UserChat>
                        {
                            new UserChat { UserId = _loggedInUser.UserId },
                            new UserChat { UserId = selectedUser.UserId }
                        }
                    };
                    context.Chats.Add(chat);
                    context.SaveChanges();
                }

                var messages = context.Messages
                    .Include(m => m.Sender)
                    .Where(m => m.ChatId == chat.ChatId)
                    .ToList();

                _messages.Clear();
                foreach (var message in messages)
                {
                    _messages.Add(message);
                }

                ChatWithTextBlock.Text = selectedUser.Username;
            }
        }

        private void OpenGroupChat(Chat selectedGroup)
        {
            using (var context = new AppDbContext())
            {
                var messages = context.Messages
                    .Include(m => m.Sender)
                    .Where(m => m.ChatId == selectedGroup.ChatId)
                    .ToList();

                _messages.Clear();
                foreach (var message in messages)
                {
                    _messages.Add(message);
                }

                ChatWithTextBlock.Text = selectedGroup.ChatName;
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is User selectedUser)
            {
                using (var context = new AppDbContext())
                {
                    var chat = context.UserChats
                        .Where(uc => (uc.UserId == _loggedInUser.UserId && uc.Chat.UserChats.Any(uc2 => uc2.UserId == selectedUser.UserId)) ||
                                     (uc.UserId == selectedUser.UserId && uc.Chat.UserChats.Any(uc2 => uc2.UserId == _loggedInUser.UserId)))
                        .Select(uc => uc.Chat)
                        .FirstOrDefault();

                    if (chat != null)
                    {
                        var message = new Message
                        {
                            Content = MessageTextBox.Text,
                            Timestamp = DateTime.Now,
                            SenderId = _loggedInUser.UserId,
                            ChatId = chat.ChatId
                        };

                        context.Messages.Add(message);
                        context.SaveChanges();

                        _messages.Add(message);

                        await _signalRClient.SendMessageAsync(_loggedInUser.Username, message.Content);

                        MessageTextBox.Clear();
                    }
                }
            }
            else if (GroupList.SelectedItem is Chat selectedGroup)
            {
                using (var context = new AppDbContext())
                {
                    var message = new Message
                    {
                        Content = MessageTextBox.Text,
                        Timestamp = DateTime.Now,
                        SenderId = _loggedInUser.UserId,
                        ChatId = selectedGroup.ChatId
                    };

                    context.Messages.Add(message);
                    context.SaveChanges();

                    _messages.Add(message);

                    await _signalRClient.SendGroupMessageAsync(selectedGroup.ChatName, _loggedInUser.Username, message.Content);

                    MessageTextBox.Clear();
                }
            }
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var createGroupWindow = new CreateGroupWindow(_loggedInUser);
            createGroupWindow.ShowDialog();

            if (createGroupWindow.DialogResult == true)
            {
                LoadUsersAndGroups();
            }
        }

        private void ShowNotification(string title, string message)
        {
            var notificationWindow = new NotificationWindow(title, message);
            notificationWindow.Show();
        }
    }
}
