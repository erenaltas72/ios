using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SecureChatAPI.Models;

namespace SecureChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ChatContext _context;
        public MessagesController(ChatContext context) { _context = context; }

        // 1. Sohbeti Getir (GİZLENENLERİ FİLTRELE)
        [HttpGet("conversation")]
        public async Task<ActionResult<IEnumerable<MessageDetailDto>>> GetConversation([FromQuery] int myId, [FromQuery] int targetId, [FromQuery] bool isGroup)
        {
            // Önce kullanıcının gizlediği mesaj ID'lerini alalım
            var me = await _context.Users.FindAsync(myId);
            var hiddenIds = me?.HiddenMessageIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList() ?? new List<int>();

            var now = DateTime.Now;
            List<Message> messages;

            if (isGroup)
            {
                // Grup OKUNDU işaretleme
                var unreadGroupMsgs = await _context.Messages
                    .Where(m => m.GroupId == targetId && m.SenderId != myId && !m.IsRead)
                    .Where(m => m.ScheduledTime == null || m.ScheduledTime <= now)
                    .ToListAsync();

                if (unreadGroupMsgs.Any()) { 
                    foreach (var m in unreadGroupMsgs) { m.IsRead = true; m.IsDelivered = true; } 
                    await _context.SaveChangesAsync(); 
                }

                messages = await _context.Messages
                    .Where(m => m.GroupId == targetId)
                    .Where(m => m.ScheduledTime == null || m.ScheduledTime <= now)
                    // GİZLENENLERİ GETİRME
                    .Where(m => !hiddenIds.Contains(m.Id)) 
                    .OrderBy(m => m.ScheduledTime ?? m.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                var unread = await _context.Messages
                    .Where(m => m.GroupId == null && m.SenderId == targetId && m.ReceiverId == myId && !m.IsRead && (m.ScheduledTime == null || m.ScheduledTime <= now))
                    .ToListAsync();
                if (unread.Any()) { foreach (var m in unread) { m.IsDelivered = true; m.IsRead = true; } await _context.SaveChangesAsync(); }

                messages = await _context.Messages
                    .Where(m => m.GroupId == null && 
                        ((m.SenderId == myId && m.ReceiverId == targetId) || (m.SenderId == targetId && m.ReceiverId == myId)))
                    .Where(m => m.SenderId == myId || (m.SenderId == targetId && (m.ScheduledTime == null || m.ScheduledTime <= now)))
                    // GİZLENENLERİ GETİRME
                    .Where(m => !hiddenIds.Contains(m.Id))
                    .OrderBy(m => m.ScheduledTime ?? m.CreatedAt)
                    .ToListAsync();
            }

            var result = new List<MessageDetailDto>();
            foreach (var m in messages)
            {
                var sender = await _context.Users.FindAsync(m.SenderId);
                result.Add(new MessageDetailDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = sender?.Username ?? "Bilinmiyor",
                    SenderIcon = sender?.ProfileIcon ?? "person",
                    EncryptedContent = m.EncryptedContent,
                    CreatedAt = m.CreatedAt,
                    ScheduledTime = m.ScheduledTime,
                    IsDelivered = m.IsDelivered,
                    IsRead = m.IsRead,
                    IsDeletedForEveryone = m.IsDeletedForEveryone,
                    StarTag = m.StarTag,
                    IsPinned = m.IsPinned
                });
            }
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<Message>> PostMessage(Message message)
        {
            message.CreatedAt = DateTime.Now;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(message);
        }

        // 3. Mesaj Sil (GÜNCELLENDİ: GRUP İÇİN BENDEN SİL ÇALIŞIR)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, [FromQuery] int requestUserId, [FromQuery] string mode)
        {
            var msg = await _context.Messages.FindAsync(id);
            if (msg == null) return NotFound();

            if (mode == "everyone")
            {
                // Herkesten sil: Sadece gönderen yapabilir
                if (msg.SenderId == requestUserId) {
                    msg.IsDeletedForEveryone = true;
                    msg.EncryptedContent = "";
                    await _context.SaveChangesAsync();
                } else {
                    return BadRequest("Başkasına ait mesajı herkesten silemezsin.");
                }
            }
            else // "me" (Benden Sil)
            {
                // Mesajı silmek yerine kullanıcının "Gizli Listesine" ekliyoruz
                var user = await _context.Users.FindAsync(requestUserId);
                if (user != null)
                {
                    user.HiddenMessageIds += id + ","; // ID'yi listeye ekle
                    await _context.SaveChangesAsync();
                }
            }
            return NoContent();
        }

        [HttpPut("star/{id}")]
        public async Task<IActionResult> StarMessage(int id, [FromQuery] string? tag)
        {
            var msg = await _context.Messages.FindAsync(id);
            if (msg == null) return NotFound();
            msg.StarTag = tag;
            await _context.SaveChangesAsync();
            return Ok(msg);
        }

        [HttpPut("pin/{id}")]
        public async Task<IActionResult> PinMessage(int id)
        {
            var msg = await _context.Messages.FindAsync(id);
            if (msg == null) return NotFound();
            msg.IsPinned = !msg.IsPinned;
            await _context.SaveChangesAsync();
            return Ok(msg.IsPinned);
        }

        [HttpGet("starred/{userId}")]
        public async Task<ActionResult<IEnumerable<StarredMessageDto>>> GetStarredMessages(int userId)
        {
            // Yıldızlılarda da silinenleri gösterme
            var me = await _context.Users.FindAsync(userId);
            var hiddenIds = me?.HiddenMessageIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList() ?? new List<int>();

            var myGroupIds = await _context.GroupMembers.Where(gm => gm.UserId == userId).Select(gm => gm.GroupId).ToListAsync();
            var messages = await _context.Messages
                .Where(m => m.StarTag != null && !m.IsDeletedForEveryone)
                .Where(m => !hiddenIds.Contains(m.Id)) // FİLTRE
                .Where(m => (m.SenderId == userId) || (m.ReceiverId == userId) || (m.GroupId != null && myGroupIds.Contains(m.GroupId.Value)))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var dtos = new List<StarredMessageDto>();
            foreach(var m in messages)
            {
                var sender = await _context.Users.FindAsync(m.SenderId);
                string? groupName = null;
                if (m.GroupId != null) { var grp = await _context.Groups.FindAsync(m.GroupId); groupName = grp?.GroupName; }
                dtos.Add(new StarredMessageDto { Id = m.Id, SenderName = sender?.Username ?? "Bilinmiyor", GroupName = groupName, GroupId = m.GroupId, EncryptedContent = m.EncryptedContent, CreatedAt = m.CreatedAt, StarTag = m.StarTag, SenderId = m.SenderId, ReceiverId = m.ReceiverId ?? 0 });
            }
            return dtos;
        }
    }
}