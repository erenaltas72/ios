namespace SecureChatAPI.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public int? GroupId { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ScheduledTime { get; set; }

        public bool IsDelivered { get; set; } = false;
        public bool IsRead { get; set; } = false;

        public bool IsDeletedForSender { get; set; } = false;
        public bool IsDeletedForReceiver { get; set; } = false;
        public bool IsDeletedForEveryone { get; set; } = false;

        public string? StarTag { get; set; }
        public bool IsPinned { get; set; } = false;
    }

    public class StarredMessageDto
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? GroupName { get; set; }
        public int? GroupId { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? StarTag { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
    }

    // YENİ: Sohbet Ekranı İçin Detaylı Mesaj Paketi
    public class MessageDetailDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty; // Kim attı?
        public string SenderIcon { get; set; } = "person";     // Fotosu ne?
        public string EncryptedContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeletedForEveryone { get; set; }
        public string? StarTag { get; set; }
        public bool IsPinned { get; set; }
    }
}