using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TalkieClient.Models;

public class Chat : INotifyPropertyChanged
{
    [Key]
    public int ChatId { get; set; }

    private string _chatName;
    [Required]
    public string ChatName
    {
        get => _chatName;
        set
        {
            if (_chatName != value)
            {
                _chatName = value;
                OnPropertyChanged(nameof(ChatName));
            }
        }
    }

    private bool _isGroup;
    public bool IsGroup
    {
        get => _isGroup;
        set
        {
            if (_isGroup != value)
            {
                _isGroup = value;
                OnPropertyChanged(nameof(IsGroup));
            }
        }
    }

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

    public virtual ICollection<UserChat> UserChats { get; set; }
    public virtual ICollection<Message> Messages { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
