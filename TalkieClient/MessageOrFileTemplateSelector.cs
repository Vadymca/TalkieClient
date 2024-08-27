using System.Windows;
using System.Windows.Controls;
using TalkieClient.Models;

namespace TalkieClient
{
    public class MessageOrFileTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MessageTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Message message)
            {
                if (message.Type == MessageType.File)
                    return FileTemplate;

                return MessageTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
