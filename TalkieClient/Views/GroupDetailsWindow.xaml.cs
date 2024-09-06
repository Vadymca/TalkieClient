using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using TalkieClient.Data;
using TalkieClient.Models;
using System.Threading.Tasks;

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

            // Проверка прав модератора
            SetModeratorPermissions();

            // Загрузка данных группы
            LoadGroupData();

            // Загрузка участников группы
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

            // Загрузка аватара
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

                // Обновление списка участников после удаления
                LoadGroupMembersAsync();
            }
            else
            {
                MessageBox.Show("Please select a member to remove.");
            }
        }

        private void UpdateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохранение изменений в базе данных
            SaveChangesToDatabase();

            // Обновление данных в главном окне
            UpdateMainWindow();

            // Закрытие окна после обновления
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
