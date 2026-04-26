using Microsoft.EntityFrameworkCore;
using EMS.DAL.Models;
using EMS.DAL.Data;  // ← ADD THIS LINE

namespace EMS.DAL.Repository
{
    public class SessionRepository : GenericRepository<SessionInfo>, ISessionRepository
    {
        public SessionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SessionInfo>> GetSessionsByEventAsync(Guid eventId)
        {
            return await _dbSet
                .Where(s => s.EventId == eventId)
                .Include(s => s.Speaker)
                .Include(s => s.Event)
                .OrderBy(s => s.SessionStart)
                .ToListAsync();
        }

        public async Task<bool> AssignSpeakerAsync(Guid sessionId, Guid speakerId)
        {
            var session = await GetByIdAsync(sessionId);
            if (session != null)
            {
                session.SpeakerId = speakerId;
                await UpdateAsync(session);
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<SessionInfo>> GetUpcomingSessionsAsync()
        {
            return await _dbSet
                .Where(s => s.SessionStart >= DateTime.Now)
                .Include(s => s.Event)
                .Include(s => s.Speaker)
                .OrderBy(s => s.SessionStart)
                .ToListAsync();
        }
    }
}