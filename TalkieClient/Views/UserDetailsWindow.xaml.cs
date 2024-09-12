using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using TalkieClient.Data;

namespace TalkieClient.Views
{
    public partial class UserDetailsWindow : Window
    {
        private User _currentUser;
        private User _selectedUser;
        public bool IsReadOnly { get; set; }

        public UserDetailsWindow(User selectedUser, User currentUser)
        {
            InitializeComponent();
            _selectedUser = selectedUser;
            _currentUser = currentUser;
            var converter = new ByteArrayToImageConverter();
            DataContext = _selectedUser;

            UsernameTextBox.Text = _selectedUser.Username;
            EmailTextBox.Text = _selectedUser.Email;

            if (_selectedUser.UserId == _currentUser.UserId)
            {
                IsReadOnly = false;
                _selectedUser.Status = "Online";
                PasswordLabel.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;
                UpdateButton.Visibility = Visibility.Visible;
                ChangeAvatarButton.Visibility = Visibility.Visible;
            }
            else
            {
                IsReadOnly = true;
            }

            // Uploading an avatar
            if (_selectedUser?.Avatar != null && _selectedUser.Avatar.Length > 0)
            {
                AvatarImage.Source = (BitmapImage)converter.Convert(_selectedUser.Avatar, typeof(BitmapImage), null, CultureInfo.InvariantCulture);
                ChangeAvatarButton.Content = "Change Avatar";
            }
            else
            {
                AvatarImage.Source = null;
                ChangeAvatarButton.Content = "Add Avatar";
            }
        }

        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                byte[] imageData = File.ReadAllBytes(openFileDialog.FileName);
                _selectedUser.Avatar = imageData; 
                AvatarImage.Source = ConvertByteArrayToImage(imageData);
                SaveChangesToDatabase();
            }
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
                context.Users.Update(_selectedUser);
                context.SaveChanges();
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedUser.Username = UsernameTextBox.Text;
            _selectedUser.Email = EmailTextBox.Text;

            if (!string.IsNullOrEmpty(PasswordBox.Password))
            {
                _selectedUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password);
            }
            SaveChangesToDatabase();
            UpdateMainWindow();
            this.Close();
        }

        private void UpdateMainWindow()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                if (_selectedUser.UserId == _currentUser.UserId)
                {
                    _currentUser.Status = "Online";
                    _currentUser.IsOnline = true;
                    SaveChangesToDatabase();  
                }

                // Updating the list of users and groups
                mainWindow.RefreshUsersAndGroups(_selectedUser.UserId, null);

                // Update the display in the main window
                mainWindow.UserList.Items.Refresh();
                mainWindow.GroupList.Items.Refresh();
            }
        }
    }
}
