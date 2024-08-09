using System.ComponentModel.DataAnnotations;
using TalkieClient.Models;

public class Chat
{
    [Key]
    public int ChatId { get; set; }

    [Required]
    public string ChatName { get; set; }

    public bool IsGroup { get; set; }

    public virtual ICollection<UserChat> UserChats { get; set; }
    public virtual ICollection<Message> Messages { get; set; }
}

