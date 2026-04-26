using Microsoft.EntityFrameworkCore;
using EMS.DAL.Models;
using EMS.DAL.Data;  // ← ADD THIS LINE

namespace EMS.DAL.Repository
{
    public class EventRepository : GenericRepository<EventDetails>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<EventDetails>> GetActiveEventsAsync()
        {
            return await _dbSet
                .Where(e => e.Status == "Active" && e.EventDate >= DateTime.Now)
                .Include(e => e.Sessions)
                .ThenInclude(s => s.Speaker)
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventDetails>> GetEventsByCategoryAsync(string category)
        {
            return await _dbSet
                .Where(e => e.EventCategory == category && e.Status == "Active")
                .Include(e => e.Sessions)
                .ToListAsync();
        }

        public async Task<bool> ToggleEventStatusAsync(Guid eventId)
        {
            var eventEntity = await GetByIdAsync(eventId);
            if (eventEntity != null)
            {
                eventEntity.Status = eventEntity.Status == "Active" ? "In-Active" : "Active";
                await UpdateAsync(eventEntity);
                return true;
            }
            return false;
        }
    }
}