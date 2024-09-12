using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TalkieClient.Data;
using TalkieClient.Models;
using TalkieClient.SignalR;

namespace TalkieClient.Views
{
    public partial class MainWindow : Window
    {
        private User _loggedInUser;
        private SignalRClient _signalRClient;
        private ObservableCollection<Message> _messages;
        private ObservableCollection<User> _users;
        private ObservableCollection<Chat> _groups;
        private HashSet<string> _onlineUsers = new HashSet<string>();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.Close();

        }

        public MainWindow(User loggedInUser)
        {
            if (loggedInUser == null)
                throw new ArgumentNullException(nameof(loggedInUser), "Logged-in user cannot be null.");

            InitializeComponent();
            DataContext = this;
            _loggedInUser = loggedInUser;
            App.CurrentUserId = _loggedInUser.UserId;
            CurrentUserTextBlock.Text = $"Name: {_loggedInUser.Username}";
            CurrentUserTextBlock.ToolTip = $"Role: {_loggedInUser.Role}";
            _users = App.Users;
            _groups = new ObservableCollection<Chat>();

            UserList.ItemsSource = _users;
            GroupList.ItemsSource = _groups;

            LoadUsersAndGroupsAsync();

            _signalRClient = new SignalRClient(_loggedInUser.Username);

            _signalRClient.OnMessageReceived += SignalRClient_OnMessageReceived;
            _signalRClient.OnPrivateMessageReceived += SignalRClient_OnPrivateMessageReceived;
            _signalRClient.OnNotificationReceived += SignalRClient_OnNotificationReceived;
            _signalRClient.OnUserOnline += SignalRClient_OnUserOnline;
            _signalRClient.OnUserOffline += SignalRClient_OnUserOffline;
            _signalRClient.OnFileReceived += _signalRClient_OnFileReceived;

            _messages = new ObservableCollection<Message>();
            MessageList.ItemsSource = _messages;

            StartSignalRClient();

            try
            {
                // Initializing the template selector
                var templateSelector = new MessageOrFileTemplateSelector
                {
                    MessageTemplate = Resources["MessageTemplate"] as DataTemplate,
                    FileTemplate = Resources["FileTemplate"] as DataTemplate
                };

                // Set DataContext for ListBox if templates are found successfully
                if (templateSelector.MessageTemplate != null && templateSelector.FileTemplate != null)
                {
                    MessageList.ItemTemplateSelector = templateSelector;
                }
                else
                {
                    MessageBox.Show("One or both DataTemplates could not be found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

        }

        private void _signalRClient_OnFileReceived(string arg1, string arg2, byte[] arg3)
        {
            throw new NotImplementedException();
        }

        private async void StartSignalRClient()
        {
            await _signalRClient.StartAsync();
        }

        private void SignalRClient_OnMessageReceived(string user, string message)
        {
            Dispatcher.InvokeAsync(() =>
            {
                _messages.Add(new Message { Sender = new User { Username = user }, Content = message, Timestamp = DateTime.Now });
                ShowNotification($"New message from {user}", message);
            });
        }

        private void SignalRClient_OnPrivateMessageReceived(string fromUser, string message)
        {
            Dispatcher.InvokeAsync(() =>
            {
                _messages.Add(new Message { Sender = new User { Username = fromUser }, Content = $"(Private) {message}", Timestamp = DateTime.Now });
                ShowNotification($"Private message from {fromUser}", message);
            });
        }

        private void SignalRClient_OnNotificationReceived(string notification)
        {
            Dispatcher.InvokeAsync(() =>
            {
                ShowNotification("Notification", notification);
            });
        }

        private void SignalRClient_OnUserOnline(string username)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var user = _users.FirstOrDefault(u => u.Username == username);
                if (user != null)
                {
                    user.Status = "Online";
                    user.IsOnline = true;
                    SortUsersByStatus();

                    // Update the data source for the user list
                    UserList.ItemsSource = null;
                    UserList.ItemsSource = _users;
                }
            });
        }

        private bool IsUserOnline(string username)
        {
            return _onlineUsers.Contains(username);
        }

        private void SignalRClient_OnUserOffline(string username)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var user = _users.FirstOrDefault(u => u.Username == username);
                if (user != null)
                {
                    user.Status = "Offline";
                    user.IsOnline = false;
                    SortUsersByStatus();

                    
                    UserList.ItemsSource = null;
                    UserList.ItemsSource = _users;
                }
            });
        }

        private void SortUsersByStatus()
        {
            _users = new ObservableCollection<User>(_users.OrderByDescending(u => u.IsOnline).ThenBy(u => u.Username));
            UserList.ItemsSource = _users;

        }

        private async void LoadUsersAndGroupsAsync()
        {
            using (var context = new AppDbContext())
            {

                // Loading all users from the database
                var users = await context.Users.ToListAsync() ?? new List<User>();
                

                _users.Clear();

                foreach (var user in users)
                {
                    // Check if the user is on the network through your SignalR connection or other network communication
                    if (IsUserOnline(user.Username))
                    {
                        user.Status = "Online";
                        user.IsOnline = true;
                    }
                    else
                    {
                        user.Status = "Offline";
                        user.IsOnline = false;
                    }

                    _users.Add(user);
                }
                SortUsersByStatus();

                UserList.ItemsSource = null;
                UserList.ItemsSource = _users;

                // Loading all groups from the database
                var groups = await context.Chats.Where(c => c.IsGroup).ToListAsync() ?? new List<Chat>();

                _groups.Clear();

                foreach (var group in groups)
                {
                    _groups.Add(group);
                }

                GroupList.ItemsSource = null;
                GroupList.ItemsSource = _groups;
            }
        }


        public void RefreshUsersAndGroups(int? updatedUserId = null, int? updatedGroupId = null)
{
    try
    {
        using (var context = new AppDbContext())
        {
            // If you need to update users
            if (updatedUserId.HasValue)
            {
                // Loading an updated user from the database
                var updatedUser = context.Users
                    .Include(u => u.UserChats)
                    .FirstOrDefault(u => u.UserId == updatedUserId.Value);

                if (updatedUser != null)
                {
                    // Check if the user is in the list
                    var userInList = _users.FirstOrDefault(u => u.UserId == updatedUser.UserId);

                    if (userInList != null)
                    {
                        userInList.Username = updatedUser.Username;
                        userInList.Avatar = updatedUser.Avatar;
                        userInList.Status = updatedUser.Status;

                        // Updating an item in a collection
                        var index = _users.IndexOf(userInList);
                        _users[index] = userInList;
                    }
                    else
                    {
                        _users.Add(updatedUser);
                    }
                }
            }

            // If you need to update the groups
            if (updatedGroupId.HasValue)
            {
                // Loading an updated group from the database
                var updatedGroup = context.Chats
                    .Include(g => g.UserChats)
                    .FirstOrDefault(g => g.ChatId == updatedGroupId.Value);

                if (updatedGroup != null)
                {
                    var groupInList = _groups.FirstOrDefault(g => g.ChatId == updatedGroup.ChatId);

                    if (groupInList != null)
                    {
                        // Updating group data
                        groupInList.ChatName = updatedGroup.ChatName;
                        groupInList.Avatar = updatedGroup.Avatar;
                        groupInList.UserChats = updatedGroup.UserChats;

                        // Updating an item in a collection
                        var index = _groups.IndexOf(groupInList);
                        _groups[index] = groupInList;
                    }
                    else
                    {
                        _groups.Add(updatedGroup);
                    }
                }
            }

            // Binding data to lists in the UI
            UserList.ItemsSource = _users;
            UserList.Items.Refresh();

            GroupList.ItemsSource = _groups;
            GroupList.Items.Refresh();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error when updating users and groups: {ex.Message}");
    }
}


        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserList.SelectedItem is User selectedUser)
            {
                GroupList.SelectedItem = null;
                OpenChat(selectedUser);
            }
        }

        private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupList.SelectedItem is Chat selectedGroup)
            {
                UserList.SelectedItem = null;
                OpenGroupChat(selectedGroup);
            }
        }

        private void OpenChat(User selectedUser)
        {
            if (selectedUser == null)
                throw new ArgumentNullException(nameof(selectedUser), "Selected user cannot be null.");

            using (var context = new AppDbContext())
            {
                var chat = GetOrCreateChatWithUser(context, selectedUser);

                if (chat != null)
                {
                    LoadMessagesForChat(context, chat);
                    ChatWithTextBlock.Text = selectedUser.Username;
                }
            }
        }

        private void OpenGroupChat(Chat selectedGroup)
        {
            if (selectedGroup == null)
                throw new ArgumentNullException(nameof(selectedGroup), "Selected group cannot be null.");

            using (var context = new AppDbContext())
            {
                LoadMessagesForChat(context, selectedGroup);
                ChatWithTextBlock.Text = selectedGroup.ChatName;
            }
        }

        private Chat GetOrCreateChatWithUser(AppDbContext context, User selectedUser)
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
                    Avatar = _loggedInUser.Avatar,
                    UserChats = new List<UserChat>
                    {
                        new UserChat { UserId = _loggedInUser.UserId },
                        new UserChat { UserId = selectedUser.UserId }
                    }
                };
                context.Chats.Add(chat);
                context.SaveChanges();
            }

            return chat;
        }

        private void LoadMessagesForChat(AppDbContext context, Chat chat)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");

            // Load messages for the current chat
            var messages = context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Files)
                .Where(m => m.ChatId == chat.ChatId)
                .ToList();

            _messages.Clear();

            // Add uploaded messages
            foreach (var message in messages)
            {
                _messages.Add(message);
            }

            MessageList.Items.Refresh();
            MessageList.ItemsSource = _messages;
        }

        private async Task SendMessageToChatAsync(Chat chat, string messageContent)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");

            if (string.IsNullOrWhiteSpace(messageContent))
                throw new ArgumentNullException(nameof(messageContent), "Message content cannot be null or empty.");



            using (var context = new AppDbContext())
            {

                if (chat.ChatId == 0)
                    throw new ArgumentException("ChatId cannot be 0 or default value.", nameof(chat.ChatId));

                if (chat.UserChats == null || !chat.UserChats.Any())
                {
                    throw new InvalidOperationException("No users found for this chat.");
                }

                var message = new Message
                {
                    Content = messageContent,
                    Timestamp = DateTime.Now,
                    SenderId = _loggedInUser.UserId,
                    ChatId = chat.ChatId
                };

                context.Messages.Add(message);
                await context.SaveChangesAsync();

                _messages.Add(message);

                if (chat.IsGroup)
                {
                    if (string.IsNullOrWhiteSpace(chat.ChatName))
                        throw new ArgumentException("ChatName cannot be null or empty for group chat.", nameof(chat.ChatName));

                    try
                    {
                        await _signalRClient.SendGroupMessageAsync(chat.ChatName, _loggedInUser.Username, message.Content);
                        Console.WriteLine($"Message sent to group {chat.ChatName} by {_loggedInUser.Username}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending message to group {chat.ChatName}: {ex.Message}");
                    }
                }
                else
                {
                    var recipient = chat.UserChats.FirstOrDefault(uc => uc.UserId != _loggedInUser.UserId)?.User.Username;

                    if (string.IsNullOrWhiteSpace(recipient))
                        throw new ArgumentException("Recipient cannot be null or empty in private chat.", nameof(recipient));

                    await _signalRClient.SendPrivateMessageAsync(_loggedInUser.Username, recipient, message.Content);
                }

                MessageTextBox.Clear();
            }
        }



        private async Task SendFileToChatAsync(Chat chat, Models.File file)
        {
            if (chat == null)
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");

            if (file == null)
                throw new ArgumentNullException(nameof(file), "File cannot be null.");

            if (_loggedInUser == null)
                throw new InvalidOperationException("Logged-in user is not set.");

            // Check that the file has a path
            if (string.IsNullOrWhiteSpace(file.FilePath))
                throw new ArgumentException("FilePath cannot be null or empty.", nameof(file.FilePath));

            using (var context = new AppDbContext())
            {
                // Load the chat room together with UserChats and associated users
                chat = context.Chats
                              .Include(c => c.UserChats)
                              .ThenInclude(uc => uc.User)
                              .FirstOrDefault(c => c.ChatId == chat.ChatId);

                if (chat == null)
                    throw new InvalidOperationException("Chat not found.");

                if (chat.UserChats == null || !chat.UserChats.Any())
                {
                    throw new InvalidOperationException("No users found for this chat.");
                }

                var message = new Message
                {
                    SenderId = _loggedInUser.UserId,
                    ChatId = chat.ChatId,
                    Content = $"File: {file.FileName}",
                    Files = new List<Models.File> { file },
                    Timestamp = DateTime.Now,
                    Type = MessageType.File
                };

                context.Messages.Add(message);
                await context.SaveChangesAsync();

                _messages.Add(message);

                if (chat.IsGroup)
                {
                    await _signalRClient.SendGroupMessageAsync(chat.ChatName, _loggedInUser.Username, $"File: {file.FileName}");
                }
                else
                {
                    var userChat = chat.UserChats.FirstOrDefault(uc => uc.UserId != _loggedInUser.UserId);
                    if (userChat == null)
                        throw new InvalidOperationException("No other user found in this chat.");

                    var recipient = userChat.User?.Username;
                    if (string.IsNullOrWhiteSpace(recipient))
                        throw new InvalidOperationException("Recipient username is null or empty.");

                    await _signalRClient.SendPrivateMessageAsync(_loggedInUser.Username, recipient, $"File: {file.FileName}");
                }
            }
        }

        private void MessageList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MessageList.SelectedItem is Message selectedMessage && selectedMessage.Files?.Count > 0)
            {
                var file = selectedMessage.Files.First();
                if (!string.IsNullOrEmpty(file.FilePath) && System.IO.File.Exists(file.FilePath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(file.FilePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("File does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UserList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UserList.SelectedItem is User selectedUser)
            {
                UserDetailsWindow userDetailsWindow = new UserDetailsWindow(selectedUser, _loggedInUser);
                userDetailsWindow.ShowDialog();
            }
        }

        private void GroupList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedGroup = GroupList.SelectedItem as Chat;
            if (selectedGroup != null)
            {
                var groupDetailsWindow = new GroupDetailsWindow(selectedGroup, _loggedInUser);
                groupDetailsWindow.ShowDialog();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }


        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                MessageBox.Show("Message cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (UserList.SelectedItem == null && GroupList.SelectedItem == null)
            {
                MessageBox.Show("Please select a user or group to send a message.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                            Type = MessageType.Text,
                            ChatId = chat.ChatId
                        };

                        context.Messages.Add(message);
                        context.SaveChanges();

                        _messages.Add(message);

                        await _signalRClient.SendMessageAsync(_loggedInUser.Username, message.Content);

                        MessageTextBox.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Unable to find or create a chat with the selected user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            else
            {
                MessageBox.Show("Please select a valid user or group.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;
                var fileData = System.IO.File.ReadAllBytes(fileName);

                var file = new File
                {
                    FileName = System.IO.Path.GetFileName(fileName),
                    Data = fileData,
                    FilePath = fileName 
                };

                if (UserList.SelectedItem is User selectedUser)
                {
                    using (var context = new AppDbContext())
                    {
                        var chat = GetOrCreateChatWithUser(context, selectedUser);
                        if (chat != null)
                        {
                            await SendFileToChatAsync(chat, file);
                        }
                    }
                }
                else if (GroupList.SelectedItem is Chat selectedGroup)
                {
                    await SendFileToChatAsync(selectedGroup, file);
                }
            }
        }

        private void EmojiPickerButton_Click(object sender, RoutedEventArgs e)
        {
            EmojiWindow emojiWindow = new EmojiWindow();
            if (emojiWindow.ShowDialog() == true)
            {
                MessageTextBox.Text += emojiWindow.SelectedEmoji;
            }
        }

        private void ShowNotification(string title, string message)
        {
            var notificationWindow = new NotificationWindow(title, message);
            notificationWindow.Show();

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            timer.Tick += (sender, args) =>
            {
                notificationWindow.Close();
                timer.Stop();
            };

            timer.Start();
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var createGroupWindow = new CreateGroupWindow(_loggedInUser);
            createGroupWindow.ShowDialog();

            if (createGroupWindow.DialogResult == true)
            {
                LoadUsersAndGroupsAsync();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                UserList.ItemsSource = _users;
                GroupList.ItemsSource = _groups;
            }
            else
            {
                var filteredUsers = _users.Where(u => u.Username.ToLower().Contains(searchText)).ToList();
                var filteredGroups = _groups.Where(g => g.ChatName.ToLower().Contains(searchText)).ToList();

                UserList.ItemsSource = filteredUsers;
                GroupList.ItemsSource = filteredGroups;
            }
        }

        private void EditMessage_Click(object sender, RoutedEventArgs e)
        {
            if (MessageList.SelectedItem is Message selectedMessage)
            {
                EditMessageWindow editWindow = new EditMessageWindow(selectedMessage);
                if (editWindow.ShowDialog() == true)
                {
                    using (var context = new AppDbContext())
                    {
                        var message = context.Messages.Find(selectedMessage.MessageId);
                        if (message != null)
                        {
                            message.Content = editWindow.UpdatedMessageContent;
                            context.SaveChanges();
                            RefreshMessagesList(selectedMessage.ChatId);
                        }
                    }
                }
            }
        }

        private void DeleteMessage_Click(object sender, RoutedEventArgs e)
        {
            if (MessageList.SelectedItem is Message selectedMessage)
            {
                if (MessageBox.Show("Are you sure you want to delete this message?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var context = new AppDbContext())
                    {
                        var message = context.Messages.Find(selectedMessage.MessageId);
                        if (message != null)
                        {
                            context.Messages.Remove(message);
                            context.SaveChanges();
                            RefreshMessagesList(selectedMessage.ChatId);
                        }
                    }
                }
            }
        }

        private void RefreshMessagesList(int chatId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Downloading messages for the specified chat from the database, including sender information
                    var updatedMessages = context.Messages
                        .Where(m => m.ChatId == chatId)
                        .OrderBy(m => m.Timestamp)
                        .Include(m => m.Sender)
                        .Include(m => m.Files)
                        .ToList();

                    _messages = new ObservableCollection<Message>(updatedMessages);

                    MessageList.ItemsSource = _messages;
                    MessageList.Items.Refresh(); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении списка сообщений: {ex.Message}");
            }
        }
    }
}
