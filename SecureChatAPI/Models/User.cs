namespace SecureChatAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        public string NickName { get; set; } = string.Empty;
        public string About { get; set; } = "Merhaba, SecureChat kullanıyorum!";
        public string ProfileIcon { get; set; } = "person";

        // YENİ: Kullanıcının "Benden Sil" dediği mesajların ID'leri (Virgülle ayrılmış string: "1,5,99,")
        public string HiddenMessageIds { get; set; } = ""; 
    }
}