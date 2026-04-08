using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.DAL.Data;
using EMS.DAL.Models;

namespace EMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Where(e => e.Status == "Active" && e.EventDate >= DateTime.Now)
                .Include(e => e.Sessions)
                .ThenInclude(s => s.Speaker)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            ViewBag.Categories = await _context.Events
                .Where(e => e.Status == "Active")
                .Select(e => e.EventCategory)
                .Distinct()
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> EventDetails(Guid id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Sessions)
                .ThenInclude(s => s.Speaker)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
                return NotFound();

            return View(eventEntity);
        }

        public async Task<IActionResult> SessionDetails(Guid id)
        {
            var session = await _context.Sessions
                .Include(s => s.Event)
                .Include(s => s.Speaker)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null)
                return NotFound();

            return View(session);
        }

        public async Task<IActionResult> EventsByCategory(string category)
        {
            var events = await _context.Events
                .Where(e => e.EventCategory == category && e.Status == "Active" && e.EventDate >= DateTime.Now)
                .Include(e => e.Sessions)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            ViewBag.Category = category;
            return View(events);
        }
    }
}