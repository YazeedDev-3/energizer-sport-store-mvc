using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_web2.Data;
using project_web2.Models;

namespace project_web2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class getnameController : ControllerBase
    {
        private readonly project_web2Context _context;

        public getnameController(project_web2Context context)
        {
            _context = context;
        }

        [HttpGet("{cat}")]
        public IEnumerable<allUsers> Get(string cat)
        {
            return _context.Users
                .Where(u => u.Role == cat)
                .Select(u => new allUsers { name = u.Username })
                .ToList();
        }
    }
}
