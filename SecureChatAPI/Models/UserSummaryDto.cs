namespace SecureChatAPI.Models
{
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int LastMessageSenderId { get; set; }
        public bool LastMessageIsRead { get; set; }
        public bool LastMessageIsDelivered { get; set; }
        
        public bool IsGroup { get; set; } = false;
        public string? ProfileIcon { get; set; } // Fotoğraf verisi buraya gelecek
    }
}