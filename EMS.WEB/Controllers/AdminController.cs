using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.DAL.Models;
using EMS.DAL.Data;

namespace EMS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== DASHBOARD ====================
        
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalEvents = await _context.Events.CountAsync();
            ViewBag.TotalSessions = await _context.Sessions.CountAsync();
            ViewBag.TotalSpeakers = await _context.Speakers.CountAsync();
            ViewBag.TotalParticipants = await _context.Users.CountAsync(u => u.Role == "Participant");
            ViewBag.ActiveEvents = await _context.Events.CountAsync(e => e.Status == "Active");
            return View();
        }

        // ==================== EVENT MANAGEMENT ====================

        public async Task<IActionResult> Events()
        {
            var events = await _context.Events
                .Include(e => e.Sessions)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();
            return View(events);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventDetails eventEntity)
        {
            if (ModelState.IsValid)
            {
                if (eventEntity.EventDate <= DateTime.UtcNow)
                {
                    ModelState.AddModelError("EventDate", "Event date must be in the future");
                    return View(eventEntity);
                }

                eventEntity.EventId = Guid.NewGuid();
                _context.Events.Add(eventEntity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction("Events");
            }
            return View(eventEntity);
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(Guid id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null) return NotFound();
            return View(eventEntity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EventDetails eventEntity)
        {
            if (ModelState.IsValid)
            {
                _context.Update(eventEntity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event updated successfully!";
                return RedirectToAction("Events");
            }
            return View(eventEntity);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity != null)
            {
                _context.Events.Remove(eventEntity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event deleted successfully!";
            }
            return RedirectToAction("Events");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleEventStatus(Guid id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity != null)
            {
                eventEntity.Status = eventEntity.Status == "Active" ? "In-Active" : "Active";
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Event status changed to {eventEntity.Status}!";
            }
            return RedirectToAction("Events");
        }

        // ==================== SESSION MANAGEMENT ====================

        public async Task<IActionResult> Sessions()
        {
            var sessions = await _context.Sessions
                .Include(s => s.Event)
                .Include(s => s.Speaker)
                .OrderBy(s => s.SessionStart)
                .ToListAsync();
            return View(sessions);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSession()
        {
            ViewBag.Events = await _context.Events.Where(e => e.Status == "Active").ToListAsync();
            ViewBag.Speakers = await _context.Speakers.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSession([Bind("EventId,SessionTitle,SpeakerId,Description,SessionStart,SessionEnd,SessionUrl")] SessionInfo session)
        {
            // Remove validation for auto-generated fields
            ModelState.Remove("SessionId");
            ModelState.Remove("Event");
            ModelState.Remove("Speaker");

            // Convert dates to UTC to avoid timezone issues
            session.SessionStart = DateTime.SpecifyKind(session.SessionStart, DateTimeKind.Utc);
            session.SessionEnd = DateTime.SpecifyKind(session.SessionEnd, DateTimeKind.Utc);

            if (ModelState.IsValid)
            {
                // Validate session times
                if (session.SessionStart >= session.SessionEnd)
                {
                    ModelState.AddModelError("SessionEnd", "Session end time must be after start time");
                    ViewBag.Events = await _context.Events.Where(e => e.Status == "Active").ToListAsync();
                    ViewBag.Speakers = await _context.Speakers.ToListAsync();
                    return View(session);
                }

                // Validate event exists
                var eventExists = await _context.Events.AnyAsync(e => e.EventId == session.EventId);
                if (!eventExists)
                {
                    ModelState.AddModelError("EventId", "Selected event does not exist");
                    ViewBag.Events = await _context.Events.Where(e => e.Status == "Active").ToListAsync();
                    ViewBag.Speakers = await _context.Speakers.ToListAsync();
                    return View(session);
                }

                try
                {
                    session.SessionId = Guid.NewGuid();
                    _context.Sessions.Add(session);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Session '{session.SessionTitle}' created successfully!";
                    return RedirectToAction("Sessions");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating session: {ex.Message}";
                    ViewBag.Events = await _context.Events.Where(e => e.Status == "Active").ToListAsync();
                    ViewBag.Speakers = await _context.Speakers.ToListAsync();
                    return View(session);
                }
            }

            ViewBag.Events = await _context.Events.Where(e => e.Status == "Active").ToListAsync();
            ViewBag.Speakers = await _context.Speakers.ToListAsync();
            return View(session);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSession(Guid id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session != null)
            {
                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Session deleted successfully!";
            }
            return RedirectToAction("Sessions");
        }

        // ==================== SPEAKER MANAGEMENT ====================

        public async Task<IActionResult> Speakers()
        {
            var speakers = await _context.Speakers.Include(s => s.Sessions).ToListAsync();
            return View(speakers);
        }

        [HttpGet]
        public IActionResult CreateSpeaker()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpeaker(SpeakersDetails speaker)
        {
            if (ModelState.IsValid)
            {
                speaker.SpeakerId = Guid.NewGuid();
                _context.Speakers.Add(speaker);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Speaker added successfully!";
                return RedirectToAction("Speakers");
            }
            return View(speaker);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSpeaker(Guid id)
        {
            var speaker = await _context.Speakers.FindAsync(id);
            if (speaker != null)
            {
                _context.Speakers.Remove(speaker);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Speaker deleted successfully!";
            }
            return RedirectToAction("Speakers");
        }

        // ==================== ASSIGN SPEAKER ====================

        [HttpGet]
        public async Task<IActionResult> AssignSpeaker()
        {
            var sessions = await _context.Sessions
                .Include(s => s.Event)
                .Include(s => s.Speaker)
                .OrderByDescending(s => s.SessionStart)
                .ToListAsync();
            
            var speakers = await _context.Speakers.OrderBy(s => s.SpeakerName).ToListAsync();
            
            ViewBag.Sessions = sessions;
            ViewBag.Speakers = speakers;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSpeaker(Guid sessionId, Guid speakerId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session != null)
            {
                session.SpeakerId = speakerId;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Speaker assigned successfully!";
            }
            return RedirectToAction("Sessions");
        }
    }
}