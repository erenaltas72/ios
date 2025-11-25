namespace SecureChatAPI.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GroupIcon { get; set; } = "group"; // Base64 Fotoğraf
        public int CreatorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}