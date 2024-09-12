using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using TalkieClient.Data;

namespace TalkieClient.Views
{
    public partial class GroupDetailsWindow : Window
    {
        private Chat _currentGroup;
        private User _currentUser;

        public GroupDetailsWindow(Chat group, User currentUser)
        {
            InitializeComponent();
            _currentGroup = group;
            _currentUser = currentUser;
            DataContext = this;

            SetModeratorPermissions();
            LoadGroupData();
            LoadGroupMembersAsync();
        }

        private void SetModeratorPermissions()
        {
            bool isModerator = _currentUser.Role == "Moderator";

            GroupNameTextBox.IsEnabled = isModerator;
            ChangeAvatarButton.Visibility = isModerator ? Visibility.Visible : Visibility.Hidden;
            RemoveMemberButton.Visibility = isModerator ? Visibility.Visible : Visibility.Hidden;
            UpdateGroupButton.Visibility = isModerator ? Visibility.Visible : Visibility.Hidden;
        }

        private void LoadGroupData()
        {
            GroupNameTextBox.Text = _currentGroup.ChatName;

            // Uploading an avatar
            if (_currentGroup.Avatar != null && _currentGroup.Avatar.Length > 0)
            {
                GroupAvatar.Source = ConvertByteArrayToImage(_currentGroup.Avatar);
            }
            else
            {
                GroupAvatar.Source = null;
                ChangeAvatarButton.Content = "Add Avatar";
            }
        }

        private async void LoadGroupMembersAsync()
        {
            using (var context = new AppDbContext())
            {
                var members = await Task.Run(() =>
                    context.UserChats
                        .Where(uc => uc.ChatId == _currentGroup.ChatId)
                        .Select(uc => uc.User)
                        .ToList());

                MembersListBox.ItemsSource = members;
            }
        }

        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                byte[] imageData = System.IO.File.ReadAllBytes(openFileDialog.FileName);
                _currentGroup.Avatar = imageData;
                GroupAvatar.Source = ConvertByteArrayToImage(imageData);
            }
        }

        private void RemoveMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (MembersListBox.SelectedItem is User selectedUser)
            {
                using (var context = new AppDbContext())
                {
                    var userChat = context.UserChats
                        .FirstOrDefault(uc => uc.ChatId == _currentGroup.ChatId && uc.UserId == selectedUser.UserId);

                    if (userChat != null)
                    {
                        context.UserChats.Remove(userChat);
                        context.SaveChanges();
                    }
                }
                LoadGroupMembersAsync();
            }
            else
            {
                MessageBox.Show("Please select a member to remove.");
            }
        }

        private void UpdateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            SaveChangesToDatabase();
            UpdateMainWindow();
            this.Close();
        }

        private BitmapImage ConvertByteArrayToImage(byte[] imageData)
        {
            using (var stream = new MemoryStream(imageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        private void SaveChangesToDatabase()
        {
            using (var context = new AppDbContext())
            {
                _currentGroup.ChatName = GroupNameTextBox.Text;
                context.Chats.Update(_currentGroup);
                context.SaveChanges();
            }
        }

        private void UpdateMainWindow()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.RefreshUsersAndGroups(null, _currentGroup.ChatId);
            }
        }
    }
}
