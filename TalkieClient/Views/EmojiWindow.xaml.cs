using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
                // Сохраняем выбранный эмодзи
                SelectedEmoji = button.Content.ToString();
                DialogResult = true;  // Закрываем окно и возвращаем результат
                this.Close();
            }
        }
    }
}
