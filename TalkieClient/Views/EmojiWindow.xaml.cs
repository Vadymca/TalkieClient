using System.Windows;
using System.Windows.Controls;

namespace TalkieClient.Views
{
    public partial class EmojiWindow : Window
    {
        public string SelectedEmoji { get; private set; }

        public EmojiWindow()
        {
            InitializeComponent();
        }

        private void EmojiButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                SelectedEmoji = button.Content.ToString();
                DialogResult = true; 
                this.Close();
            }
        }
    }
}
