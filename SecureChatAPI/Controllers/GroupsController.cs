using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureChatAPI.Models;

namespace SecureChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly ChatContext _context;
        public GroupsController(ChatContext context) { _context = context; }

        [HttpPost("create")]
        public async Task<ActionResult> CreateGroup([FromBody] GroupRequestDto req)
        {
            var group = new Group { GroupName = req.GroupName, CreatorId = req.CreatorId, Description = "Yeni Grup", GroupIcon = req.GroupIcon ?? "group" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _context.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = req.CreatorId, IsAdmin = true });
            foreach(var userId in req.MemberIds) { _context.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = userId }); }
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return NotFound();
            return group;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] Group updated)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return NotFound();
            
            group.GroupName = updated.GroupName;
            group.Description = updated.Description;
            group.GroupIcon = updated.GroupIcon; // FOTOĞRAF GÜNCELLEME
            
            await _context.SaveChangesAsync();
            return Ok(group);
        }
    }

    public class GroupRequestDto
    {
        public string GroupName { get; set; } = string.Empty;
        public string? GroupIcon { get; set; }
        public int CreatorId { get; set; }
        public List<int> MemberIds { get; set; } = new List<int>();
    }
}