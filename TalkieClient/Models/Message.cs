using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows;

namespace TalkieClient.Models
{
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

        public HorizontalAlignment Alignment
        {
            get
            {
                return SenderId == App.CurrentUserId ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
        }
    }
}
