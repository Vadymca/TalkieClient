using Microsoft.EntityFrameworkCore;
using System.Windows;
using TalkieClient.Data;
using TalkieClient.Models;

namespace TalkieClient.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var user = new User
            {
                Username = UsernameTextBox.Text,
                Email = EmailTextBox.Text,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password),
                Role = RoleComboBox.Text,
                Avatar = "", 
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
