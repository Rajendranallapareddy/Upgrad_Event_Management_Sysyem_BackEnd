using EMS.DAL.Models;

namespace EMS.DAL.Repository
{
    public interface IEventRepository : IGenericRepository<EventDetails>
    {
        Task<IEnumerable<EventDetails>> GetActiveEventsAsync();
        Task<IEnumerable<EventDetails>> GetEventsByCategoryAsync(string category);
        Task<bool> ToggleEventStatusAsync(Guid eventId);
    }
}