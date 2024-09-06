using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using TalkieClient.Data;
using TalkieClient.Models;

namespace TalkieClient.Views
{
    public partial class RegisterWindow : Window
    {
        private byte[] avatarData;
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void UploadAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                avatarData = System.IO.File.ReadAllBytes(openFileDialog.FileName);
                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(avatarData))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                AvatarImage.Source = bitmap;
            }
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var user = new User
            {
                Username = UsernameTextBox.Text,
                Email = EmailTextBox.Text,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password),
                Role = RoleComboBox.Text,
                Avatar = avatarData, 
                Status = "Active"
            };

            try
            {
                using (var context = new AppDbContext())
                {
                    context.Users.Add(user);
                    context.SaveChanges();
                }
                MessageBox.Show("Registration successful!");
                this.Close();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"An error occurred while saving the user: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


    }
}
