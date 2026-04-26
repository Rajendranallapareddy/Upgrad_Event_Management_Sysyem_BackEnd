using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.DAL.Data;
using EMS.DAL.Models;
using System.Security.Claims;

namespace EMS.Web.Controllers
{
    [Authorize(Roles = "Participant")]
    public class ParticipantController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParticipantController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var registrations = await _context.ParticipantEvents
                .Include(pe => pe.Event)
                .ThenInclude(e => e.Sessions)
                .Where(pe => pe.ParticipantEmailId == email)
                .OrderByDescending(pe => pe.Event.EventDate)
                .ToListAsync();

            return View(registrations);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterForEvent(Guid eventId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var isRegistered = await _context.ParticipantEvents
                .AnyAsync(pe => pe.ParticipantEmailId == email && pe.EventId == eventId);

            if (!isRegistered)
            {
                var registration = new ParticipantEventDetails
                {
                    ParticipantEmailId = email,
                    EventId = eventId,
                    IsAttended = false
                };
                _context.ParticipantEvents.Add(registration);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Successfully registered for the event!";
            }
            else
            {
                TempData["Error"] = "You are already registered for this event.";
            }

            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> MyEvents()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var registrations = await _context.ParticipantEvents
                .Include(pe => pe.Event)
                .ThenInclude(e => e.Sessions)
                .Where(pe => pe.ParticipantEmailId == email)
                .OrderByDescending(pe => pe.Event.EventDate)
                .ToListAsync();

            return View(registrations);
        }

        [HttpPost]
        public async Task<IActionResult> CancelRegistration(Guid registrationId)
        {
            var registration = await _context.ParticipantEvents.FindAsync(registrationId);
            if (registration != null)
            {
                _context.ParticipantEvents.Remove(registration);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Registration cancelled successfully!";
            }
            return RedirectToAction("MyEvents");
        }
    }
}