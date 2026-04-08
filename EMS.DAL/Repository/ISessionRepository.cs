using EMS.DAL.Models;

namespace EMS.DAL.Repository
{
    public interface ISessionRepository : IGenericRepository<SessionInfo>
    {
        Task<IEnumerable<SessionInfo>> GetSessionsByEventAsync(Guid eventId);
        Task<bool> AssignSpeakerAsync(Guid sessionId, Guid speakerId);
        Task<IEnumerable<SessionInfo>> GetUpcomingSessionsAsync();
    }
}