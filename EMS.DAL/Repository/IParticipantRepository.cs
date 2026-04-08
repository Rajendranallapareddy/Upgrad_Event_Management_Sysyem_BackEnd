using EMS.DAL.Models;

namespace EMS.DAL.Repository
{
    public interface IParticipantRepository : IGenericRepository<ParticipantEventDetails>
    {
        Task<IEnumerable<EventDetails>> GetRegisteredEventsByParticipantAsync(string email);
        Task<bool> RegisterForEventAsync(string email, Guid eventId);
        Task<bool> MarkAttendanceAsync(Guid participantEventId);
        Task<bool> IsParticipantRegisteredAsync(string email, Guid eventId);
        Task<IEnumerable<ParticipantEventDetails>> GetParticipantRegistrationsAsync(string email);
    }
}