namespace SecureChatAPI.Models
{
    public class GroupMember
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}