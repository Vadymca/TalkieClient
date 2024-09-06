using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using TalkieClient.Data;
using TalkieClient.Models;

namespace TalkieClient.Views
{
    public partial class CreateGroupWindow : Window
    {
        private User _loggedInUser;
        private byte[] avatarData;

        public CreateGroupWindow(User loggedInUser)
        {
            InitializeComponent();
            _loggedInUser = loggedInUser;
            LoadUsers();
        }
        private void UploadAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (openFileDialog.ShowDialog() == true)
            {
                avatarData = System.IO.File.ReadAllBytes(openFileDialog.FileName);
                AvatarPreview.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }
        private void LoadUsers()
        {
            using (var context = new AppDbContext())
            {
                var users = context.Users.Where(u => u.UserId != _loggedInUser.UserId).ToList();
                UserSelectionList.ItemsSource = users;
            }
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUsers = UserSelectionList.SelectedItems.Cast<User>().ToList();
            if (selectedUsers.Count > 0 && !string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
            {
                using (var context = new AppDbContext())
                {
                    var group = new Chat
                    {
                        ChatName = GroupNameTextBox.Text,
                        IsGroup = true,
                        Avatar = avatarData,
                        UserChats = new ObservableCollection<UserChat>
                        {
                            new UserChat { UserId = _loggedInUser.UserId }
                        }
                    };

                    foreach (var user in selectedUsers)
                    {
                        group.UserChats.Add(new UserChat { UserId = user.UserId });
                    }

                    context.Chats.Add(group);
                    context.SaveChanges();

                    DialogResult = true;
                    Close();
                }
            }
            else
            {
                MessageBox.Show("Please enter a group name and select at least one user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
