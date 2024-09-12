using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TalkieClient.Models;

public class User : INotifyPropertyChanged
{
    [Key]
    public int UserId { get; set; }

    private string _username;
    [Required]
    public string Username
    {
        get => _username;
        set
        {
            if (_username != value)
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
    }

    private string _email;
    [Required]
    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
    }

    [Required]
    public string PasswordHash { get; set; }

    private byte[] _avatar;
    public byte[] Avatar
    {
        get => _avatar;
        set
        {
            if (_avatar != value)
            {
                _avatar = value;
                OnPropertyChanged(nameof(Avatar));
            }
        }
    }

    private string _status;
    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    public string Role { get; set; }

    private bool _isOnline;
    public bool IsOnline
    {
        get => _isOnline;
        set
        {
            if (_isOnline != value)
            {
                _isOnline = value;
                OnPropertyChanged(nameof(IsOnline));
            }
        }
    }

    public virtual ICollection<UserChat> UserChats { get; set; }
    public virtual ICollection<Message> Messages { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
