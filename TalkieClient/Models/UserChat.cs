using System.ComponentModel.DataAnnotations;

namespace TalkieClient.Models
{
    public class UserChat
    {
        public int UserId { get; set; }
        public int ChatId { get; set; }

        public virtual User User { get; set; }
        public virtual Chat Chat { get; set; }
    }



}
