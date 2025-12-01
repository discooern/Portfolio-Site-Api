using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Models;

namespace PortfolioAPI.Controllers
{
    [Route("eventlogs")]
    [ApiController]
    public class EventLogController : Controller
    {
        private readonly PortfolioDbContext _context;

        public EventLogController(PortfolioDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var result = await _context.EventLogs.ToListAsync();

            return Ok(result);
        }
    }
}
