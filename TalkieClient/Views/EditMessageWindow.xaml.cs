using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TalkieClient.Models;

namespace TalkieClient.Views
{
    public partial class EditMessageWindow : Window
    {
    public string UpdatedMessageContent { get; private set; }
        public EditMessageWindow(Message message)
        {
            InitializeComponent();
            DataContext = this;
            MessageContentTextBox.Text = message.Content;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            UpdatedMessageContent = MessageContentTextBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}
