using System.ComponentModel.DataAnnotations;
using TalkieClient.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public string Avatar { get; set; }
    public string Status { get; set; }
    public string Role { get; set; }

    public virtual ICollection<UserChat> UserChats { get; set; }
    public virtual ICollection<Message> Messages { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; }
}
