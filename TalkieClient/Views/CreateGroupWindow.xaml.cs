using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TalkieClient.Data;
using TalkieClient.Models;

namespace TalkieClient.Views
{
    public partial class CreateGroupWindow : Window
    {
        private User _loggedInUser;

        public CreateGroupWindow(User loggedInUser)
        {
            InitializeComponent();
            _loggedInUser = loggedInUser;
            LoadUsers();
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
            if (GroupNameTextBox.Text == "Enter group name" || string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
            {
                MessageBox.Show("Please enter a group name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedUsers = UserSelectionList.SelectedItems.Cast<User>().ToList();
            if (selectedUsers.Count > 0)
            {
                using (var context = new AppDbContext())
                {
                    var group = new Chat
                    {
                        ChatName = GroupNameTextBox.Text,
                        IsGroup = true,
                        UserChats = new List<UserChat>
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
                MessageBox.Show("Please select at least one user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
