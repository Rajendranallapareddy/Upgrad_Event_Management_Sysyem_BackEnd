using Microsoft.EntityFrameworkCore;
using EMS.DAL.Models;
using EMS.DAL.Data;  // ← ADD THIS LINE

namespace EMS.DAL.Repository
{
    public class ParticipantRepository : GenericRepository<ParticipantEventDetails>, IParticipantRepository
    {
        public ParticipantRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<EventDetails>> GetRegisteredEventsByParticipantAsync(string email)
        {
            var participantEvents = await _dbSet
                .Where(pe => pe.ParticipantEmailId == email)
                .Include(pe => pe.Event)
                .ThenInclude(e => e.Sessions)
                .ToListAsync();

            return participantEvents.Select(pe => pe.Event);
        }

        public async Task<bool> RegisterForEventAsync(string email, Guid eventId)
        {
            if (await IsParticipantRegisteredAsync(email, eventId))
                return false;

            var registration = new ParticipantEventDetails
            {
                ParticipantEmailId = email,
                EventId = eventId,
                IsAttended = false
            };

            await AddAsync(registration);
            return true;
        }

        public async Task<bool> MarkAttendanceAsync(Guid participantEventId)
        {
            var registration = await GetByIdAsync(participantEventId);
            if (registration != null)
            {
                registration.IsAttended = true;
                await UpdateAsync(registration);
                return true;
            }
            return false;
        }

        public async Task<bool> IsParticipantRegisteredAsync(string email, Guid eventId)
        {
            return await _dbSet.AnyAsync(pe => pe.ParticipantEmailId == email && pe.EventId == eventId);
        }

        public async Task<IEnumerable<ParticipantEventDetails>> GetParticipantRegistrationsAsync(string email)
        {
            return await _dbSet
                .Where(pe => pe.ParticipantEmailId == email)
                .Include(pe => pe.Event)
                .ThenInclude(e => e.Sessions)
                .OrderByDescending(pe => pe.Event.EventDate)
                .ToListAsync();
        }
    }
}