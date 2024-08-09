using System.ComponentModel.DataAnnotations;

namespace TalkieClient.Models
{
    public class File
    {
        [Key]
        public int FileID { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public byte[] Data { get; set; }

        public int? MessageID { get; set; }
        public virtual Message Message { get; set; }
    }

}