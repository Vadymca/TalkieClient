using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalkieClient.Models
{
    public enum MessageType
    {
        Text,
        File
    }
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [ForeignKey("Chat")]
        public int ChatId { get; set; }

        [ForeignKey("User")]
        public int SenderId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public virtual Chat Chat { get; set; }
        public virtual User Sender { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        public virtual ICollection<File> Files { get; set; } = new List<File>();
       
    }
}
