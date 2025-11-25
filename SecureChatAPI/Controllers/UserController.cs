using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureChatAPI.Models;

namespace SecureChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ChatContext _context;
        public UsersController(ChatContext context) { _context = context; }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] User user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null) { if (existingUser.Password == user.Password) return existingUser; else return Unauthorized("Parola yanlış!"); }
            user.NickName = "@" + user.Username; user.ProfileIcon = "person"; 
            _context.Users.Add(user); await _context.SaveChangesAsync(); return user;
        }

        [HttpGet("search/{username}")]
        public async Task<ActionResult<User>> SearchUser(string username) { var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username); if (user == null) return NotFound("Kullanıcı bulunamadı."); return user; }

        [HttpGet("profile/{id}")]
        public async Task<ActionResult<User>> GetProfile(int id) { var user = await _context.Users.FindAsync(id); if (user == null) return NotFound(); return user; }

        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] User updated) { var user = await _context.Users.FindAsync(id); if (user == null) return NotFound(); user.NickName = updated.NickName; user.About = updated.About; user.ProfileIcon = updated.ProfileIcon; await _context.SaveChangesAsync(); return Ok(user); }

        // LİSTE GETİRME (GİZLENENLER FİLTRELENDİ)
        [HttpGet("{myId}")]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetMyContacts(int myId)
        {
            var summaryList = new List<UserSummaryDto>();
            var now = DateTime.Now;
            
            // GİZLİ LİSTE
            var me = await _context.Users.FindAsync(myId);
            var hiddenIds = me?.HiddenMessageIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList() ?? new List<int>();

            // 1. İletildi Yap
            var pendingDelivery = await _context.Messages.Where(m => m.ReceiverId == myId && !m.IsDelivered && (m.ScheduledTime == null || m.ScheduledTime <= now)).ToListAsync();
            if (pendingDelivery.Any()) { foreach (var msg in pendingDelivery) msg.IsDelivered = true; await _context.SaveChangesAsync(); }

            // 2. Kişiler
            var allUsers = await _context.Users.Where(u => u.Id != myId).ToListAsync();
            foreach (var user in allUsers)
            {
                var lastMsg = await _context.Messages
                    .Where(m => m.GroupId == null && ((m.SenderId == myId && m.ReceiverId == user.Id) || (m.SenderId == user.Id && m.ReceiverId == myId)))
                    .Where(m => m.ScheduledTime == null || m.ScheduledTime <= now)
                    .Where(m => !hiddenIds.Contains(m.Id)) // FİLTRE
                    .OrderByDescending(m => m.ScheduledTime ?? m.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastMsg == null) continue;

                var unreadCount = await _context.Messages.Where(m => m.GroupId == null && m.SenderId == user.Id && m.ReceiverId == myId && !m.IsRead).Where(m => m.ScheduledTime == null || m.ScheduledTime <= now).Where(m => !hiddenIds.Contains(m.Id)).CountAsync();

                summaryList.Add(new UserSummaryDto { Id = user.Id, Name = user.NickName.StartsWith("@") ? user.NickName : user.Username, LastMessage = lastMsg.IsDeletedForEveryone ? "🚫 Silindi" : lastMsg.EncryptedContent, UnreadCount = unreadCount, LastMessageTime = lastMsg.ScheduledTime ?? lastMsg.CreatedAt, LastMessageSenderId = lastMsg.SenderId, LastMessageIsDelivered = lastMsg.IsDelivered, LastMessageIsRead = lastMsg.IsRead, IsGroup = false, ProfileIcon = user.ProfileIcon });
            }

            // 3. Gruplar
            var myMemberships = await _context.GroupMembers.Where(gm => gm.UserId == myId).ToListAsync();
            foreach (var member in myMemberships)
            {
                var group = await _context.Groups.FindAsync(member.GroupId);
                if (group == null) continue;

                var lastMsg = await _context.Messages
                    .Where(m => m.GroupId == group.Id)
                    .Where(m => m.ScheduledTime == null || m.ScheduledTime <= now)
                    .Where(m => !hiddenIds.Contains(m.Id)) // FİLTRE
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                var groupUnreadCount = await _context.Messages.Where(m => m.GroupId == group.Id && m.SenderId != myId && !m.IsRead).Where(m => m.ScheduledTime == null || m.ScheduledTime <= now).Where(m => !hiddenIds.Contains(m.Id)).CountAsync();

                summaryList.Add(new UserSummaryDto { Id = group.Id, Name = group.GroupName, LastMessage = lastMsg != null ? (lastMsg.IsDeletedForEveryone ? "🚫 Silindi" : lastMsg.EncryptedContent) : "Grup kuruldu", UnreadCount = groupUnreadCount, LastMessageTime = lastMsg?.CreatedAt ?? group.CreatedAt, LastMessageSenderId = lastMsg?.SenderId ?? 0, IsGroup = true, ProfileIcon = group.GroupIcon });
            }
            return summaryList.OrderByDescending(u => u.LastMessageTime).ToList();
        }
    }
}